using System.Text;
using System.Text.RegularExpressions;
using LtiLibrary.Core.Extensions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;
using Web.Api.Configuration;

namespace uLearn.Web.Core.Controllers;

public class ErrorsController : Controller
{
	private string IndexHtmlPath => configuration.OldWebConfig["ulearn.react.index.html"];
	private string CspHeader => configuration.OldWebConfig["ulearn.web.cspHeader"];

	private WebConfiguration configuration;
	private static DirectoryInfo AppDirectory => new DirectoryInfo(Ulearn.Core.Utils.GetAppPath());

	private readonly List<string> excludedPrefixes;
	private readonly byte[] content;

	public ErrorsController(WebConfiguration configuration, IHostEnvironment environment)
		: this(configuration, environment, excludedPrefixes: new List<string>
		{
			//"/elmah/",
			"/Certificate/",
			"/Analytics/ExportCourseStatisticsAs",
			"/Exercise/StudentZip",
			"/Content/"
		})
	{
	}

	public ErrorsController(WebConfiguration configuration, IHostEnvironment environment, List<string> excludedPrefixes )
	{
		this.configuration = configuration;
		this.excludedPrefixes = excludedPrefixes;
		var file = AppDirectory.GetFile(environment.ContentRootPath + configuration.OldWebConfig["ulearn.react.index.html"]);
		content = System.IO.File.ReadAllBytes(file.FullName);
		content = InsertFrontendConfiguration(content);
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

	public ActionResult Error404()
	{
		var httpContext = HttpContext;
		var uri = new Uri(httpContext.Request.GetDisplayUrl());
		foreach (var prefix in excludedPrefixes)
			if (uri != null && uri.LocalPath.StartsWith(prefix))
				return Real404();

		var acceptHeader = httpContext.Request.Headers.Accept.Count == 0 ? "" : httpContext.Request.Headers.Accept.ToString();
		var cspHeader = CspHeader ?? "";
		if (acceptHeader.Contains("text/html") && httpContext.Request.Method == "GET")
		{
			httpContext.Response.Headers.Add("Content-Security-Policy-Report-Only", cspHeader);
			return new FileContentResult(content, "text/html");
		}

		return Real404();
	}

	private ActionResult Real404()
	{
		return new NotFoundResult();
	}

	public ActionResult Error500()
	{
		return View("Error");
	}
}