using Database.Models;
using Database.Repos;
using Microsoft.AspNet.Identity;
using Vostok.Logging.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using uLearn.Web.Core.Authentication;
using uLearn.Web.Core.Extensions;

namespace uLearn.Web.Core.Controllers;

public class AuthenticationManager
{
	private readonly ICourseRolesRepo courseRolesRepo;
	private readonly SignInManager<ApplicationUser> signInManager;
	private readonly IUserClaimsPrincipalFactory<ApplicationUser> principalFactory;

	private static ILog log => LogProvider.Get().ForContext(typeof(AuthenticationManager));

	public AuthenticationManager(ICourseRolesRepo courseRolesRepo, IUserClaimsPrincipalFactory<ApplicationUser> principalFactory, SignInManager<ApplicationUser> signInManager)
	{
		this.courseRolesRepo = courseRolesRepo;
		this.principalFactory = principalFactory;
		this.signInManager = signInManager;
	}

	public async Task LoginAsync(HttpContext context, ApplicationUser user, bool isPersistent)
	{
		log.Info($"Пользователь {user.VisibleName} (логин = {user.UserName}, id = {user.Id}) залогинился");
		await InternalLoginAsync(context, user, isPersistent);
	}

	public async Task Logout(HttpContext context)
	{
		await context.SignOutAsync(UlearnAuthenticationConstants.DefaultExternalAuthenticationScheme);
		await context.SignOutAsync(UlearnAuthenticationConstants.DefaultAuthenticationScheme);
	}

	public async Task<ExternalLoginInfo> GetExternalLoginInfoAsync(string? xsrfKey = null)
	{
		return await signInManager.GetExternalLoginInfoAsync(xsrfKey);
		//return await context.GetExternalLoginInfoAsync(xsrfKey, userId);
	}

	private async Task InternalLoginAsync(HttpContext context, ApplicationUser user, bool isPersistent)
	{
		//await context.SignOutAsync(DefaultAuthenticationTypes.ExternalCookie);
		await context.AuthenticateAsync(UlearnAuthenticationConstants.DefaultAuthenticationScheme);
		var claimsIdentity = await user.GenerateUserIdentityAsync(principalFactory, courseRolesRepo);
		await context.SignInAsync(UlearnAuthenticationConstants.DefaultAuthenticationScheme, claimsIdentity, new AuthenticationProperties { IsPersistent = isPersistent });
	}
}