using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
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
		private static ILog log => LogProvider.Get().ForContext(typeof(CourseManager));

		public const string ExampleCourseId = "Help";

		private static readonly CourseStorage courseStorage = new CourseStorage();
		public static ICourseStorage CourseStorageInstance => courseStorage;
		public static ICourseStorageUpdater CourseStorageUpdaterInstance => courseStorage;

		private readonly DirectoryInfo coursesDirectory;
		private readonly DirectoryInfo tempCourseStaging;

		public static readonly char[] InvalidForCourseIdCharacters = new[] { '&', CourseLoader.CourseIdDelimiter }.Concat(Path.GetInvalidFileNameChars()).Concat(Path.GetInvalidPathChars()).Distinct().ToArray();

		/* TODO (andgein): Use DI */
		private static readonly CourseLoader loader = new CourseLoader(new UnitLoader(new XmlSlideLoader()));
		protected static readonly ErrorsBot errorsBot = new ErrorsBot();

		protected static readonly ConcurrentDictionary<CourseVersionToken, bool> BrokenVersions = new ConcurrentDictionary<CourseVersionToken, bool>();

		public static readonly string CoursesSubdirectory = "Courses";
		public static readonly string TempCourseStagingSubdirectory = "TempCourseStaging";

		protected CourseManager(DirectoryInfo baseDirectory)
			: this(
				baseDirectory.GetSubdirectory(CoursesSubdirectory),
				baseDirectory.GetSubdirectory(TempCourseStagingSubdirectory)
			)
		{
		}

		private CourseManager(DirectoryInfo coursesDirectory, DirectoryInfo tempCourseStaging)
		{
			this.coursesDirectory = coursesDirectory;
			coursesDirectory.EnsureExists();
			this.tempCourseStaging = tempCourseStaging;
			tempCourseStaging.EnsureExists();
		}

		protected FileInfo GetStagingTempCourseFile(string courseId)
		{
			var zipName = courseId + ".zip";
			return tempCourseStaging.GetFile(zipName);
		}

		public DirectoryInfo GetExtractedCourseDirectory(string courseId)
		{
			return coursesDirectory.GetSubdirectory(courseId);
		}

		protected static void CreateCourseFromExample(string courseId, string courseTitle, FileInfo exampleZipToModify)
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

		protected void MoveCourse(Course course, DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory)
		{
			// Не использую TempDirectory, потому что директория, в которую перемещаю, не должна существовать, иначе Move кинет ошибку.
			var tempDirectoryPath = Path.Combine(TempDirectory.TempDirectoryPath, Path.GetRandomFileName());
			try
			{
				destinationDirectory.EnsureExists();

				const int triesCount = 5;
				FuncUtils.TrySeveralTimes(() => Directory.Move(destinationDirectory.FullName, tempDirectoryPath), triesCount);

				try
				{
					FuncUtils.TrySeveralTimes(() => Directory.Move(sourceDirectory.FullName, destinationDirectory.FullName), triesCount);
				}
				catch (IOException)
				{
					/* In case of any file system's error rollback previous operation */
					FuncUtils.TrySeveralTimes(() => Directory.Move(tempDirectoryPath, destinationDirectory.FullName), triesCount);
					throw;
				}
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

		public async Task PostCourseLoadingErrorToTelegram(string courseId, Exception ex)
		{
			await errorsBot.PostToChannelAsync($"Не смог загрузить курс из папки {GetExtractedCourseDirectory(courseId)}:\n{ex.Message.EscapeMarkdown()}\n```{ex.StackTrace}```", ParseMode.Markdown);
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

		protected async Task UpdateCourseOrTempCourseToVersionFromDirectory(string courseId, CourseVersionToken publishedVersionToken, TimeSpan? courseLockLimit)
		{
			if (BrokenVersions.ContainsKey(publishedVersionToken))
				return;
			var courseInMemory = CourseStorageInstance.FindCourse(courseId);
			if (courseInMemory != null && courseInMemory.CourseVersionToken == publishedVersionToken)
				return;
			try
			{
				await UpdateCourseFromDirectoryIfRightVersion(courseId, publishedVersionToken, courseLockLimit);
			}
			catch (Exception ex)
			{
				BrokenVersions.TryAdd(publishedVersionToken, true);
				var message = $"Не смог загрузить с диска в память курс {courseId} версии {publishedVersionToken}";
				if (publishedVersionToken.IsTempCourse())
					log.Warn(ex, message);
				else
				{
					log.Error(ex, message);
					await PostCourseLoadingErrorToTelegram(courseId, ex);
				}
			}
		}

		private async Task UpdateCourseFromDirectoryIfRightVersion(string courseId, CourseVersionToken publishedVersionToken, TimeSpan? courseLockLimit)
		{
			var courseDirectory = GetExtractedCourseDirectory(courseId);
			if (!courseDirectory.Exists)
				return;
			using (var courseLock = await CourseLock.TryAcquireReaderLock(courseId, courseLockLimit))
			{
				if (!courseLock.IsLocked)
				{
					log.Warn($"Не дождался разблокировки курса {courseId} за {courseLockLimit ?? TimeSpan.Zero}");
					return;
				}

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