namespace uLearn.Web.Core.External.Kontur;

public static class KonturPassportAuthenticationExtensionsExtensions
{
	public static IApplicationBuilder UseKonturPassportAuthentication(this IApplicationBuilder app, KonturPassportAuthenticationOptions options)
	{
		if (app == null)
		{
			throw new ArgumentNullException(nameof(app));
		}

		if (options == null)
		{
			throw new ArgumentNullException(nameof(options));
		}

		//app.Use(typeof(KonturPassportAuthenticationMiddleware), app, options);
		return app;
	}

	public static IApplicationBuilder UseKonturPassportAuthentication(this IApplicationBuilder app, string clientId, string signInAsAuthenticationType)
	{
		return app.UseKonturPassportAuthentication(new KonturPassportAuthenticationOptions
		{
			ClientId = clientId,
			SignInAsAuthenticationType = signInAsAuthenticationType
		});
	}
}