using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;
using uLearn.Web.Core.Utils;
using Web.Api.Configuration;

namespace uLearn.Web.Core.Authentication.External.Google;

public class GoogleConfigBuilder: IConfigBuilder<GoogleOptions>
{
	public void Build(WebConfiguration configuration, GoogleOptions options)
	{
		var clientId = configuration.OldWebConfig["oauth.google.clientId"];
		var clientSecret = configuration.OldWebConfig["oauth.google.clientSecret"];
		options.ClientId = clientId;
		options.ClientSecret = clientSecret;

		options.Scope.Add("profile");
		options.Events.OnCreatingTicket = context =>
		{
			var user = context.User;

			if (user.TryGetProperty("given_name", out var name))
				context.Identity.AddClaim(new Claim(ClaimTypes.GivenName, name.GetString()));

			if (user.TryGetProperty("family_name", out var surname))
				context.Identity.AddClaim(new Claim(ClaimTypes.Surname, surname.GetString()));

			if (user.TryGetProperty("email", out var email))
				context.Identity.AddClaim(new Claim(ClaimTypes.Email, email.GetString()));

			if (user.TryGetProperty("picture", out var picture))
				context.Identity.AddClaim(new Claim("AvatarUrl", picture.GetString()));

			return Task.CompletedTask;
		};
	}
}