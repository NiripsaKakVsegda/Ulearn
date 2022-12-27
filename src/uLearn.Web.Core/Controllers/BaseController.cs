using Microsoft.AspNetCore.Mvc;

namespace uLearn.Web.Core.Controllers;

public class BaseController : Controller
{
	private readonly List<string> utmTags = new() { "utm_source", "utm_medium", "utm_term", "utm_content", "utm_name" };

	protected RedirectToActionResult RedirectToAction(string actionName, string controllerName, RouteValueDictionary routeValues)
	{
		var redirectResult = base.RedirectToAction(actionName, controllerName, routeValues);
		return AddUtmTagsInRedirectResult(redirectResult);
	}

	protected RedirectToActionResult RedirectToActionPermanent(string actionName, string controllerName, RouteValueDictionary routeValues)
	{
		var redirectResult = base.RedirectToActionPermanent(actionName, controllerName, routeValues);
		return AddUtmTagsInRedirectResult(redirectResult);
	}

	protected RedirectToRouteResult RedirectToRoute(string routeName, RouteValueDictionary routeValues)
	{
		var redirectResult = base.RedirectToRoute(routeName, routeValues);
		return AddUtmTagsInRedirectResult(redirectResult);
	}

	protected RedirectToRouteResult RedirectToRoutePermanent(string routeName, RouteValueDictionary routeValues)
	{
		var redirectResult = base.RedirectToRoutePermanent(routeName, routeValues);
		return AddUtmTagsInRedirectResult(redirectResult);
	}

	private RedirectToRouteResult AddUtmTagsInRedirectResult(RedirectToRouteResult redirectResult)
	{
		foreach (var utmTag in utmTags)
		{
			var utmTagValue = HttpContext.Request.Query[utmTag];
			if (!string.IsNullOrEmpty(utmTagValue))
				redirectResult.RouteValues?.Add(utmTag, utmTagValue);
		}

		return redirectResult;
	}

	private RedirectToActionResult AddUtmTagsInRedirectResult(RedirectToActionResult redirectResult)
	{
		foreach (var utmTag in utmTags)
		{
			var utmTagValue = HttpContext.Request.Query[utmTag];
			if (!string.IsNullOrEmpty(utmTagValue))
				redirectResult.RouteValues?.Add(utmTag, utmTagValue);
		}

		return redirectResult;
	}

	protected string GetRealClientIp()
	{
		var xForwardedFor = Request.Headers["X-Forwarded-For"];
		if (string.IsNullOrEmpty(xForwardedFor))
			return Request.Host.Value; //UserHostAddress
		return xForwardedFor.FirstOrDefault() ?? "";
	}
}