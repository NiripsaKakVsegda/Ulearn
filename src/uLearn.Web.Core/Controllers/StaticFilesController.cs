using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Api;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses;
using Vostok.Clusterclient.Core.Model;
using Web.Api.Client;

namespace uLearn.Web.Core.Controllers;

[Obsolete("Use api")] // Для openedu и stepik
[AllowAnonymous]
// Нельзя, чтобы в папке c web была подпапка Courses или ярлык на нее (как было раньше).
// Тогда пользователь будет видеть 500 и писаться ошибка 'Server cannot append header after HTTP headers have been sent' at System.Web.HttpResponse.AppendCookie(HttpCookie cookie)
public class StaticFilesController : Controller
{
	private static readonly UlearnConfiguration config = ApplicationConfiguration.Read<UlearnConfiguration>();

	public async Task<ActionResult> CourseFile(string courseId, string path)
	{
		if (string.IsNullOrEmpty(courseId) || string.IsNullOrEmpty(path) || path.Contains("..") || path.Contains("/courses/"))
			return NotFound();
		var extension = Path.GetExtension(path);
		var mimeType = CourseStaticFilesHelper.AllowedExtensions.GetOrDefault(extension);
		if (mimeType == null)
			return NotFound();
		IWebApiClient webApiClient = new WebApiClient(new ApiClientSettings(config.BaseUrlApi));
		var response = await webApiClient.GetCourseStaticFile(courseId, path);
		if (response == null)
			return new StatusCodeResult(500);
		if (response.Code != ResponseCode.Ok)
			return new StatusCodeResult((int)response.Code);
		if (response.HasStream)
			return new FileStreamResult(response.Stream, mimeType);
		if (response.HasContent)
			return new FileContentResult(response.Content.ToArray(), mimeType);
		return new StatusCodeResult(500);
	}
}