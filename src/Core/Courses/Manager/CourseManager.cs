using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Ionic.Zip;
using Telegram.Bot.Types.Enums;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Units;
using Ulearn.Core.Helpers;
using Ulearn.Core.Telegram;
using Vostok.Logging.Abstractions;

namespace Ulearn.Core.Courses.Manager
{
	public abstract class CourseManager
	{
		private const string examplePackageName = "Help";

		private static ILog log => LogProvider.Get().ForContext(typeof(CourseManager));

		public const string ExampleCourseId = "Help";

		private static readonly CourseStorage courseStorage = new CourseStorage();
		public static ICourseStorage CourseStorageInstance => courseStorage;
		public static IUpdateCourseStorage CourseStorageUpdaterInstance => courseStorage;

		private readonly DirectoryInfo stagedDirectory;
		private readonly DirectoryInfo coursesDirectory;
		private readonly DirectoryInfo tempCourseStaging;
		private readonly DirectoryInfo coursesVersionsDirectory;

		public static readonly char[] InvalidForCourseIdCharacters = new[] { '&', CourseLoader.CourseIdDelimiter }.Concat(Path.GetInvalidFileNameChars()).Concat(Path.GetInvalidPathChars()).Distinct().ToArray();

		/* TODO (andgein): Use DI */
		public static readonly CourseLoader loader = new CourseLoader(new UnitLoader(new XmlSlideLoader()));
		private static readonly ErrorsBot errorsBot = new ErrorsBot();

		private static readonly ConcurrentDictionary<string, bool> courseIdToIsBroken = new ConcurrentDictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);

		protected static readonly ConcurrentDictionary<CourseVersionToken, bool> brokenVersions = new ConcurrentDictionary<CourseVersionToken, bool>();

		public static readonly string CoursesSubdirectory = "Courses";

		public CourseManager(DirectoryInfo baseDirectory)
			: this(
				baseDirectory.GetSubdirectory("Courses.Staging"),
				baseDirectory.GetSubdirectory("Courses.Versions"),
				baseDirectory.GetSubdirectory(CoursesSubdirectory),
				baseDirectory.GetSubdirectory("TempCourseStaging")
			)
		{
		}

		private CourseManager(DirectoryInfo stagedDirectory, DirectoryInfo coursesVersionsDirectory, DirectoryInfo coursesDirectory, DirectoryInfo tempCourseStaging)
		{
			this.stagedDirectory = stagedDirectory;
			stagedDirectory.EnsureExists();
			this.coursesDirectory = coursesDirectory;
			coursesDirectory.EnsureExists();
			this.tempCourseStaging = tempCourseStaging;
			tempCourseStaging.EnsureExists();
			this.coursesVersionsDirectory = coursesVersionsDirectory;
			coursesVersionsDirectory.EnsureExists();
		}

		protected bool CourseIsBroken(string courseId)
		{
			return courseIdToIsBroken.TryGetValue(courseId, out var val) ? val : false;
		}

		protected void RemoveCourseFromBroken(string courseId)
		{
			courseIdToIsBroken.TryRemove(courseId, out _);
		}

		///
		/// <exception cref="CourseLoadingException"></exception>
		///
		public Course GetVersion(Guid versionId)
		{
			var versionFile = GetCourseVersionFile(versionId);
			var version = LoadCourseFromZip(versionFile);
			return version;
		}

		public FileInfo GetStagingCourseFile(string courseId)
		{
			var packageName = GetPackageName(courseId);
			if (Path.GetInvalidFileNameChars().Any(packageName.Contains))
				throw new Exception(courseId);
			return stagedDirectory.GetFile(packageName);
		}

		public FileInfo GetStagingTempCourseFile(string courseId)
		{
			var packageName = GetPackageName(courseId);
			if (Path.GetInvalidFileNameChars().Any(packageName.Contains))
				throw new Exception(courseId);
			return tempCourseStaging.GetFile(packageName);
		}

		public DirectoryInfo GetExtractedCourseDirectory(string courseId)
		{
			return coursesDirectory.GetSubdirectory(courseId);
		}

		public DirectoryInfo GetExtractedVersionDirectory(Guid versionId)
		{
			return GetExtractedCourseDirectory(versionId.GetNormalizedGuid());
		}

		public FileInfo GetCourseVersionFile(Guid versionId)
		{
			var packageName = GetPackageName(versionId);
			return coursesVersionsDirectory.GetFile(packageName);
		}

