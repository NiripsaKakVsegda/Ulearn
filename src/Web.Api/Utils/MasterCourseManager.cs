using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Ionic.Zip;
using Microsoft.Extensions.DependencyInjection;
using Ulearn.Common;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Helpers;
using Vostok.Logging.Abstractions;

namespace Ulearn.Web.Api.Utils
{
	public class MasterCourseManager : SlaveCourseManager, IMasterCourseManager
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(MasterCourseManager));

		private readonly IServiceScopeFactory serviceScopeFactory;
		private readonly ExerciseStudentZipsCache exerciseStudentZipsCache;

		public MasterCourseManager(IServiceScopeFactory serviceScopeFactory, ExerciseStudentZipsCache exerciseStudentZipsCache)
			: base(serviceScopeFactory)
		{
			this.serviceScopeFactory = serviceScopeFactory;
			this.exerciseStudentZipsCache = exerciseStudentZipsCache;

			CourseStorageInstance.CourseChangedEvent += courseId =>
			{
				exerciseStudentZipsCache.DeleteCourseZips(courseId);
				ExerciseCheckerZipsCache.DeleteCourseZips(courseId);
			};
		}

		// Невременные курсы не выкладываются на диск сразу, а публикуются в базу и UpdateCourses их обновляет на диске.
		public override async Task UpdateCourses()
		{
			using (var scope = serviceScopeFactory.CreateScope())
			{
				var coursesRepo = scope.ServiceProvider.GetService<ICoursesRepo>();
				var publishedCourseVersions = await coursesRepo.GetPublishedCourseVersions();
				foreach (var publishedCourseVersion in publishedCourseVersions)
				{
					await UpdateCourseToVersionInDirectory(publishedCourseVersion, coursesRepo);
				}
			}
		}

		private async Task UpdateCourseToVersionInDirectory(CourseVersion publishedCourseVersions, ICoursesRepo coursesRepo)
		{
			var courseId = publishedCourseVersions.CourseId;
			var publishedVersionToken = new CourseVersionToken(publishedCourseVersions.Id);
			if (BrokenVersions.ContainsKey(publishedVersionToken))
				return;
			var courseInMemory = CourseStorageInstance.FindCourse(courseId);
			if (courseInMemory != null && courseInMemory.CourseVersionToken == publishedVersionToken)
				return;

			if (courseInMemory == null) // Проверяем, вдруг на диске актуальная версия
			{
				await UpdateCourseOrTempCourseToVersionFromDirectory(courseId, publishedVersionToken);
				courseInMemory = CourseStorageInstance.FindCourse(courseId);
				if (courseInMemory != null && courseInMemory.CourseVersionToken == publishedVersionToken)
					return;
			}

			var courseFile = await coursesRepo.GetVersionFile(publishedCourseVersions.Id);
			await UpdateCourseInCommonDirectory(courseId, courseFile.File, publishedVersionToken);
		}

		private async Task UpdateCourseInCommonDirectory(string courseId, byte[] content, CourseVersionToken publishedVersionToken)
		{
			using (var courseDirectory = await ExtractCourseVersionToTemporaryDirectory(courseId, publishedVersionToken, content))
			{
				var (course, error) = LoadCourseFromDirectory(courseId, courseDirectory.DirectoryInfo);
				if (error != null)
				{
					BrokenVersions.TryAdd(publishedVersionToken, true);
					var message = $"Не смог загрузить с диска в память курс {courseId} версии {publishedVersionToken}";
					log.Error(error, message);
					return;
				}

				using (await CourseLock.AcquireWriterLock(courseId))
				{
					try
					{
						MoveCourse(course, courseDirectory.DirectoryInfo, GetExtractedCourseDirectory(courseId));
						CourseStorageUpdaterInstance.AddOrUpdateCourse(course);
					}
					catch (Exception ex)
					{
						log.Error(ex, $"Не смог переместить курс {courseId} версия {publishedVersionToken} из временной папки в общую");
					}
				}
			}
		}

		// Временные курсы выкладываются на диск сразу контроллером, который их создает, здесь только загружаю курсы с диска
		// Это актуально при старте, а в дальнейшем должен находить курсы на диске уже актуальной версии
		public override async Task UpdateTempCourses()
		{
			await base.UpdateTempCourses();
		}

		public async Task<FileInfo> GenerateOrFindStudentZip(string courseId, Slide slide)
		{
			return await exerciseStudentZipsCache.GenerateOrFindZip(courseId, slide, GetExtractedCourseDirectory(courseId).FullName);
		}

