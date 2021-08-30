using System;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using Ulearn.Common.Extensions;

namespace Stepik.Api
{
	public static class OAuth
	{
		private const string scope = "read write";
		private const string oAuthBaseUrl = "https://stepik.org/oauth2/authorize/";

		public static string GetAuthorizationUrl(string clientId, string redirectUri, string state)
		{
			var parameters = new NameValueCollection
			{
				["response_type"] = "code",
				["scope"] = scope,
				["redirect_uri"] = redirectUri,
				["client_id"] = clientId,
				["state"] = state,
			};
			var uriBuilder = new UriBuilder(oAuthBaseUrl) { Query = parameters.ToQueryString() };
			return uriBuilder.ToString();
		}

		// TODO (andgein): DataProtectionScope.LocalMachine is bad idea because second request may be handled by another web server
		public static string EncryptState(string state)
		{
			var stateBytes = Encoding.UTF8.GetBytes(state);
			return Convert.ToBase64String(ProtectedData.Protect(stateBytes, null, DataProtectionScope.LocalMachine));
		}

		public static string DecryptState(string encryptedState)
		{
			var unpacked = Convert.FromBase64String(encryptedState);
			return Encoding.UTF8.GetString(ProtectedData.Unprotect(unpacked, null, DataProtectionScope.LocalMachine));
		}
	}
}