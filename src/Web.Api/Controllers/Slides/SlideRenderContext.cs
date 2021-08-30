using Microsoft.AspNetCore.Mvc;
using Ulearn.Core.Courses.Slides;

namespace Ulearn.Web.Api.Controllers.Slides
{
	public class SlideRenderContext
	{
		public string CourseId;
		public Slide Slide;
		public string BaseUrlApi;
		public string BaseUrlWeb;
		public bool RemoveHiddenBlocks;
		public string VideoAnnotationsGoogleDoc;
		public IUrlHelper UrlHelper;
		public string UserId;

		public SlideRenderContext(string courseId, Slide slide, string userId, string baseUrlApi, string baseUrlWeb, bool removeHiddenBlocks,
			string videoAnnotationsGoogleDoc, IUrlHelper urlHelper)
		{
			CourseId = courseId;
			Slide = slide;
			BaseUrlApi = baseUrlApi;
			BaseUrlWeb = baseUrlWeb;
			RemoveHiddenBlocks = removeHiddenBlocks;
			VideoAnnotationsGoogleDoc = videoAnnotationsGoogleDoc;
			UrlHelper = urlHelper;
			UserId = userId;
		}
	}
}