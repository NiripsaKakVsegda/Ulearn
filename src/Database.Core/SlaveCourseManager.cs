using System.Threading.Tasks;
using Database.Repos;
using Microsoft.Extensions.DependencyInjection;
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
					await UpdateCourseOrTempCourseToVersionFromDirectory(courseId, new CourseVersionToken(publishedVersionId));
				}
			}
		}

		public virtual async Task UpdateTempCourses()
		{
			using (var scope = serviceScopeFactory.CreateScope())
			{
				var tempCoursesRepo = scope.ServiceProvider.GetService<ITempCoursesRepo>();
				var tempCourses = await tempCoursesRepo.GetTempCoursesAsync();
				foreach (var tempCourse in tempCourses)
				{
					var courseId = tempCourse.CourseId;
					var publishedLoadingTime = tempCourse.LoadingTime;
					await UpdateCourseOrTempCourseToVersionFromDirectory(courseId, new CourseVersionToken(publishedLoadingTime));
				}
			}
		}
	}
}