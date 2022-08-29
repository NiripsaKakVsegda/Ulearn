using System.Security.Claims;
using AspNet.Security.OAuth.Vkontakte;
using Database.Models;
using Database.Repos;
using Database.Repos.Users;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Web.Core.Attributes;
using uLearn.Web.Core.External.Kontur;
using uLearn.Web.Core.External.Vkontakte;
using uLearn.Web.Core.Models;
using Vostok.Logging.Abstractions;

namespace uLearn.Web.Core.Controllers;

public class HandleHttpAntiForgeryException : ActionFilterAttribute, IExceptionFilter
{
	private static ILog log => LogProvider.Get().ForContext(typeof(HandleHttpAntiForgeryException));

	public void OnException(ExceptionContext filterContext)
	{
		if (filterContext.Exception is not AntiforgeryValidationException)
			return;

		if (IsAjaxRequest(filterContext.HttpContext.Request))
			filterContext.Result = new ForbidResult();
		else
			filterContext.Result = new RedirectResult("/");

		log.Info($"{nameof(HandleHttpAntiForgeryException)} did his job");
		filterContext.ExceptionHandled = true;
	}

	/// <summary>
	/// Determines whether the specified HTTP request is an AJAX request.
	/// </summary>
	/// 
	/// <returns>
	/// true if the specified HTTP request is an AJAX request; otherwise, false.
	/// </returns>
	/// <param name="request">The HTTP request.</param><exception cref="T:System.ArgumentNullException">The <paramref name="request"/> parameter is null (Nothing in Visual Basic).</exception>
	public static bool IsAjaxRequest(HttpRequest request)
	{
		if (request == null)
			throw new ArgumentNullException(nameof(request));

		if (request.Headers != null)
			return request.Headers["X-Requested-With"] == "XMLHttpRequest";
		return false;
	}
}

public class LoginController : BaseUserController
{
	private SignInManager<ApplicationUser> signInManager;
	private AuthenticationManager authenticationManager;

	public LoginController(UlearnUserManager userManager, IUsersRepo usersRepo, SignInManager<ApplicationUser> signInManager, AuthenticationManager authenticationManager)
		: base(userManager, usersRepo)
	{
		this.signInManager = signInManager;
		this.authenticationManager = authenticationManager;
	}

	private static ILog log => LogProvider.Get().ForContext(typeof(LoginController));

