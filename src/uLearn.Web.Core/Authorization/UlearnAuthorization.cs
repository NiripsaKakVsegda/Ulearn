using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Web.Api.Configuration;

namespace uLearn.Web.Core.Authorization;

public static class UlearnAuthorization
{
	public static void AddUlearnAuthorization(this IServiceCollection services, WebConfiguration configuration)
	{
		services.AddTransient<IClaimsTransformation, SysAdminClaimsTransformation>();
		services.AddScoped<IAuthorizationHandler, CourseRoleAuthorizationHandler>();
		services.AddScoped<IAuthorizationHandler, CourseAccessAuthorizationHandler>();
		services.AddAuthorization(options => new UlearnAuthorizationBuilder().Build(configuration, options));
	}
}