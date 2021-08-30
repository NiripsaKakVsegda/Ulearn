using System;
using Microsoft.Owin.Security;
using Owin;

namespace uLearn.Web.Kontur.Passport
{
	public static class KonturPassportAuthenticationExtensions
	{
		public static IAppBuilder UseKonturPassportAuthentication(this IAppBuilder app, KonturPassportAuthenticationOptions options)
		{
			if (app == null)
			{
				throw new ArgumentNullException(nameof(app));
			}

			if (options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			app.Use(typeof(KonturPassportAuthenticationMiddleware), app, options);
			return app;
		}

		public static IAppBuilder UseKonturPassportAuthentication(this IAppBuilder app, string clientId)
		{
			return app.UseKonturPassportAuthentication(new KonturPassportAuthenticationOptions
			{
				ClientId = clientId,
				SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType()
			});
		}
	}
}