		public string GetStagingCoursePath(string courseId)
		{
			return GetStagingCourseFile(courseId).FullName;
		}

		public bool TryReloadCourse(string courseId)
		{
			if (CourseIsBroken(courseId))
				return false;
			try
			{
				ReloadCourseNotSafe(courseId);
				return true;
			}
			catch (Exception e)
			{
				log.Error(e, $"Не могу загрузить курс {courseId}");
				courseIdToIsBroken.AddOrUpdate(courseId, _ => true, (_, _) => true);
			}

			return false;
		}

		public void ReloadCourseNotSafe(string courseId, bool notifyAboutErrors = true)
		{
			/* First try load course from directory */
			var courseDir = GetExtractedCourseDirectory(courseId);
			try
			{
				log.Info($"Сначала попробую загрузить уже распакованный курс из {courseDir.FullName}");
				ReloadCourseFromDirectory(courseDir);
			}
			catch (Exception e)
			{
				log.Warn(e, $"Не смог загрузить курс из папки {courseDir}");
				var zipFile = GetStagingCourseFile(courseId);
				log.Info($"Буду загружать из zip-архива: {zipFile.FullName}");
				if (notifyAboutErrors)
					errorsBot.PostToChannel($"Не смог загрузить курс из папки {courseDir}, буду загружать из zip-архива {zipFile.FullName}:\n{e.Message.EscapeMarkdown()}\n```{e.StackTrace}```", ParseMode.Markdown);
				ReloadCourseFromZip(zipFile);
			}
		}

		private Course ReloadCourseFromZip(FileInfo zipFile)
		{
			var course = LoadCourseFromZip(zipFile);
			courseStorage.AddOrUpdateCourse(course);
			log.Info($"Курс {course.Id} загружен из {zipFile.FullName} и сохранён в памяти");
			
			return course;
		}

		private Course ReloadCourseFromDirectory(DirectoryInfo directory)
		{
			var course = LoadCourseFromDirectory(directory);
			courseStorage.AddOrUpdateCourse(course);
			log.Info($"Курс {course.Id} загружен из {directory.FullName} и сохранён в памяти");
			return course;
		}

		protected void UnzipFile(FileInfo zipFile, DirectoryInfo unpackDirectory)
		{
			using (var zip = ZipFile.Read(zipFile.FullName, new ReadOptions { Encoding = ZipUtils.Cp866 })) // Использует UTF8, где нужно
			{
				log.Info($"Очищаю директорию {unpackDirectory.FullName}");
				unpackDirectory.ClearDirectory();
				log.Info($"Директория {unpackDirectory.FullName} очищена");
				zip.ExtractAll(unpackDirectory.FullName, ExtractExistingFileAction.OverwriteSilently);
				foreach (var f in unpackDirectory.GetFiles("*", SearchOption.AllDirectories).Cast<FileSystemInfo>().Concat(unpackDirectory.GetDirectories("*", SearchOption.AllDirectories)))
					f.Attributes &= ~FileAttributes.ReadOnly;
				log.Info($"Архив {zipFile.FullName} распакован");
			}
		}

		private void UnzipTempWithOverwrite(FileInfo zipFile, DirectoryInfo unpackDirectory)
		{
			using (var zip = ZipFile.Read(zipFile.FullName, new ReadOptions { Encoding = Encoding.UTF8 }))
			{
				zip.ExtractAll(unpackDirectory.FullName, ExtractExistingFileAction.OverwriteSilently);
				foreach (var f in unpackDirectory.GetFiles("*", SearchOption.AllDirectories).Cast<FileSystemInfo>().Concat(unpackDirectory.GetDirectories("*", SearchOption.AllDirectories)))
					f.Attributes &= ~FileAttributes.ReadOnly;
				log.Info($"Архив {zipFile.FullName} распакован");
			}
		}

		private DirectoryInfo UnzipCourseFile(FileInfo zipFile)
		{
			var courseOrVersionId = GetCourseId(zipFile.Name);
			var courseDir = coursesDirectory.GetSubdirectory(courseOrVersionId);
			if (courseDir.Exists)
				return courseDir;
			courseDir.Create();
			log.Info($"Распаковываю архив с курсом из {zipFile.FullName} в {courseDir.FullName}");
			UnzipFile(zipFile, courseDir);
			log.Info($"Распаковал архив с курсом из {zipFile.FullName} в {courseDir.FullName}");
			return courseDir;
		}

