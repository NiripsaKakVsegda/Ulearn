using System;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security;
using Owin;

namespace uLearn.Web.Stepik.OAuth
{
	public static class StepikOAuthAuthenticationExtensions
	{
		public static IAppBuilder UseStepikOAuthAuthentication(this IAppBuilder app, StepikOAuthAuthenticationOptions options)
		{
			if (app == null)
			{
				throw new ArgumentNullException(nameof(app));
			}
			if (options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			app.Use(typeof(StepikOAuthAuthenticationMiddleware), app, options);
			return app;
		}

		public static IAppBuilder UseStepikOAuthAuthentication(
			this IAppBuilder app,
			string clientId,
			string clientSecret)
		{
			return UseStepikOAuthAuthentication(
				app,
				new StepikOAuthAuthenticationOptions("/signin-stepik", StepikOAuthAuthenticationHandler.OnAuthorizationCodeReceived)
				{
					ClientId = clientId,
					ClientSecret = clientSecret,
					SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType(),
					Configuration = new OpenIdConnectConfiguration
					{
						AuthorizationEndpoint = "https://stepik.org/oauth2/authorize/",
						TokenEndpoint = "https://stepik.org/oauth2/token/",
					},
				});
		}
	}
}