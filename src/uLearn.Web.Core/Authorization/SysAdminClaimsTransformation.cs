using System.Security.Claims;
using Database.Models;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authentication;
using Ulearn.Common.Extensions;

namespace uLearn.Web.Core.Authorization;

///This transformation adding SysAdmin claim to identity
public class SysAdminClaimsTransformation : IClaimsTransformation
{
	private readonly IUsersRepo usersRepo;

	public SysAdminClaimsTransformation(IUsersRepo usersRepo)
	{
		this.usersRepo = usersRepo;
	}

	public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
	{
		var claimsIdentity = new ClaimsIdentity();
		if (await usersRepo.IsSystemAdministrator(principal.GetUserId()))
		{
			claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, LmsRoleType.SysAdmin.ToString()));
			claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, LmsRoleType.SysAdmin.GetDisplayName()));
		}

		principal.AddIdentity(claimsIdentity);
		return await Task.FromResult(principal);
	}
}