		private Course LoadCourseFromZip(FileInfo zipFile)
		{
			var courseDir = UnzipCourseFile(zipFile);
			return LoadCourseFromDirectory(courseDir);
		}

		private Course LoadCourseFromDirectory(DirectoryInfo dir)
		{
			return loader.Load(dir);
		}

		public static string GetCourseId(string packageName)
		{
			return Path.GetFileNameWithoutExtension(packageName);
		}

		public string GetPackageName(string courseId)
		{
			return courseId + ".zip";
		}

		public string GetPackageName(Guid versionId)
		{
			return versionId.GetNormalizedGuid() + ".zip";
		}

		// Пересоздает временный курс в том смылсе, что выкладывает на диск копию основного курса
		public bool CreateTempCourseOnDisk(string courseId, string courseTitle, DateTime loadingTime)
		{
			var package = stagedDirectory.GetFile(GetPackageName(courseId));
			if (package.Exists)
				return true;

			var examplePackage = stagedDirectory.GetFile(GetPackageName(examplePackageName));

			examplePackage.CopyTo(package.FullName, true);
			File.SetLastWriteTime(examplePackage.FullName, DateTime.Now);
			CreateCourseFromExample(courseId, courseTitle, new FileInfo(package.FullName));

			ReloadCourseFromZip(package);
			return true;
		}
		
		public void EnsureVersionIsExtracted(Guid versionId)
		{
			var versionDirectory = GetExtractedVersionDirectory(versionId);
			if (!versionDirectory.Exists)
			{
				Directory.CreateDirectory(versionDirectory.FullName);
				UnzipFile(GetCourseVersionFile(versionId), versionDirectory);
			}
		}

		public void ExtractTempCourseChanges(string tempCourseId)
		{
			var zipWithChanges = GetStagingTempCourseFile(tempCourseId);
			var courseDirectory = GetExtractedCourseDirectory(tempCourseId);
			UnzipTempWithOverwrite(zipWithChanges, courseDirectory);
		}

		public static void CreateCourseFromExample(string courseId, string courseTitle, FileInfo exampleZipToModify)
		{
			var nsResolver = new XmlNamespaceManager(new NameTable());
			nsResolver.AddNamespace("ulearn", "https://ulearn.me/schema/v2");
			using (var zip = ZipFile.Read(exampleZipToModify.FullName, new ReadOptions { Encoding = ZipUtils.Cp866 }))
			{
				var courseXml = zip.Entries.FirstOrDefault(e => Path.GetFileName(e.FileName).Equals("course.xml", StringComparison.OrdinalIgnoreCase) && !e.IsDirectory);
				if (courseXml != null)
					UpdateXmlAttribute(zip[courseXml.FileName], "//ulearn:course", "title", courseTitle, zip, nsResolver);
			}
		}

		private static void UpdateXmlAttribute(ZipEntry entry, string selector, string attribute, string value, ZipFile zip, IXmlNamespaceResolver nsResolver)
		{
			UpdateXmlEntity(entry, selector, element =>
			{
				var elementAttribute = element.Attribute(attribute);
				if (elementAttribute != null)
					elementAttribute.Value = value;
			}, zip, nsResolver);
		}

		private static void UpdateXmlEntity(ZipEntry entry, string selector, Action<XElement> update, ZipFile zip, IXmlNamespaceResolver nsResolver)
		{
			using (var output = StaticRecyclableMemoryStreamManager.Manager.GetStream())
			{
				using (var entryStream = entry.OpenReader())
				{
					var xml = XDocument.Load(entryStream);
					var element = xml.XPathSelectElement(selector, nsResolver);
					update(element.EnsureNotNull($"no element [{selector}] in zip entry {entry.FileName}"));
					xml.Save(output);
				}

				output.Position = 0;
				zip.UpdateEntry(entry.FileName, output.ToArray());
				zip.Save();
			}
		}

		public bool IsCourseIdAllowed(string courseId)
		{
			return !courseId.Any(InvalidForCourseIdCharacters.Contains);
		}

		private void UpdateCourse(Course course)
		{
			if (!courseStorage.HasCourse(course.Id))
				return;

			courseStorage.AddOrUpdateCourse(course);
		}

		private const int updateCourseEachOperationTriesCount = 5;

