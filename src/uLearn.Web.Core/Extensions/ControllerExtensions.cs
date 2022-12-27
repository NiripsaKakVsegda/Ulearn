using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Controller = Microsoft.AspNetCore.Mvc.Controller;
using ViewContext = Microsoft.AspNetCore.Mvc.Rendering.ViewContext;
using ViewDataDictionary = Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary;

namespace uLearn.Web.Core.Extensions;

public static class ControllerExtensions
{
	public static async Task<string> RenderPartialViewToString(this Controller controller, string viewName, object model, bool partial = true)
	{
		// var vd = new ViewDataDictionary();
		// foreach (var item in controller.ViewData.Keys)
		// {
		// 	vd.Add(item, controller.ViewData[item]);
		// }
		//
		// vd.Model = model;
		// using (var sw = new StringWriter())
		// {
		// 	var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
		// 	var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, vd, controller.TempData, sw);
		// 	viewResult.View.Render(viewContext, sw);
		//
		// 	return sw.GetStringBuilder().ToString();
		// }

		if (string.IsNullOrEmpty(viewName))
			viewName = controller.ControllerContext.ActionDescriptor.ActionName;

		controller.ViewData.Model = model;

		using (var writer = new StringWriter())
		{
			var viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
			var viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !partial);

			if (viewResult.Success == false)
				return $"A view with the name {viewName} could not be found";

			var viewContext = new ViewContext(
				controller.ControllerContext,
				viewResult.View,
				controller.ViewData,
				controller.TempData,
				writer,
				new HtmlHelperOptions()
			);

			await viewResult.View.RenderAsync(viewContext);
		}

		return null;
	}

	public static string GetRedirectToUrlWithTrailingSlash(this Controller controller)
	{
		var requestUrl = controller.Request.GetDisplayUrl() ?? ""; //.Url?.AbsolutePath ??
		if (requestUrl != "" && requestUrl != "/" && !requestUrl.EndsWith("/"))
		{
			var redirectUrl = requestUrl + "/";
			return redirectUrl;
		}

		return null;
	}
}