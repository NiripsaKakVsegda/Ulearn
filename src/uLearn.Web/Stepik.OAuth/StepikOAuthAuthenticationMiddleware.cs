using log4net;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

namespace uLearn.Web.Stepik.OAuth
{
	public class StepikOAuthAuthenticationMiddleware : OpenIdConnectAuthenticationMiddleware
	{
		private readonly ILogger owinLogger;
		private readonly ILog log;

		public StepikOAuthAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, StepikOAuthAuthenticationOptions options)
			: base(next, app, options)
		{
			owinLogger = app.CreateLogger<StepikOAuthAuthenticationMiddleware>();
			log = LogManager.GetLogger(typeof(StepikOAuthAuthenticationMiddleware));
		}

		protected override AuthenticationHandler<OpenIdConnectAuthenticationOptions> CreateHandler()
		{
			return new StepikOAuthAuthenticationHandler(owinLogger);
		}
	}
}