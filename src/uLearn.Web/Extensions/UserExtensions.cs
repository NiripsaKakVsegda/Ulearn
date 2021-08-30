using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Database;
using Database.Extensions;
using Database.Models;

namespace uLearn.Web.Extensions
{
	public static class UserExtensions
	{
		public static IEnumerable<string> GetControllableCoursesId(this IPrincipal principal)
		{
			if (!principal.IsSystemAdministrator())
				return principal.GetCoursesIdFor(CourseRole.Instructor);
			var courseStorage = WebCourseManager.CourseStorageInstance;
			return courseStorage.GetCourses().Select(course => course.Id);
		}
	}
}