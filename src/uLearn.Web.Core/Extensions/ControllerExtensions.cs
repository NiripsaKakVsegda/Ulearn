using Controller = Microsoft.AspNetCore.Mvc.Controller;
using ViewContext = Microsoft.AspNetCore.Mvc.Rendering.ViewContext;
using ViewDataDictionary = Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary;

namespace uLearn.Web.Core.Extensions;

// public static class ControllerExtensions
// {
// 	public static string RenderPartialViewToString(this Controller controller, string viewName, object model)
// 	{
// 		var vd = new ViewDataDictionary();
// 		foreach (var item in controller.ViewData.Keys)
// 		{
// 			vd.Add(item, controller.ViewData[item]);
// 		}
//
// 		vd.Model = model;
// 		using var sw = new StringWriter();
// 		var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
// 		var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, vd, controller.TempData, sw);
// 		viewResult.View.Render(viewContext, sw);
//
// 		return sw.GetStringBuilder().ToString();
// 	}
//
// 	public static string GetRedirectToUrlWithTrailingSlash(this Controller controller)
// 	{
// 		var requestUrl = controller.Request.Url?.AbsolutePath ?? "";
// 		if (requestUrl != "" && requestUrl != "/" && !requestUrl.EndsWith("/"))
// 		{
// 			var redirectUrl = requestUrl + "/";
// 			return redirectUrl;
// 		}
//
// 		return null;
// 	}
// }