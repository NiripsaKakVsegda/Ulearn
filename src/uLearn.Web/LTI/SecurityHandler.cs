using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using Database.DataContexts;
using Database.Models;
using Vostok.Logging.Abstractions;
using LtiLibrary.Owin.Security.Lti.Provider;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Ulearn.Common;

namespace uLearn.Web.LTI
{
	public static class SecurityHandler
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(SecurityHandler));

		/// <summary>
		/// Invoked after the LTI request has been authenticated so the application can sign in the application user.
		/// </summary>
		/// <param name="context">Contains information about the login session as well as the LTI request.</param>
		/// <param name="claims">Optional set of claims to add to the identity.</param>
		/// <returns>A <see cref="Task"/> representing the completed operation.</returns>
		public static async Task OnAuthenticated(LtiAuthenticatedContext context, IEnumerable<Claim> claims = null)
		{
			log.Info($"LTI обрабатывает запрос на {context.Request.Uri}");

			ClaimsIdentity identity = null;
			using (var db = new ULearnDb())
			{
				var loginProvider = string.Join(":", context.Options.AuthenticationType, context.LtiRequest.ConsumerKey);
				var providerKey = context.LtiRequest.UserId;
				var ltiLogin = new UserLoginInfo(loginProvider, providerKey);

				identity = await FuncUtils.TrySeveralTimesAsync(
					// ReSharper disable once AccessToDisposedClosure
					() => GetIdentityForLtiLogin(context, db, ltiLogin),
					3,
					() => Task.Delay(200)
				);
			}

			if (identity == null)
				throw new Exception("Can\'t authenticate identity for LTI user");

			log.Info($"Аутенфицирую пользователя по identity: {identity.Name}");
			context.OwinContext.Authentication.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);
		}

		private static async Task<ClaimsIdentity> GetIdentityForLtiLogin(LtiAuthenticatedContext context, ULearnDb db, UserLoginInfo ltiLogin)
		{
			
			var userManager = new ULearnUserManager(db);
			using (var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable))
			{
				var ltiLoginUser = await userManager.FindAsync(ltiLogin);
				if (ltiLoginUser != null)
				{
					log.Info($"Нашёл LTI-логин: провайдер {ltiLogin.LoginProvider}, идентификатор {ltiLogin.ProviderKey}, он принадлежит пользователю {ltiLoginUser.UserName} (Id = {ltiLoginUser.Id})");
					return await userManager.CreateIdentityAsync(ltiLoginUser, context.Options.SignInAsAuthenticationType);
				}

				log.Info($"Не нашёл LTI-логин: провайдер {ltiLogin.LoginProvider}, идентификатор {ltiLogin.ProviderKey}");

				if (IsAuthenticated(context.OwinContext))
				{
					var ulearnPrincipal = context.OwinContext.Authentication.User;
					log.Info($"Пришёл LTI-запрос на аутенфикацию, пользователь уже аутенфицирован на ulearn: {ulearnPrincipal.Identity.Name}. Прикрепляю к пользователю LTI-логин");
					await userManager.AddLoginAsync(ulearnPrincipal.Identity.GetUserId(), ltiLogin);

					return (ClaimsIdentity)ulearnPrincipal.Identity;
				}

				var usernameContext = new LtiGenerateUserNameContext(context.OwinContext, context.LtiRequest);
				await context.Options.Provider.GenerateUserName(usernameContext);

				if (string.IsNullOrEmpty(usernameContext.UserName))
					throw new Exception("Can't generate username");
				log.Info($"Сгенерировал имя пользователя для LTI-пользователя: {usernameContext.UserName}, ищу пользователя по этому имени");

				var ulearnUser = await userManager.FindByNameAsync(usernameContext.UserName);
				if (ulearnUser == null)
				{
					log.Info("Не нашёл пользователя с таким именем, создаю нового");
					ulearnUser = new ApplicationUser { UserName = usernameContext.UserName };
					var result = await userManager.CreateAsync(ulearnUser);
					if (!result.Succeeded)
					{
						var errors = string.Join("\n\n", result.Errors);
						throw new Exception("Can't create user for LTI: " + errors);
					}
				}

				await userManager.AddLoginAsync(ulearnUser.Id, ltiLogin);

				var identity = await userManager.CreateIdentityAsync(ulearnUser, context.Options.SignInAsAuthenticationType);

				transaction.Commit();
				return identity;
			}
		}

		/// <summary>
		/// Generate a valid application username using information from an LTI request. The default
		/// ASP.NET application using Microsoft Identity uses an email address as the username. This
		/// code will generate an "anonymous" email address if one is not supplied in the LTI request.
		/// </summary>
		/// <param name="context">Contains information about the login session as well as the LTI request.</param>
		/// <returns>A <see cref="Task"/> representing the completed operation.</returns>
		public static Task OnGenerateUserName(LtiGenerateUserNameContext context)
		{
			if (string.IsNullOrEmpty(context.LtiRequest.LisPersonEmailPrimary))
			{
				var username = context.LtiRequest.UserId;
				Uri url;
				if (string.IsNullOrEmpty(context.LtiRequest.ToolConsumerInstanceUrl)
					|| !Uri.TryCreate(context.LtiRequest.ToolConsumerInstanceUrl, UriKind.Absolute, out url))
				{
					context.UserName = string.Concat(username, "@", context.LtiRequest.ConsumerKey);
				}
				else
				{
					context.UserName = string.Concat(username, "@", url.Host);
				}
			}
			else
			{
				context.UserName = context.LtiRequest.LisPersonEmailPrimary;
			}

			return Task.FromResult<object>(null);
		}

		private static bool IsAuthenticated(IOwinContext context)
		{
			var auth = context.Authentication;
			return auth.User?.Identity != null && auth.User.Identity.IsAuthenticated;
		}
	}
}