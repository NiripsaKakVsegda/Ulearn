using System.Security.Claims;
using Database.Models;
using Database.Repos;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authentication;
using Ulearn.Common.Extensions;
using uLearn.Web.Core.Extensions;

namespace uLearn.Web.Core.Authorization;

///This transformation adding SysAdmin claim and course roles to identity
public class CourseRolesAndSysAdminClaimsTransformation : IClaimsTransformation
{
	private readonly IUsersRepo usersRepo;
	private readonly ICourseRolesRepo courseRolesRepo;

	public CourseRolesAndSysAdminClaimsTransformation(IUsersRepo usersRepo, ICourseRolesRepo courseRolesRepo)
	{
		this.usersRepo = usersRepo;
		this.courseRolesRepo = courseRolesRepo;
	}

	public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
	{
		var claimsIdentity = new ClaimsIdentity();
		var id = principal.GetUserId();
		if (await usersRepo.IsSystemAdministrator(id))
		{
			claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, LmsRoleType.SysAdmin.ToString()));
			claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, LmsRoleType.SysAdmin.GetDisplayName()));
		}

		var roles = (await courseRolesRepo.GetRoles(id))
			.Select(r => new Claim(UserExtensions.courseRoleClaimType, r.Key + " " + r.Value));
		claimsIdentity.AddClaims(roles);

		principal.AddIdentity(claimsIdentity);
		return await Task.FromResult(principal);
	}
}