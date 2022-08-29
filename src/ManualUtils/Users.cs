using System.IO;
using System.Linq;
using Database;
using Database.Models;
using Database.Repos;

namespace ManualUtils
{
	public class Users
	{
		public static void PrintCourseAdmins(UlearnDb db)
		{
			var courseRolesRepo = new CourseRolesRepo(db, null, null);
			var adminIds = courseRolesRepo.GetListOfUsersWithCourseRole(CourseRoleType.CourseAdmin, null, false).Result;
			var users = adminIds.Select(
				adminId =>
				{
					var roles = courseRolesRepo.Internal_GetActualUserRoles(adminId).Result.Where(r => r.Role == CourseRoleType.CourseAdmin).ToList();
					return new
					{
						Name = roles[0].User.VisibleName,
						Email = roles[0].User.Email,
						Courses = string.Join(", ", roles.Select(e => e.CourseId).ToList()),
						Comments = string.Join(", ", roles.Select(e => e.Comment).Where(c => !string.IsNullOrWhiteSpace(c)).ToList())
					};
				}).ToList();
			var lines = users.Select(u => $"{u.Name}\t{u.Email}\t{u.Courses}\t{u.Comments}");
			File.WriteAllLines("admins.txt", lines);
		}
	}
}