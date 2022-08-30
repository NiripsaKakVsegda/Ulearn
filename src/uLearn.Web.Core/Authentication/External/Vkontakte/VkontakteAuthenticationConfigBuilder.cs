using System.Security.Claims;
using AspNet.Security.OAuth.Vkontakte;
using uLearn.Web.Core.Utils;
using Web.Api.Configuration;

namespace uLearn.Web.Core.Authentication.External.Vkontakte;

public class VkontakteConfigBuilder : IConfigBuilder<VkontakteAuthenticationOptions>
{
	public void Build(WebConfiguration configuration, VkontakteAuthenticationOptions options)
	{
		var vkAppId = configuration.OldWebConfig["oauth.vk.appId"];
		var vkAppSecret = configuration.OldWebConfig["oauth.vk.appSecret"];

		options.ClientId = vkAppId;
		options.ClientSecret = vkAppSecret;

		options.Fields.Add("sex");
		options.Fields.Add("photo_50");

		options.Events.OnCreatingTicket = context =>
		{
			var user = context.User;

			if (user.TryGetProperty("name", out var name))
				context.Identity.AddClaim(new Claim(ClaimTypes.GivenName, name.GetString()));

			if (user.TryGetProperty("surname", out var surname))
				context.Identity.AddClaim(new Claim(ClaimTypes.Surname, surname.GetString()));

			if (user.TryGetProperty("photo", out var photo))
				context.Identity.AddClaim(new Claim("AvatarUrl", photo.GetString()));

			if (user.TryGetProperty("email", out var email))
				context.Identity.AddClaim(new Claim(ClaimTypes.Email, email.GetString()));

			if (user.TryGetProperty("sex", out var sex))
				context.Identity.AddClaim(new Claim(ClaimTypes.Gender, sex.GetInt32().ToString()));

			return Task.CompletedTask;
		};
	}
}