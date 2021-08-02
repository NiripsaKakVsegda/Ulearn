using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Database.DataContexts;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;
using Vostok.Logging.Abstractions;
using Ulearn.Core.Courses.Manager;

namespace Database
{
	public class WebCourseManager : CourseManager, IWebCourseManager
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(WebCourseManager));

		private static readonly WebCourseManager courseManagerInstance = new WebCourseManager();
		public static IWebCourseManager CourseManagerInstance => courseManagerInstance;
		public static ICourseUpdater CourseUpdaterInstance => courseManagerInstance;

		private WebCourseManager()
		{
		}

		public async Task UpdateCourses()
		{
			var coursesRepo = new CoursesRepo();
			var publishedCourseVersions = coursesRepo.GetPublishedCourseVersions();
			foreach (var publishedVersion in publishedCourseVersions)
			{
				var courseId = publishedVersion.CourseId;
				var publishedVersionId = publishedVersion.Id;
				await UpdateCourseOrTempCourseToVersionFromDirectory(courseId, new CourseVersionToken(publishedVersionId), TimeSpan.FromSeconds(0.1));
			}
		}

		public async Task UpdateTempCourses()
		{
			var tempCoursesRepo = new TempCoursesRepo();
			var recentTempCourses = tempCoursesRepo.GetRecentTempCourses();
			foreach (var tempCourse in recentTempCourses)
			{
				var courseId = tempCourse.CourseId;
				var publishedLoadingTime = tempCourse.LoadingTime;
				await UpdateCourseOrTempCourseToVersionFromDirectory(courseId, new CourseVersionToken(publishedLoadingTime), TimeSpan.FromSeconds(0.1));
			}
		}

		public async Task<bool> CreateCourseIfNotExists(string courseId, Guid versionId, string courseTitle, string userId)
		{
			var coursesRepo = new CoursesRepo();
			var hasCourse = coursesRepo.GetPublishedCourseVersion(courseId) != null;
			if (!hasCourse)
			{
				var helpVersionFile = coursesRepo.GetPublishedVersionFile(ExampleCourseId);
				using (var exampleCourseZip = SaveVersionZipToTemporaryDirectory(courseId, new CourseVersionToken(versionId), new MemoryStream(helpVersionFile.File)))
				{
					CourseZipWithTitleUpdater.Update(exampleCourseZip.FileInfo, courseTitle);
					await coursesRepo.AddCourseVersion(courseId, courseTitle, versionId, userId, null, null, null, null, await exampleCourseZip.FileInfo.ReadAllContentAsync()
						.ConfigureAwait(false)).ConfigureAwait(false);
				}
				await coursesRepo.MarkCourseVersionAsPublished(versionId).ConfigureAwait(false);
			}
			return !hasCourse;
		}

		public async Task<byte[]> GetTempCourseZipBytes(string courseId)
		{
			var path = GetExtractedCourseDirectory(courseId);
			using (await CourseLock.AcquireReaderLockAsync(courseId).ConfigureAwait(false))
			{
				using var stream = ZipUtils.CreateZipFromDirectory(new List<string> { path.FullName }, new List<string> { CourseVersionToken.VersionFileName }, null);
				return stream.ToArray();
			}
		}
	}
}