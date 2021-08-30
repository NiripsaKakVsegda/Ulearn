using System;
using System.Threading.Tasks;
using System.Web.Configuration;
using Microsoft.Owin;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode;

namespace uLearn.Web.Stepik.OAuth
{
	public class StepikOAuthAuthenticationOptions : OpenIdConnectAuthenticationOptions
	{
		public StepikOAuthAuthenticationOptions(string authenticationType, string scope, string relativeRedirectUri, Func<AuthorizationCodeReceivedNotification, Task> authorizationCodeReceived)
			: base(authenticationType)
		{
			Caption = authenticationType;
			RedirectUri = GetAbsoluteRedirectUri(relativeRedirectUri);
			CallbackPath = new PathString(relativeRedirectUri);
			AuthenticationMode = AuthenticationMode.Active;
			Scope = scope;
			ResponseType = "code";
			
			Notifications = new OpenIdConnectAuthenticationNotifications();
			Notifications.AuthorizationCodeReceived = authorizationCodeReceived;
		}

		private string GetAbsoluteRedirectUri(string relativeRedirectUri)
		{
			var baseUrl = WebConfigurationManager.AppSettings["ulearn.baseUrl"] ?? "";
			var uriBuilder = new UriBuilder(baseUrl) { Path = relativeRedirectUri };
			return uriBuilder.ToString();
		}

		public StepikOAuthAuthenticationOptions(string relativeRedirectUri, Func<AuthorizationCodeReceivedNotification, Task> authorizationCodeReceived)
			: this(StepikOAuthConstants.AuthenticationType, StepikOAuthConstants.Scope, relativeRedirectUri, authorizationCodeReceived)
		{
		}
	}
}