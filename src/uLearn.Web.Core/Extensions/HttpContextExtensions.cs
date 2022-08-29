using Microsoft.AspNetCore.Authentication;

namespace uLearn.Web.Core.Extensions;

public static class HttpContextExtensions
{
	public static async Task<AuthenticationScheme[]> GetExternalProvidersAsync(this HttpContext context)
	{
		if (context == null)
			throw new ArgumentNullException(nameof(context));

		var schemeProvider = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
		var schemes = await schemeProvider.GetAllSchemesAsync();

		return (
				from scheme in schemes
				where !string.IsNullOrEmpty(scheme.DisplayName)
				select scheme)
			.ToArray();
	}

	public static async Task<bool> IsProviderSupportedAsync(this HttpContext context, string provider)
	{
		if (context == null)
			throw new ArgumentNullException(nameof(context));

		return (
				from scheme in await context.GetExternalProvidersAsync()
				where string.Equals(scheme.Name, provider, StringComparison.OrdinalIgnoreCase)
				select scheme)
			.Any();
	}
}