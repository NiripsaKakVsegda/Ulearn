using System;
using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Security.Claims;
using System.Threading.Tasks;
using log4net;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;

namespace uLearn.Web.Stepik.OAuth
{
	public class StepikOAuthAuthenticationHandler : OpenIdConnectAuthenticationHandler
	{	
		private static readonly ILog log = LogManager.GetLogger(typeof(StepikOAuthAuthenticationHandler));

		public StepikOAuthAuthenticationHandler(ILogger owinLogger) : base(owinLogger)
		{
		}

		/* Copy-paste from OpenIdConnectAuthenticationHandler */
		private AuthenticationProperties GetPropertiesFromState(string state)
		{
			int num;
			if (string.IsNullOrWhiteSpace(state) || (num = state.IndexOf("OpenIdConnect.AuthenticationProperties", StringComparison.Ordinal)) == -1)
				return (AuthenticationProperties)null;
			int index = num + "OpenIdConnect.AuthenticationProperties".Length;
			if (index == -1 || index == state.Length || (int)state[index] != 61)
				return (AuthenticationProperties)null;
			int startIndex = index + 1;
			int length = state.Substring(startIndex, state.Length - startIndex).IndexOf("&", StringComparison.Ordinal);
			if (length == -1)
				return this.Options.StateDataFormat.Unprotect(Uri.UnescapeDataString(state.Substring(startIndex).Replace('+', ' ')));
			return this.Options.StateDataFormat.Unprotect(Uri.UnescapeDataString(state.Substring(startIndex, length).Replace('+', ' ')));
		}

		protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
		{
			if (Options.CallbackPath.HasValue && Options.CallbackPath != Request.PathBase + Request.Path)
				return null;

			var state = Request.Query.Get("state");
			var error = Request.Query.Get("error");
			var code = Request.Query.Get("code");
			var openIdConnectMessage = new OpenIdConnectMessage
			{
				Error = error,
				State = state,
				Code = code,
			};

			ExceptionDispatchInfo authFailedEx;
			try
			{
				var properties = GetPropertiesFromState(state);
				if (properties == null)
				{
					log.Warn("The state field is missing or invalid.");
					return null;
				}
				if (!string.IsNullOrWhiteSpace(openIdConnectMessage.Error))
					throw new OpenIdConnectProtocolException(
						string.Format(CultureInfo.InvariantCulture, openIdConnectMessage.Error, typeof(Exception), openIdConnectMessage.ErrorDescription ?? string.Empty, openIdConnectMessage.ErrorUri ?? string.Empty));
				
				if (openIdConnectMessage.Code != null)
				{
					var authorizationCodeReceivedNotification = new AuthorizationCodeReceivedNotification(Context, Options)
					{
						Code = openIdConnectMessage.Code,
						ProtocolMessage = openIdConnectMessage,
						RedirectUri = properties.Dictionary.ContainsKey("OpenIdConnect.Code.RedirectUri") ? properties.Dictionary["OpenIdConnect.Code.RedirectUri"] : string.Empty
					};
					await Options.Notifications.AuthorizationCodeReceived(authorizationCodeReceivedNotification);
				}
				var identity = new ClaimsIdentity();
				identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, code));
				return new AuthenticationTicket(identity, properties);
			}
			catch (Exception ex)
			{
				authFailedEx = ExceptionDispatchInfo.Capture(ex);
			}
			if (authFailedEx != null)
			{
				log.Error("Exception occurred while processing message: ", authFailedEx.SourceException);
				authFailedEx.Throw();
			}
			return null;
		}

		public static Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification notification)
		{
			log.Info($"OnAuthorizationCodeReceived. Ticket: {notification.AuthenticationTicket}, code: {notification.Code}, redirect uri: {notification.RedirectUri}");
			return Task.Delay(100);
		}
	}
}