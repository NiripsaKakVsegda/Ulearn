using System.Text;
using System.Text.RegularExpressions;
using LtiLibrary.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;
using Web.Api.Configuration;

namespace uLearn.Web.Core.Controllers;

/* Single Page Application Controller — controller which return SPA's index.html for each request.
	This controllers stands last in RouteConfig.cs */
public class SpaController : Controller
{
	private static DirectoryInfo AppDirectory => new(Ulearn.Core.Utils.GetAppPath());
	
	private readonly WebConfiguration configuration;

	private readonly byte[] content;

	public SpaController(IWebHostEnvironment environment, WebConfiguration configuration)
	{
		this.configuration = configuration;
		content = GetSpaIndexHtml(environment.WebRootPath);
	}

	public static byte[] GetSpaIndexHtml(string wwwPath)
	{
		var file = AppDirectory.GetFile(wwwPath + "//index.html");
		var content = System.IO.File.ReadAllBytes(file.FullName);
		return InsertFrontendConfiguration(content);
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

	public ActionResult IndexHtml()
	{
		var cspHeader = configuration.Web.CspHeader ?? "";

		HttpContext.Response.Headers.Add("Content-Security-Policy-Report-Only", cspHeader);
		HttpContext.Response.Headers.Add("ReactRender", "true");

		return new FileContentResult(content, "text/html");
	}
}