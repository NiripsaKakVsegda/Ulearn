using Microsoft.Owin;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace uLearn.Web.Kontur.Passport
{
	public class KonturPassportAuthenticationMiddleware : AuthenticationMiddleware<KonturPassportAuthenticationOptions>
	{
		private readonly PassportClient passportClient;

		public KonturPassportAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, KonturPassportAuthenticationOptions options)
			: base(next, options)
		{
			passportClient = new PassportClient(options.ClientId, new []{"openid", "profile", "email"});

			if (Options.StateDataFormat == null)
			{
				var dataProtector = app.CreateDataProtector(
					typeof(KonturPassportAuthenticationMiddleware).FullName,
					Options.AuthenticationType,
					"v1");
				Options.StateDataFormat = new PropertiesDataFormat(dataProtector);
			}
		}

		protected override AuthenticationHandler<KonturPassportAuthenticationOptions> CreateHandler()
		{
			return new KonturPassportAuthenticationHandler(passportClient);
		}
	}
}