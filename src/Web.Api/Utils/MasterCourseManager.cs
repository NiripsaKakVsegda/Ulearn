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

		public async Task<(TempCourse Course, string Error)> UpdateTempCourseFromStream(string tempCourseId, Stream zipContent, bool isFullCourse)
		{
			using (var scope = serviceScopeFactory.CreateScope())
			{
				var loadingTime = DateTime.Now;
				var tempCoursesRepo = scope.ServiceProvider.GetService<ITempCoursesRepo>();

				using (CourseLock.AcquireWriterLock(tempCourseId))
				{
					var stagingTempCourseFile = GetStagingTempCourseFile(tempCourseId);
					using (var fs = new FileStream(stagingTempCourseFile.FullName, FileMode.Create, FileAccess.Write))
						await zipContent.CopyToAsync(fs);

					var (course, exception) = await TryUpdateCourseOnDisk(tempCourseId, new CourseVersionToken(loadingTime), isFullCourse);

					if (exception != null)
					{
						var errorMessage = exception.Message;
						while (exception.InnerException != null)
						{
							errorMessage += $"\n\n{exception.InnerException.Message}";
							exception = exception.InnerException;
						}

						await tempCoursesRepo.UpdateOrAddTempCourseErrorAsync(tempCourseId, errorMessage);
						return (null, errorMessage);
					}

					CourseStorageUpdaterInstance.AddOrUpdateCourse(course);
					await tempCoursesRepo.MarkTempCourseAsNotErroredAsync(tempCourseId);
					await tempCoursesRepo.UpdateTempCourseLoadingTimeAsync(tempCourseId, loadingTime);
				}

				var tempCourse = await tempCoursesRepo.FindAsync(tempCourseId);
				return (tempCourse, null);
			}
		}

		private async Task<(Course Course, Exception Exception)> TryUpdateCourseOnDisk(string courseId, CourseVersionToken versionToken, bool isFull)
		{
			var courseDirectory = GetExtractedCourseDirectory(courseId);
			var stagingTempCourseFile = GetStagingTempCourseFile(courseId);

			var updater = new TempCourseOnDiskUpdater(courseDirectory, versionToken, stagingTempCourseFile, isFull);

			updater.ApplyChanges();

			var (course, error) = LoadCourseFromDirectory(courseId, courseDirectory);
			if (error != null)
			{
				log.Warn(error, $"Не смог загрузить с диска в память временный курс {courseId}. Откатываю.");
				updater.Revert();
				log.Warn(error, $"Откатил временный курс {courseId}.");
				return (course, error);
			}
			return (course, error);
		}

		private class TempCourseOnDiskUpdater
		{
			private readonly DirectoryInfo courseDirectory;
			private readonly CourseVersionToken newVersionToken;
			private readonly CourseVersionToken versionTokenBeforeChanges;
			private readonly List<FileContent> filesToDelete;
			private readonly List<string> directoriesToDelete;
			private readonly List<FileContent> filesToUpdateBeforeChanges;
			private readonly List<string> filesToAdd;

			public TempCourseOnDiskUpdater(DirectoryInfo courseDirectory, CourseVersionToken versionToken, FileInfo zipFile, bool isFull)
			{
				this.courseDirectory = courseDirectory;
				newVersionToken = versionToken;
				var pathPrefix = courseDirectory.FullName;
				var filesToDeleteRelativePaths = ParseDeletedTxt(zipFile);
				var filesInDirectoriesToDelete = GetFilesInDirectoriesToDelete(filesToDeleteRelativePaths, pathPrefix);
				filesToDeleteRelativePaths.AddRange(filesInDirectoriesToDelete);
				var zip = ZipFile.Read(zipFile.FullName, new ReadOptions { Encoding = Encoding.UTF8 });
				var filesToChangeRelativePaths = zip.Entries
					.Where(x => !x.IsDirectory)
					.Select(x => x.FileName)
					.Select(x => x.Replace('/', '\\'))
					.ToList();
				var courseFileRelativePaths = Directory
					.EnumerateFiles(courseDirectory.FullName, "*.*", SearchOption.AllDirectories)
					.Select(file => TrimPrefix(file, pathPrefix))
					.ToHashSet();

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

				filesToUpdateBeforeChanges = filesToChangeRelativePaths
					.Where(courseFileRelativePaths.Contains)
					.Select(path => Path.Combine(pathPrefix, path))
					.Select(path => new FileContent { Path = path, Data = File.ReadAllBytes(path) })
					.ToList();
				filesToAdd = filesToChangeRelativePaths
					.Where(file => !courseFileRelativePaths.Contains(file))
					.Select(path => Path.Combine(pathPrefix, path))
					.ToList();
				filesToDelete = deletedFiles;
				directoriesToDelete = deletedDirectories;
			}

			public void ApplyChanges()
			{
				DeleteFiles(filesToDelete, directoriesToDelete);
				DeleteEmptySubdirectories(courseDirectory.FullName);
				ExtractTempCourseChanges(courseId);
			}

			public void Revert()
			{
				filesToDelete.ForEach(file => new FileInfo(file.Path).Directory.Create());

				static void WriteContent(FileContent fileContent)
				{
					var fInfo = new FileInfo(fileContent.Path);
					if (fInfo.Exists && fInfo.Attributes.HasFlag(FileAttributes.Hidden))
						fInfo.Attributes &= ~FileAttributes.Hidden; // WriteAllBytes кидает ошибку при записи в скрытый файл
					File.WriteAllBytes(fileContent.Path, fileContent.Data);
				}

				filesToUpdateBeforeChanges.ForEach(WriteContent);
				filesToDelete.ForEach(WriteContent);
				filesToAdd.ForEach(File.Delete);
			}

			private List<string> ParseDeletedTxt(FileInfo stagingTempCourseFile)
			{
				var filesToDelete = new List<string>();
				using (var zip = ZipFile.Read(stagingTempCourseFile.FullName))
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

			private static void DeleteFiles(List<FileContent> filesToDelete, List<string> directoriesToDelete)
			{
				filesToDelete.ForEach(file => File.Delete(file.Path));
				directoriesToDelete.ForEach(DeleteNotEmptyDirectory);
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


			private List<string> GetDeletedDirs(List<string> filesToDeleteRelativePaths, string pathPrefix)
			{
				return filesToDeleteRelativePaths.Select(path => Path.Combine(pathPrefix, path))
					.Where(path => Directory.Exists(path) &&
									path.StartsWith(pathPrefix) &&
									!path.Contains(".."))
					.ToList();
			}
		}

		#endregion
	}
}