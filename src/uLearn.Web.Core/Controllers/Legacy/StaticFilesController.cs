using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using Ulearn.Common.Api;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses;
using Vostok.Clusterclient.Core.Model;
using Web.Api.Client;

namespace uLearn.Web.Controllers;

[Obsolete("Use api")] // Для openedu и stepik
[AllowAnonymous]
// Нельзя, чтобы в папке c web была подпапка Courses или ярлык на нее (как было раньше).
// Тогда пользователь будет видеть 500 и писаться ошибка 'Server cannot append header after HTTP headers have been sent' at System.Web.HttpResponse.AppendCookie(HttpCookie cookie)
public class StaticFilesController : Controller
{
	private static readonly UlearnConfiguration config = ApplicationConfiguration.Read<UlearnConfiguration>();

	public async Task<ActionResult> CourseFile(string courseId, string path)
	{
		if (path.Contains(".."))
			return HttpNotFound();
		var extension = Path.GetExtension(path);
		var mimeType = CourseStaticFilesHelper.AllowedExtensions.GetOrDefault(extension);
		if (mimeType == null)
			return HttpNotFound();
		IWebApiClient webApiClient = new WebApiClient(new ApiClientSettings(config.BaseUrlApi));
		var response = await webApiClient.GetCourseStaticFile(courseId, path);
		if (response == null)
			return new HttpStatusCodeResult(500);
		if (response.Code != ResponseCode.Ok)
			return new HttpStatusCodeResult((int)response.Code);
		if (response.HasStream)
			return new FileStreamResult(response.Stream, mimeType);
		if (response.HasContent)
			return new FileContentResult(response.Content.ToArray(), mimeType);
		return new HttpStatusCodeResult(500);
	}
}