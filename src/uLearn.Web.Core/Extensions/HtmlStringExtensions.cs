using Microsoft.AspNetCore.Html;

namespace uLearn.Web.Core.Extensions;

public static class HtmlStringExtensions
{
	public static HtmlString ToLegacyHtmlString(this HtmlString aspNetCoreHtmlString)
	{
		return new HtmlString(aspNetCoreHtmlString.Value);
	}
}