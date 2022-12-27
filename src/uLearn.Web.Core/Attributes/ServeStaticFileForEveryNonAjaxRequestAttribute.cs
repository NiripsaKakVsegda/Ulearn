using System.Text;
using System.Text.RegularExpressions;
using LtiLibrary.Core.Extensions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ulearn.Core.Configuration;
using Web.Api.Configuration;

namespace uLearn.Web.Core.Attributes;

public class ServeStaticFileForEveryNonAjaxRequestAttribute : ActionFilterAttribute
{
	private readonly List<string> excludedPrefixes;
	private readonly List<Regex> excludedRegexps;
	private readonly byte[] content;
	private readonly WebConfiguration configuration;

	public ServeStaticFileForEveryNonAjaxRequestAttribute(
		FileInfo file, 
		List<string> excludedPrefixes,
		List<Regex> excludedRegexps,
		WebConfiguration configuration)
	{
		this.excludedPrefixes = excludedPrefixes;
		this.excludedRegexps = excludedRegexps;
		content = File.ReadAllBytes(file.FullName);
		content = InsertFrontendConfiguration(content);
		this.configuration = configuration;
	}

	private static byte[] InsertFrontendConfiguration(byte[] content)
	{
		var configuration = ApplicationConfiguration.Read<WebConfiguration>();
		var frontendConfigJson = configuration.Frontend.ToJsonString();
		var decodedContent = Encoding.UTF8.GetString(content);
		var regex = new Regex(@"(window.config\s*=\s*)(\{\})");
		var contentWithConfig = regex.Replace(decodedContent, "$1" + frontendConfigJson);

		return Encoding.UTF8.GetBytes(contentWithConfig);
	}

	public override void OnActionExecuting(ActionExecutingContext filterContext)
	{
		var httpContext = filterContext.HttpContext;
		var url = new Uri(httpContext.Request.GetDisplayUrl());

		if (excludedPrefixes.Any(prefix => url.LocalPath.StartsWith(prefix)))
			return;

		if (excludedRegexps.Any(regex => regex.IsMatch(url.LocalPath)))
			return;

		var acceptHeader = httpContext.Request.Headers.Accept.SelectMany(a => a.Split(','));
		var cspHeader = configuration.Web.CspHeader ?? "";
		
		if (!acceptHeader.Contains("text/html") || httpContext.Request.Method != "GET")
			return;
		
		filterContext.HttpContext.Response.Headers.Add("Content-Security-Policy-Report-Only", cspHeader);
		filterContext.Result = new FileContentResult(content, "text/html");
	}

	public override void OnResultExecuting(ResultExecutingContext filterContext)
	{
		/* Add no-cache headers for correct working of react application (otherwise clicking on `back` button in browsers loads cached not-reacted version) */
		filterContext.HttpContext.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
		filterContext.HttpContext.Response.Headers["Expires"] = "-1";
		filterContext.HttpContext.Response.Headers["Pragma"] = "no-cache";

		base.OnResultExecuting(filterContext);
	}
}