#region CreateAndUpdateTempCourses

		public string GetTempCourseId(string baseCourseId, string userId)
		{
			return $"{baseCourseId}_{userId}";
		}

		public async Task<TempCourse> CreateTempCourse(string baseCourseId, string userId)
		{
			var tempCourseId = GetTempCourseId(baseCourseId, userId);
			using (var scope = serviceScopeFactory.CreateScope())
			{
				var tempCoursesRepo = scope.ServiceProvider.GetService<ITempCoursesRepo>();
				var courseRolesRepo = scope.ServiceProvider.GetService<ICourseRolesRepo>();
				var coursesRepo = scope.ServiceProvider.GetService<ICoursesRepo>();

				var tmpCourseDbData = await tempCoursesRepo.FindAsync(tempCourseId);
				if (tmpCourseDbData != null)
				{
					log.Warn($"Временный курс {tempCourseId} уже существует в базе");

					var course = CourseStorageInstance.FindCourse(tempCourseId);
					if (course != null && course.CourseVersionToken.LoadingTime == tmpCourseDbData.LoadingTime)
					{
						log.Warn($"Временный курс {tempCourseId} версии {course.CourseVersionToken} уже загружен в память");
						return tmpCourseDbData;
					}
				}

				var loadingTime = DateTime.Now;
				var versionToken = new CourseVersionToken(loadingTime);
				var baseCourseVersionFile = await coursesRepo.GetPublishedVersionFile(baseCourseId);
				await UpdateCourseInCommonDirectory(tempCourseId, baseCourseVersionFile.File, versionToken);

				if (tmpCourseDbData == null)
				{
					tmpCourseDbData = await tempCoursesRepo.AddTempCourseAsync(tempCourseId, userId, loadingTime);
					await courseRolesRepo.ToggleRole(tempCourseId, userId, CourseRoleType.CourseAdmin, userId, "Создал временный курс");
					return tmpCourseDbData;
				}

				await tempCoursesRepo.UpdateTempCourseLoadingTimeAsync(tempCourseId, loadingTime);
				return await tempCoursesRepo.FindAsync(tempCourseId);
			}
		}

		public async Task<(TempCourse Course, string Error)> UpdateTempCourseFromStream(string tmpCourseId, Stream zipContent, bool isFullCourse)
		{
			using (var scope = serviceScopeFactory.CreateScope())
			{
				var loadingTime = DateTime.Now;
				var tempCoursesRepo = scope.ServiceProvider.GetService<ITempCoursesRepo>();
				UploadChanges(tmpCourseId, zipContent);

				var filesToDelete = ExtractFileNamesToDelete(tmpCourseId);

				var error = await TryPublishChanges(tmpCourseId, filesToDelete, isFullCourse);

				if (error != null)
				{
					await tempCoursesRepo.UpdateOrAddTempCourseErrorAsync(tmpCourseId, error);
					return (null, error);
				}

				await tempCoursesRepo.MarkTempCourseAsNotErroredAsync(tmpCourseId);
				await tempCoursesRepo.UpdateTempCourseLoadingTimeAsync(tmpCourseId, loadingTime);
				var tempCourse = await tempCoursesRepo.FindAsync(tmpCourseId);
				return (tempCourse, null);
			}
		}

		private List<string> ExtractFileNamesToDelete(string tmpCourseId)
		{
			var stagingFile = GetStagingTempCourseFile(tmpCourseId);
			var filesToDelete = new List<string>();
			using (var zip = ZipFile.Read(stagingFile.FullName))
			{
				var e = zip["deleted.txt"];
				if (e is null)
					return new List<string>();
				var r = e.OpenReader();
				using var sr = new StreamReader(r);
				while (!sr.EndOfStream)
				{
					var line = sr.ReadLine();
					if (!string.IsNullOrEmpty(line))
						filesToDelete.Add(line);
				}
			}

			return filesToDelete
				.Select(x =>
				{
					if (x.StartsWith('\\') || x.StartsWith('/'))
						return x.Substring(1);
					return x;
				}).ToList();
		}

		private async Task<string> TryPublishChanges(string courseId, List<string> filesToDelete, bool isFull)
		{
			var revertStructure = GetRevertStructure(courseId, filesToDelete, isFull);
			DeleteFiles(revertStructure.DeletedFiles, revertStructure.DeletedDirectories);
			var courseDirectory = GetExtractedCourseDirectory(courseId);
			DeleteEmptySubdirectories(courseDirectory.FullName);
			ExtractTempCourseChanges(courseId);

			try
			{
				ReloadCourseNotSafe(courseId, notifyAboutErrors: false);
				await UpdateStagingZipFromExtracted(courseId);
			}
			catch (Exception error)
			{
				var errorMessage = error.Message;
				while (error.InnerException != null)
				{
					errorMessage += $"\n\n{error.InnerException.Message}";
					error = error.InnerException;
				}

				revertStructure.Revert();
				return errorMessage;
			}

			return null;
		}

		private static void DeleteFiles(List<FileContent> filesToDelete, List<string> directoriesToDelete)
		{
			filesToDelete.ForEach(file => File.Delete(file.Path));
			directoriesToDelete.ForEach(DeleteNotEmptyDirectory);
		}

		private async Task UpdateStagingZipFromExtracted(string courseId)
		{
			var courseDirectory = GetExtractedCourseDirectory(courseId);
			var stagingFile = GetStagingCourseFile(courseId);
			var stream = ZipUtils.CreateZipFromDirectory(new List<string> { courseDirectory.FullName }, null, null);
			await using (var fs = stagingFile.Open(FileMode.Create, FileAccess.Write))
				await stream.CopyToAsync(fs);
		}

		private static void DeleteNotEmptyDirectory(string dirPath)
		{
			var files = Directory.GetFiles(dirPath);
			var dirs = Directory.GetDirectories(dirPath);

			foreach (var file in files)
			{
				File.SetAttributes(file, FileAttributes.Normal);
				File.Delete(file);
			}

			foreach (string dir in dirs)
			{
				DeleteNotEmptyDirectory(dir);
			}

			Directory.Delete(dirPath, false);
		}

		private void DeleteEmptySubdirectories(string startLocation)
		{
			foreach (var directory in Directory.GetDirectories(startLocation))
			{
				DeleteEmptySubdirectories(directory);
				if (Directory.GetFiles(directory).Length == 0 &&
					Directory.GetDirectories(directory).Length == 0)
				{
					Directory.Delete(directory, false);
				}
			}
		}

		private RevertStructure GetRevertStructure(string courseId, List<string> filesToDeleteRelativePaths, bool isFull)
		{
			var staging = GetStagingTempCourseFile(courseId);
			var courseDirectory = GetExtractedCourseDirectory(courseId);
			var pathPrefix = courseDirectory.FullName;
			var filesInDirectoriesToDelete = GetFilesInDirectoriesToDelete(filesToDeleteRelativePaths, pathPrefix);
			filesToDeleteRelativePaths.AddRange(filesInDirectoriesToDelete);
			var zip = ZipFile.Read(staging.FullName, new ReadOptions { Encoding = Encoding.UTF8 });
			var filesToChangeRelativePaths = zip.Entries
				.Where(x => !x.IsDirectory)
				.Select(x => x.FileName)
				.Select(x => x.Replace('/', '\\'))
				.ToList();
			var courseFileRelativePaths = Directory
				.EnumerateFiles(courseDirectory.FullName, "*.*", SearchOption.AllDirectories)
				.Select(file => TrimPrefix(file, pathPrefix))
				.ToHashSet();
			var revertStructure = GetRevertStructure(pathPrefix, filesToDeleteRelativePaths, filesToChangeRelativePaths, courseFileRelativePaths, isFull);
			return revertStructure;
		}

		private static List<string> GetFilesInDirectoriesToDelete(List<string> filesToDeleteRelativePaths, string pathPrefix)
		{
			return filesToDeleteRelativePaths
				.Select(path => Path.Combine(pathPrefix, path))
				.Where(Directory.Exists)
				.SelectMany(dir => Directory
					.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories))
				.Select(path => TrimPrefix(path, pathPrefix))
				.ToList();
		}

		private static string TrimPrefix(string text, string prefix)
		{
			return text.Substring(text.IndexOf(prefix) + prefix.Length + 1);
		}

		private static RevertStructure GetRevertStructure(
			string pathPrefix,
			List<string> filesToDeleteRelativePaths,
			List<string> filesToChangeRelativePaths,
			HashSet<string> courseFileRelativePaths,
			bool isFull)
		{
			if (isFull)
			{
				filesToDeleteRelativePaths.Clear();
				filesToDeleteRelativePaths.AddRange(courseFileRelativePaths);
			}

			var deletedFiles = filesToDeleteRelativePaths
				.Where(courseFileRelativePaths.Contains)
				.Select(relativePath => Path.Combine(pathPrefix, relativePath))
				.Select(path => new FileContent { Path = path, Data = File.ReadAllBytes(path) })
				.ToList();
			var deletedDirectories = GetDeletedDirs(filesToDeleteRelativePaths, pathPrefix);
			return new RevertStructure
			{
				FilesBeforeChanges = filesToChangeRelativePaths
					.Where(courseFileRelativePaths.Contains)
					.Select(path => Path.Combine(pathPrefix, path))
					.Select(path => new FileContent { Path = path, Data = File.ReadAllBytes(path) })
					.ToList(),
				AddedFiles = filesToChangeRelativePaths
					.Where(file => !courseFileRelativePaths.Contains(file))
					.Select(path => Path.Combine(pathPrefix, path))
					.ToList(),
				DeletedFiles = deletedFiles,
				DeletedDirectories = deletedDirectories
			};
		}

		private static List<string> GetDeletedDirs(List<string> filesToDeleteRelativePaths, string pathPrefix)
		{
			return filesToDeleteRelativePaths.Select(path => Path.Combine(pathPrefix, path))
				.Where(path => Directory.Exists(path) &&
								path.StartsWith(pathPrefix) &&
								!path.Contains(".."))
				.ToList();
		}

		private void UploadChanges(string courseId, Stream stream)
		{
			log.Info($"Start upload course '{courseId}'");
			var stagingFile = GetStagingTempCourseFile(courseId);
			using (var file = new FileStream(stagingFile.FullName, FileMode.Create, FileAccess.Write))
				stream.CopyTo(file);
		}

		private class RevertStructure
		{
			public List<FileContent> FilesBeforeChanges = new List<FileContent>();
			public List<string> AddedFiles = new List<string>();
			public List<FileContent> DeletedFiles = new List<FileContent>();
			public List<string> DeletedDirectories = new List<string>();

			public void Revert()
			{
				DeletedFiles.ForEach(file => new FileInfo(file.Path).Directory.Create());

				static void WriteContent(FileContent fileContent)
				{
					var fInfo = new FileInfo(fileContent.Path);
					if (fInfo.Exists && fInfo.Attributes.HasFlag(FileAttributes.Hidden))
						fInfo.Attributes &= ~FileAttributes.Hidden; // WriteAllBytes кидает ошибку при записи в скрытый файл
					File.WriteAllBytes(fileContent.Path, fileContent.Data);
				}

				FilesBeforeChanges.ForEach(WriteContent);
				DeletedFiles.ForEach(WriteContent);
				AddedFiles.ForEach(File.Delete);
			}
		}

#endregion

	}
}