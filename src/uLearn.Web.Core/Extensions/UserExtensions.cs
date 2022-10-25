using System.Security.Claims;
using System.Security.Principal;
using Database.Models;
using Database.Repos;
using Database.Repos.SystemAccessesRepo;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Manager;

namespace uLearn.Web.Core.Extensions;

public static class UserExtensions
{
	public static IEnumerable<string> GetControllableCoursesId(this ClaimsPrincipal principal)
	{
		principal.IsInRole(LmsRoleType.SysAdmin.ToString());
		if (!principal.IsSystemAdministrator())
			return principal.GetCoursesIdFor(CourseRoleType.Instructor);
		var courseStorage = CourseManager.CourseStorageInstance;
		return courseStorage.GetCourses().Select(course => course.Id);
	}

	public const string courseRoleClaimType = "CourseRole";

	public static bool HasAccessFor(this ClaimsPrincipal principal, string courseId, CourseRoleType minAccessLevel)
	{
		if (principal.IsSystemAdministrator())
			return true;

		var courseRole = principal
			.GetAllRoles()
			.FirstOrDefault(t => string.Equals(t.Item1, courseId, StringComparison.OrdinalIgnoreCase));

		return courseRole?.Item2 <= minAccessLevel;
	}

	public static bool HasAccess(this ClaimsPrincipal principal, CourseRoleType minAccessLevel)
	{
		if (principal.IsSystemAdministrator())
			return true;

		var roles = principal.GetAllRoles().Select(t => t.Item2).ToList();

		if (!roles.Any())
			return false;
		return roles.Min() <= minAccessLevel;
	}

	internal static IEnumerable<Tuple<string, CourseRoleType>> GetAllRoles(this ClaimsPrincipal principal)
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

	public static CourseRoleType? GetCourseRole(this ClaimsPrincipal principal, string courseId)
	{
		return principal.GetAllRoles().FirstOrDefault(t => string.Equals(t.Item1, courseId, StringComparison.OrdinalIgnoreCase))?.Item2;
	}

	public static IEnumerable<string> GetCoursesIdFor(this ClaimsPrincipal principal, CourseRoleType role)
	{
		return principal.GetAllRoles().Where(t => t.Item2 <= role).Select(t => t.Item1);
	}

	private static ClaimsPrincipal ToClaimsPrincipal(this ClaimsPrincipal principal)
	{
		return principal as ClaimsPrincipal ?? new ClaimsPrincipal(principal);
	}

	public static bool IsSystemAdministrator(this ClaimsPrincipal principal)
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

	public static async Task<ClaimsPrincipal> GenerateUserIdentityAsync(this ApplicationUser user, IUserClaimsPrincipalFactory<ApplicationUser> principalFactory, ICourseRolesRepo courseRolesRepo)
	{
		var identity = await principalFactory.CreateAsync(user);
		// var roles = (await courseRolesRepo.GetRoles(user.Id))
		// 	.Select(r => new Claim(courseRoleClaimType, r.Key + " " + r.Value));
		// identity.Identities.FirstOrDefault()?.AddClaims(roles);
		return identity;
	}

	public static async Task<bool> HasSystemAccess(this ApplicationUser user, ISystemAccessesRepo systemAccessesRepo, SystemAccessType accessType)
	{
		return await systemAccessesRepo.HasSystemAccessAsync(user.Id, accessType);
	}

	public static async Task<bool> HasSystemAccess(this ClaimsPrincipal User, ISystemAccessesRepo systemAccessesRepo, SystemAccessType accessType)
	{
		return await systemAccessesRepo.HasSystemAccessAsync(User.GetUserId(), accessType);
	}

	public static async Task<bool> HasCourseAccess(this ApplicationUser User, ICoursesRepo coursesRepo, string courseId, CourseAccessType accessType)
	{
		return await coursesRepo.HasCourseAccess(User.Id, courseId, accessType);
	}

	public static async Task<bool> HasCourseAccess(this ClaimsPrincipal User, ICoursesRepo coursesRepo, string courseId, CourseAccessType accessType)
	{
		return await coursesRepo.HasCourseAccess(User.GetUserId(), courseId, accessType);
	}

	public static bool IsUlearnBot(this ApplicationUser user)
	{
		return user.UserName == UsersRepo.UlearnBotUsername;
	}
}