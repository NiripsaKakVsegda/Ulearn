namespace uLearn.Web.Core.Extensions;

public static class HttpRequestExtensions
{
	private static readonly string xSchemeHeaderName = "X-Scheme";

	/* Return scheme from request of from header X-Scheme if request has been proxied by cloudflare or nginx or ... */
	public static string GetRealScheme(this HttpRequest request)
	{
		var scheme = request.Headers[xSchemeHeaderName];
		return scheme.Count == 0 ? request.Scheme : scheme.ToString();
	}

	public static int GetRealPort(this HttpRequest request)
	{
		if (request.Scheme == "http" && request.HttpContext.Connection.LocalPort == 80 && request.GetRealScheme() == "https")
			return 443;
		return request.HttpContext.Connection.LocalPort;
	}
}