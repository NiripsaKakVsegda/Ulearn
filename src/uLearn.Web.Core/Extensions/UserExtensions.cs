using System.Security.Claims;
using System.Security.Principal;
using Database.Models;
using Database.Repos.Users;
using Ulearn.Core.Courses.Manager;

namespace uLearn.Web.Core.Extensions;

public static class UserExtensions
{
	public static IEnumerable<string> GetControllableCoursesId(this IPrincipal principal)
	{
		principal.IsInRole(LmsRoleType.SysAdmin.ToString());
		if (!principal.IsSystemAdministrator())
			return principal.GetCoursesIdFor(CourseRoleType.Instructor);
		var courseStorage = CourseManager.CourseStorageInstance;
		return courseStorage.GetCourses().Select(course => course.Id);
	}
	
		private const string courseRoleClaimType = "CourseRole";

		public static bool HasAccessFor(this IPrincipal principal, string courseId, CourseRoleType minAccessLevel)
		{
			if (principal.IsSystemAdministrator())
				return true;

			var courseRole = principal.GetAllRoles().FirstOrDefault(t => string.Equals(t.Item1, courseId, StringComparison.OrdinalIgnoreCase));

			return courseRole?.Item2 <= minAccessLevel;
		}

		public static bool HasAccess(this IPrincipal principal, CourseRoleType minAccessLevel)
		{
			if (principal.IsSystemAdministrator())
				return true;

			var roles = principal.GetAllRoles().Select(t => t.Item2).ToList();

			if (!roles.Any())
				return false;
			return roles.Min() <= minAccessLevel;
		}

		private static IEnumerable<Tuple<string, CourseRoleType>> GetAllRoles(this IPrincipal principal)
		{
			var roleTuples = principal
				.ToClaimsPrincipal()
				.FindAll(courseRoleClaimType)
				.Select(claim => claim.Value.Split())
				.Select(s => Tuple.Create(s[0], s[1]));
			foreach (var roleTuple in roleTuples)
			{
				if (!Enum.TryParse(roleTuple.Item2, true, out CourseRoleType role))
					continue;
				yield return Tuple.Create(roleTuple.Item1, role);
			}
		}

		public static CourseRoleType? GetCourseRole(this IPrincipal principal, string courseId)
		{
			return principal.GetAllRoles().FirstOrDefault(t => string.Equals(t.Item1, courseId, StringComparison.OrdinalIgnoreCase))?.Item2;
		}

		public static IEnumerable<string> GetCoursesIdFor(this IPrincipal principal, CourseRoleType role)
		{
			return principal.GetAllRoles().Where(t => t.Item2 <= role).Select(t => t.Item1);
		}

		private static ClaimsPrincipal ToClaimsPrincipal(this IPrincipal principal)
		{
			return principal as ClaimsPrincipal ?? new ClaimsPrincipal(principal);
		}

		public static bool IsSystemAdministrator(this IPrincipal principal)
		{
			return principal.IsInRole(LmsRoleType.SysAdmin.ToString());
		}

		public static void AddCourseRoles(this ClaimsIdentity identity, Dictionary<string, CourseRoleType> roles)
		{
			foreach (var role in roles)
				identity.AddCourseRole(role.Key, role.Value);
		}

		private static void AddCourseRole(this ClaimsIdentity identity, string courseId, CourseRoleType role)
		{
			identity.AddClaim(new Claim(courseRoleClaimType, courseId + " " + role));
		}

		// public static async Task<ClaimsIdentity> GenerateUserIdentityAsync(this ApplicationUser user, UserManager<ApplicationUser> manager, UserRolesRepo userRoles)
		// {
		// 	var identity = await manager.CreateIdentityAsync(user, "Identity.Application");
		// 	identity.AddCourseRoles(userRoles.GetRoles(user.Id));
		// 	return identity;
		// }

		// public static async Task<ClaimsIdentity> GenerateUserIdentityAsync(this ApplicationUser user, UserManager<ApplicationUser> manager)
		// {
		// 	var userRoles = new UserRolesRepo();
		// 	return await user.GenerateUserIdentityAsync(manager, userRoles);
		// }
		//
		// public static bool HasSystemAccess(this ApplicationUser user, SystemAccessType accessType)
		// {
		// 	var systemAccessesRepo = new SystemAccessesRepo();
		// 	return systemAccessesRepo.HasSystemAccess(user.Id, accessType);
		// }
		//
		// public static bool HasSystemAccess(this IPrincipal User, SystemAccessType accessType)
		// {
		// 	var systemAccessesRepo = new SystemAccessesRepo();
		// 	return systemAccessesRepo.HasSystemAccess(User.Identity.GetUserId(), accessType);
		// }
		//
		// public static bool HasCourseAccess(this ApplicationUser User, string courseId, CourseAccessType accessType)
		// {
		// 	var coursesRepo = new CoursesRepo();
		// 	return coursesRepo.HasCourseAccess(User.Id, courseId, accessType);
		// }
		//
		// public static bool HasCourseAccess(this IPrincipal User, string courseId, CourseAccessType accessType)
		// {
		// 	var coursesRepo = new CoursesRepo();
		// 	return coursesRepo.HasCourseAccess(User.Identity.GetUserId(), courseId, accessType);
		// }

		public static bool IsUlearnBot(this ApplicationUser user)
		{
			return user.UserName == UsersRepo.UlearnBotUsername;
		}
}