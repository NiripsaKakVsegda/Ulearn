using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using uLearn.Web.Core.Authentication.External.Google;
using uLearn.Web.Core.Authentication.External.Kontur;
using uLearn.Web.Core.Authentication.External.Vkontakte;
using Web.Api.Configuration;

namespace uLearn.Web.Core.Authentication;

public static class UlearnAuthentication
{
	public static void AddUlearnAuthentication(this IServiceCollection services, WebConfiguration configuration)
	{
		services.ConfigureApplicationCookie(options =>
		{
			options.Cookie.Name = configuration.Web.CookieName;
			options.ExpireTimeSpan = TimeSpan.FromDays(14);
			options.Cookie.Domain = configuration.Web.CookieDomain;
			options.Cookie.SameSite = SameSiteMode.None;
			options.LoginPath = "/login";
		});

		services
			.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = UlearnAuthenticationConstants.DefaultAuthenticationScheme;
				options.DefaultScheme = UlearnAuthenticationConstants.DefaultAuthenticationScheme;
				options.DefaultChallengeScheme = UlearnAuthenticationConstants.DefaultAuthenticationScheme;
			})
			.AddCookie(options =>
			{
				options.LoginPath = "/login";
				options.LogoutPath = "/account/logout";
			})
			.AddVkontakte(
				VkontakteConstants.DefaultAuthenticationType,
				VkontakteConstants.DefaultAuthenticationType,
				options => new VkontakteConfigBuilder().Build(configuration, options))
			.AddGoogle(options => new GoogleConfigBuilder().Build(configuration, options));

		services
			.AddAuthentication(options => { options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme; })
			.AddOpenIdConnect(KonturPassportConstants.AuthenticationType, KonturPassportConstants.AuthenticationType, options => new KonturConfigBuilder().Build(configuration, options));
	}
}