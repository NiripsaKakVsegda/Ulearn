using System.Security.Claims;
using ApprovalUtilities.Utilities;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using uLearn.Web.Core.Utils;
using Vostok.Logging.Abstractions;
using Web.Api.Configuration;

namespace uLearn.Web.Core.Authentication.External.Kontur;

public class KonturConfigBuilder : IConfigBuilder<OpenIdConnectOptions>
{
	private static ILog log => LogProvider.Get().ForContext(typeof(KonturConfigBuilder));
	
	public void Build(WebConfiguration configuration, OpenIdConnectOptions options)
	{
		var passportAppId = configuration.OldWebConfig["oicd.konturPassport.clientId"];
		var passportAppSecret = configuration.OldWebConfig["oicd.konturPassport.clientSecret"];
		var passportAuthority = configuration.OldWebConfig["oicd.konturPassport.authority"];
		var returnUrl = configuration.OldWebConfig["oicd.konturPassport.returnUrl"];

		options.Authority = passportAuthority;
		options.ClientId = passportAppId;
		options.ClientSecret = passportAppSecret;
		options.CallbackPath = returnUrl;

		options.Scope.AddAll(new[] { "openid", "profile", "email" });

		options.Events.OnTicketReceived = context =>
		{
			var user = context.Principal;
			var userClaims = user.Claims.ToList();
			const string xmlSchemaForStringType = "http://www.w3.org/2001/XMLSchema#string";

			log.Info($"Received follow user claims from Kontur.Passport server: {string.Join(", ", userClaims.Select(c => c.Type + ": " + c.Value))}");

			var login = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			var sid = userClaims.FirstOrDefault(c => c.Type == KonturPassportConstants.SidClaimType)?.Value;
			var avatarUrl = userClaims.FirstOrDefault(c => c.Type == KonturPassportConstants.AvatarUrlClaimType)?.Value;
			var realNameParts = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value.Split(' ');

			var identity = new ClaimsIdentity();
			if (avatarUrl != null)
				identity.AddClaim(new Claim("AvatarUrl", avatarUrl, xmlSchemaForStringType));
			if (realNameParts is { Length: > 0 })
			{
				/* Suppose that Гейн Андрей Александрович is Surname (Гейн), GivenName (Андрей) and other. So we splitted name from Kontur.Passport into parts */
				identity.AddClaim(new Claim(ClaimTypes.Surname, realNameParts[0], xmlSchemaForStringType));
				if (realNameParts.Length > 1)
					identity.AddClaim(new Claim(ClaimTypes.GivenName, realNameParts[1], xmlSchemaForStringType));
			}

			/* Replace name from Kontur\andgein to andgein */
			if (login != null && login.Contains('\\'))
				login = login[(login.IndexOf('\\') + 1)..];

			if (login != null)
			{
				identity.AddClaim(new Claim(ClaimTypes.Name, login, xmlSchemaForStringType));
				identity.AddClaim(new Claim("KonturLogin", login, xmlSchemaForStringType));
			}

			user.AddIdentity(identity);

			return Task.CompletedTask;
		};
	}
}