		protected void MoveCourse(Course course, DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory)
		{
			// Не использую TempDirectory, потому что директория, в которую перемещаю, не должна существовать, иначе Move кинет ошибку.
			var tempDirectoryPath = Path.Combine(TempDirectory.TempDirectoryPath, Path.GetRandomFileName());
			try
			{
				destinationDirectory.EnsureExists();

				FuncUtils.TrySeveralTimes(() => Directory.Move(destinationDirectory.FullName, tempDirectoryPath), updateCourseEachOperationTriesCount);

				try
				{
					FuncUtils.TrySeveralTimes(() => Directory.Move(sourceDirectory.FullName, destinationDirectory.FullName), updateCourseEachOperationTriesCount);
				}
				catch (IOException)
				{
					/* In case of any file system's error rollback previous operation */
					FuncUtils.TrySeveralTimes(() => Directory.Move(tempDirectoryPath, destinationDirectory.FullName), updateCourseEachOperationTriesCount);
					throw;
				}

				UpdateCourse(course);
			}
			finally
			{
				var tempDir = new DirectoryInfo(tempDirectoryPath);
				if (tempDir.Exists)
					tempDir.Delete(true); 
			}
		}

		public static DirectoryInfo GetCoursesDirectory()
		{
			var coursesDirectory = ApplicationConfiguration.Read<UlearnConfiguration>().CoursesDirectory ?? "";
			if (!Path.IsPathRooted(coursesDirectory))
				coursesDirectory = Path.Combine(Utils.GetAppPath(), coursesDirectory);

			return new DirectoryInfo(coursesDirectory);
		}


#region WorkWithCourseInTemporaryDirectory

		public async Task<TempDirectory> ExtractCourseVersionToTemporaryDirectory(string courseId, CourseVersionToken versionToken, byte[] zipContent)
		{
			var tempDirectory = CreateCourseTempDirectory(courseId, versionToken);
			ZipUtils.UnpackZip(zipContent, tempDirectory.DirectoryInfo.FullName);
			await versionToken.Save(tempDirectory.DirectoryInfo);
			return tempDirectory;
		}

		private TempDirectory CreateCourseTempDirectory(string courseId, CourseVersionToken versionToken)
		{
			// @ — разделитель courseId и остального. используется CourseLoader
			var directoryName = $"{courseId}{CourseLoader.CourseIdDelimiter}{versionToken}_{DateTime.Now.ToSortable()}";
			return new TempDirectory(directoryName);
		}

		public TempFile SaveVersionZipToTemporaryDirectory(string courseId, CourseVersionToken versionToken, Stream stream)
		{
			var fileName = $"{courseId}{CourseLoader.CourseIdDelimiter}{versionToken}_{DateTime.Now.ToSortable()}";
			return new TempFile(fileName, stream);
		}

		public (Course Course, Exception Exception) LoadCourseFromDirectory(string courseId, DirectoryInfo extractedCourseDirectory)
		{
			try
			{
				var course = loader.Load(extractedCourseDirectory);
				return (course, null);
			}
			catch (Exception e)
			{
				log.Warn(e, $"Upload course from temp directory exception '{courseId}'");
				return (null, e);
			}
		}

#endregion

#region UpdateInMemoryCourseFromCommonDirectory

		protected async Task UpdateCourseOrTempCourseToVersionFromDirectory(string courseId, CourseVersionToken publishedVersionToken)
		{
			if (brokenVersions.ContainsKey(publishedVersionToken))
				return;
			var courseInMemory = CourseStorageInstance.FindCourse(courseId);
			if (courseInMemory != null && courseInMemory.CourseVersionToken == publishedVersionToken)
				return;
			try
			{
				await UpdateCourseFromDirectory(courseId, publishedVersionToken);
			}
			catch (Exception ex)
			{
				brokenVersions.TryAdd(publishedVersionToken, true);
				var message = $"Не смог загрузить с диска в память курс {courseId} версии {publishedVersionToken}";
				if (publishedVersionToken.IsTempCourse())
					log.Warn(ex, message);
				else
					log.Error(ex, message);
			}
		}

		private async Task UpdateCourseFromDirectory(string courseId, CourseVersionToken publishedVersionToken)
		{
			var courseDirectory = GetExtractedCourseDirectory(courseId);
			if (!courseDirectory.Exists)
				return;
			using (await CourseLock.AcquireReaderLock(courseId))
			{
				var courseVersionToken = CourseVersionToken.Load(courseDirectory);
				if (courseVersionToken != publishedVersionToken)
					return;
				if (!courseDirectory.Exists)
					return;
				var course = loader.Load(courseDirectory);
				CourseStorageUpdaterInstance.AddOrUpdateCourse(course);
			}
		}

#endregion
	}
}