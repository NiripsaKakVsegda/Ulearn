using System;
using System.IO;
using System.Threading.Tasks;
using Database.Repos;
using Microsoft.Extensions.DependencyInjection;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;
using Vostok.Logging.Abstractions;

namespace Database
{
	public class SlaveCourseManager : CourseManager, ISlaveCourseManager
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(SlaveCourseManager));

		private readonly IServiceScopeFactory serviceScopeFactory;

		public SlaveCourseManager(IServiceScopeFactory serviceScopeFactory)
			: base(GetCoursesDirectory())
		{
			this.serviceScopeFactory = serviceScopeFactory;
		}

		public virtual async Task UpdateCourses()
		{
			using (var scope = serviceScopeFactory.CreateScope())
			{
				var coursesRepo = scope.ServiceProvider.GetService<ICoursesRepo>();
				var publishedCourseVersions = await coursesRepo.GetPublishedCourseVersions();
				foreach (var publishedVersion in publishedCourseVersions)
				{
					var courseId = publishedVersion.CourseId;
					var publishedVersionId = publishedVersion.Id;
					await UpdateCourseOrTempCourseToVersionFromDirectory(courseId, new CourseVersionToken(publishedVersionId), TimeSpan.FromSeconds(0.1));
				}
			}
		}

		public virtual async Task UpdateTempCourses()
		{
			using (var scope = serviceScopeFactory.CreateScope())
			{
				var tempCoursesRepo = scope.ServiceProvider.GetService<ITempCoursesRepo>();
				var recentTempCourses = await tempCoursesRepo.GetRecentTempCourses();
				foreach (var tempCourse in recentTempCourses)
				{
					var courseId = tempCourse.CourseId;
					var publishedLoadingTime = tempCourse.LoadingTime;
					await UpdateCourseOrTempCourseToVersionFromDirectory(courseId, new CourseVersionToken(publishedLoadingTime), TimeSpan.FromSeconds(0.1));
				}
			}
		}

		public async Task<bool> CreateCourseIfNotExists(string courseId, Guid versionId, string courseTitle, string userId)
		{
			using (var scope = serviceScopeFactory.CreateScope())
			{
				var coursesRepo = scope.ServiceProvider.GetService<ICoursesRepo>();
				var hasCourse = await coursesRepo.GetPublishedCourseVersion(courseId) != null;
				if (!hasCourse)
				{
					var helpVersionFile = await coursesRepo.GetPublishedVersionFile(ExampleCourseId);
					using (var exampleCourseZip = SaveVersionZipToTemporaryDirectory(courseId, new CourseVersionToken(versionId), new MemoryStream(helpVersionFile.File)))
					{
						CreateCourseFromExample(courseId, courseTitle, exampleCourseZip.FileInfo);
						await coursesRepo.AddCourseVersion(courseId, versionId, userId, null, null, null, null, await exampleCourseZip.FileInfo.ReadAllContentAsync());
					}
					await coursesRepo.MarkCourseVersionAsPublished(versionId);
				}
				return !hasCourse;
			}
		}
	}
}