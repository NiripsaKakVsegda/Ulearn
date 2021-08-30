using System;
using Microsoft.Owin.Security;
using Owin;

namespace uLearn.Web.Microsoft.Owin.Security.VK
{
	public static class VkAuthenticationExtensions
	{
		public static IAppBuilder UseVkAuthentication(this IAppBuilder app, VkAuthenticationOptions options)
		{
			if (app == null)
			{
				throw new ArgumentNullException(nameof(app));
			}

			if (options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			app.Use(typeof(VkAuthenticationMiddleware), app, options);
			return app;
		}

		public static IAppBuilder UseVkAuthentication(
			this IAppBuilder app,
			string appId,
			string appSecret)
		{
			return UseVkAuthentication(
				app,
				new VkAuthenticationOptions
				{
					AppId = appId,
					AppSecret = appSecret,
					SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType(),
				});
		}
	}
}