	public ActionResult Index(string returnUrl)
	{
		ViewBag.ReturnUrl = returnUrl;
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public async Task<ActionResult> Index(LoginViewModel model, string returnUrl)
	{
		if (ModelState.IsValid)
		{
			var user = await userManager.FindByNameAsync(model.UserName);

			if (user == null)
			{
				/* If user with this username is not exists then try to find user with this email.
					It allows to login not only with username/password, but with email/password */
				var usersWithEmail = await usersRepo.FindUsersByUsernameOrEmail(model.UserName);

				/* For signing in via email/password we need to be sure that email is confirmed */
				user = usersWithEmail.FirstOrDefault(u => u.EmailConfirmed);
			}

			if (user != null)
				if (!await userManager.CheckPasswordAsync(user, model.Password).ConfigureAwait(false))
					user = null;

			if (user != null)
			{
				// await HttpContext.SignOutAsync(DefaultAuthenticationTypes.ExternalCookie);
				await authenticationManager.LoginAsync(HttpContext, user, model.RememberMe);
				await SendConfirmationEmailAfterLogin(user).ConfigureAwait(false);
				return Redirect(this.FixRedirectUrl(returnUrl));
			}

			ModelState.AddModelError("", @"Неверное имя пользователя или пароль");
		}

		/* If we got this far, something failed, redisplay form */
		ViewBag.ReturnUrl = returnUrl;
		return View(model);
	}

	private async Task SendConfirmationEmailAfterLogin(ApplicationUser user)
	{
		if (string.IsNullOrEmpty(user.Email) || user.EmailConfirmed)
			return;

		if (user.LastConfirmationEmailTime == null)
			await SendConfirmationEmail(user);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	//[HandleHttpAntiForgeryException]
	public IActionResult ExternalLogin(string provider, string? returnUrl, bool? rememberMe)
	{
		var userId = userManager.GetUserId(User);
		var properties = signInManager.ConfigureExternalAuthenticationProperties(
			provider,
			Url.Action("ExternalLoginCallback", new { returnUrl, rememberMe }),
			userId
		);

		return Challenge(properties, provider);
	}

	public async Task<ActionResult> ExternalLoginCallback(string returnUrl, bool? rememberMe)
	{
		var userId = userManager.GetUserId(User);
		var info = await signInManager.GetExternalLoginInfoAsync(userId);
		if (info == null)
			return RedirectToAction("Index", "Login");

		var user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
		if (user != null)
		{
			await UpdateUserFieldsFromExternalLoginInfo(user, info);

			await authenticationManager.LoginAsync(HttpContext, user, isPersistent: rememberMe ?? false);
			await SendConfirmationEmailAfterLogin(user);
			return Redirect(this.FixRedirectUrl(returnUrl));
		}

		//if (loginInfo.ExternalIdentity.AuthenticationType == Constants.DefaultAuthenticationType) TODO
		if (info.LoginProvider == Constants.DefaultAuthenticationType)
			metricSender.SendCount("registration.via_vk.try");
		else if (info.LoginProvider == KonturPassportConstants.AuthenticationType)
			metricSender.SendCount("registration.via_kontur_passport.try");

		// If the user does not have an account, then prompt the user to create an account
		ViewBag.ReturnUrl = returnUrl;
		ViewBag.LoginProvider = info.LoginProvider;

		Gender? loginGender = null;
		info.Principal.FindFirstValue(ClaimTypes.Gender)?.TryParseToNullableEnum(out loginGender);
		return View("ExternalLoginConfirmation",
			new ExternalLoginConfirmationViewModel
			{
				UserName = null,
				Email = info.Principal.FindFirstValue(ClaimTypes.Email),
				Gender = loginGender
			});
	}

	[HttpPost]
	//[ValidateInput(false)]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
	{
		if (User.Identity.IsAuthenticated)
			return RedirectToAction("Manage", "Account");

		var info = await authenticationManager.GetExternalLoginInfoAsync();
		if (info == null)
			return View("ExternalLoginFailure");

		ViewBag.LoginProvider = info.LoginProvider;
		ViewBag.ReturnUrl = returnUrl;
		
		if (ModelState.IsValid)
		{
			var userAvatarUrl = info.Principal.FindFirstValue("AvatarUrl");
			var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
			var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);

			/* Some users enter email with trailing whitespaces. Remove them (not users, but spaces!) */
			model.Email = (model.Email ?? "").Trim();

			if (!await CanNewUserSetThisEmail(model.Email))
			{
				ModelState.AddModelError("Email", AccountController.ManageMessageId.EmailAlreadyTaken.GetDisplayName());
				return View(model);
			}

			var user = new ApplicationUser
			{
				UserName = model.UserName,
				FirstName = firstName,
				LastName = lastName,
				AvatarUrl = userAvatarUrl,
				Email = model.Email,
				Gender = model.Gender,
			};
			var result = await userManager.CreateAsync(user);
			if (result.Succeeded)
			{
				result = await userManager.AddLoginAsync(user, info);
				if (result.Succeeded)
				{
					await userManager.AddPasswordAsync(user, model.Password);
					await authenticationManager.LoginAsync(HttpContext, user, isPersistent: false);
					if (!await SendConfirmationEmail(user))
					{
						log.Warn("ExternalLoginConfirmation(): can't send confirmation email");
						return RedirectToAction("Manage", "Account", new { Message = AccountController.ManageMessageId.ErrorOccured });
					}

					metricSender.SendCount("registration.success");
					if (info.LoginProvider == Constants.DefaultAuthenticationType)
						metricSender.SendCount("registration.via_vk.success");
					else if (info.LoginProvider == KonturPassportConstants.AuthenticationType)
						metricSender.SendCount("registration.via_kontur_passport.success");

					return Redirect(this.FixRedirectUrl(returnUrl));
				}
			}

			this.AddErrors(result);
		}

		return View(model);
	}

	public ActionResult ExternalLoginFailure()
	{
		return View();
	}

	[HttpGet]
	[Authorize(Policy = "Students")]//[ULearnAuthorize]
	public ActionResult LinkLogin(string provider, string returnUrl)
	{
		return View(new LinkLoginViewModel
		{
			Provider = provider,
			ReturnUrl = returnUrl,
		});
	}

	[HttpPost]
	[Authorize(Policy = "Students")]//[ULearnAuthorize]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public ActionResult DoLinkLogin(string provider, string returnUrl = "")
	{
		var userId = userManager.GetUserId(User);
		var properties = signInManager.ConfigureExternalAuthenticationProperties(
			provider,
			Url.Action("LinkLoginCallback", new { returnUrl }),
			userId
		);
		return Challenge(properties, provider);
	}

	[Authorize(Policy = "Students")]//[ULearnAuthorize]
	public async Task<ActionResult> LinkLoginCallback(string returnUrl = "")
	{
		var user = await userManager.GetUserAsync(User);
		var loginInfo = await authenticationManager.GetExternalLoginInfoAsync(user.Id);
		if (loginInfo == null)
		{
			log.Warn("LinkLoginCallback: GetExternalLoginInfoAsync() returned null");
			return RedirectToAction("Manage", "Account", new { Message = AccountController.ManageMessageId.ErrorOccured });
		}

		var result = await userManager.AddLoginAsync(user, loginInfo);
		if (result.Succeeded)
		{
			await UpdateUserFieldsFromExternalLoginInfo(user, loginInfo);

			if (!string.IsNullOrEmpty(returnUrl))
				return Redirect(this.FixRedirectUrl(returnUrl));

			return RedirectToAction("Manage", "Account", new { Message = AccountController.ManageMessageId.LoginAdded });
		}

		return RedirectToAction("Manage", "Account", new { Message = AccountController.ManageMessageId.AlreadyLinkedToOtherUser, Provider = loginInfo.LoginProvider, OtherUserId = user?.Id ?? "" });
	}

	private async Task UpdateUserFieldsFromExternalLoginInfo(ApplicationUser user, ExternalLoginInfo info)
	{
		var avatarUrl = info.Principal.FindFirstValue("AvatarUrl");
		var konturLogin = info.Principal.FindFirstValue("KonturLogin");
		var sex = info.Principal.FindFirstValue(ClaimTypes.Gender);
		Gender? userSex = null;
		if (Enum.TryParse(sex, out Gender parsedSex))
			userSex = parsedSex;

		if (!string.IsNullOrEmpty(avatarUrl))
			user.AvatarUrl = avatarUrl;
		if (!string.IsNullOrEmpty(konturLogin))
			user.KonturLogin = konturLogin;
		if (userSex != null && user.Gender == null)
			user.Gender = userSex;

		await userManager.UpdateAsync(user);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public async Task<ActionResult> LogOff()
	{
		await authenticationManager.Logout(HttpContext);
		return RedirectToAction("Index", "Home");
	}

	public async Task<ActionResult> EnsureKonturProfileLogin(string returnUrl)
	{
		if (User.Identity.IsAuthenticated)
		{
			var userId = User.GetUserId();
			var user = await userManager.FindByIdAsync(userId);
			var hasKonturPassportLogin = user.Logins.Any(l => l.LoginProvider == KonturPassportConstants.AuthenticationType);
			if (hasKonturPassportLogin)
				return Redirect(this.FixRedirectUrl(returnUrl));

			return View("AddKonturProfileLogin", model: returnUrl);
		}
		else
		{
			var newReturnUrl = Url.Action("EnsureKonturProfileLogin", "Login", new { returnUrl });
			return View("EnsureKonturProfileLogin", model: newReturnUrl);
		}
	}

	private const string XsrfKey = "XsrfId";

	private class ChallengeResult : UnauthorizedResult
	{
		public ChallengeResult(string provider, string redirectUri, string userId = null)
		{
			LoginProvider = provider;
			RedirectUri = redirectUri;
			UserId = userId;
		}

		public string LoginProvider { get; set; }
		public string RedirectUri { get; set; }
		public string UserId { get; set; }

		public override void ExecuteResult(ActionContext context)
		{
			var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
			if (UserId != null)
				properties.Items[XsrfKey] = UserId; // TODO DICTIONARY INSTEAD OF ITEMS

			context.HttpContext.ChallengeAsync(LoginProvider, properties);
		}
	}
}

public class LinkLoginViewModel
{
	public string Provider { get; set; }

	public string ReturnUrl { get; set; }
}