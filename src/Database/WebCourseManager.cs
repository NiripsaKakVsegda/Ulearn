using System;
using System.IO;
using System.Threading.Tasks;
using Database.DataContexts;
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
			: base(GetCoursesDirectory())
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
			var tempCourses = tempCoursesRepo.GetTempCoursesNoTracking();
			foreach (var tempCourse in tempCourses)
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
					CreateCourseFromExample(courseId, courseTitle, exampleCourseZip.FileInfo);
					await coursesRepo.AddCourseVersion(courseId, versionId, userId, null, null, null, null, await exampleCourseZip.FileInfo.ReadAllContentAsync()
						.ConfigureAwait(false)).ConfigureAwait(false);
				}
				await coursesRepo.MarkCourseVersionAsPublished(versionId).ConfigureAwait(false);
			}
			return !hasCourse;
		}
	}
}