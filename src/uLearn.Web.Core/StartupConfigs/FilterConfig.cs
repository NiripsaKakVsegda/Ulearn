using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Filters;
using Ulearn.Common.Extensions;
using uLearn.Web.Core.Attributes;
using Web.Api.Configuration;

namespace uLearn.Web.Core.StartupConfigs;

public static class FilterConfig
{
	public static void RegisterGlobalFilters(FilterCollection filters, IHostEnvironment environment, WebConfiguration configuration)
	{
		/* Next filter serves built index.html from ../Frontend/build/ (appSettings/ulearn.react.index.html).
			Before running this code build Frontend project via `yarn build` or `npm run build` */
		var appDirectory = new DirectoryInfo(Ulearn.Core.Utils.GetAppPath());
		var index = configuration.OldWebConfig["ulearn.react.index.html"];
		filters.Add(
			new ServeStaticFileForEveryNonAjaxRequestAttribute(
				appDirectory.GetFile(environment.ContentRootPath + index),
				excludedPrefixes: new List<string>
				{
					//"/elmah/",
					"/Certificate/",
					"/Analytics/ExportCourseStatisticsAs",
					"/Content/",
					"/Courses/",
					"/Exercise/StudentZip"
				}, excludedRegexps: new List<Regex>
				{
					new("^/Exercise/.*/.*/StudentZip/.*", RegexOptions.Compiled | RegexOptions.IgnoreCase)
				},
				configuration)
		);
	}
}