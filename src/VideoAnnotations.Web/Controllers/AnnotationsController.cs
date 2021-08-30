using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.VideoAnnotations.Api.Models.Parameters.Annotations;
using Ulearn.VideoAnnotations.Api.Models.Responses.Annotations;
using Ulearn.VideoAnnotations.Web.Annotations;

namespace Ulearn.VideoAnnotations.Web.Controllers
{
	[Route("/")]
	public class AnnotationsController : BaseController
	{
		private readonly IGoogleDocApiClient googleDocApiClient;
		private readonly IAnnotationsParser annotationsParser;
		private readonly IAnnotationsCache annotationsCache;

		public AnnotationsController(IGoogleDocApiClient googleDocApiClient, IAnnotationsParser annotationsParser, IAnnotationsCache annotationsCache)
		{
			this.googleDocApiClient = googleDocApiClient;
			this.annotationsParser = annotationsParser;
			this.annotationsCache = annotationsCache;
		}

		[HttpGet(Api.Urls.Annotations)]
#pragma warning disable 1998
		public async Task<ActionResult<AnnotationsResponse>> Annotations([FromQuery] AnnotationsParameters parameters)
#pragma warning restore 1998
		{
			Dictionary<string, Annotation> annotations;
			if (!annotationsCache.TryGet(parameters.GoogleDocId, out annotations))
			{
				lock (annotationsCache)
				{
					if (!annotationsCache.TryGet(parameters.GoogleDocId, out annotations))
					{
						var googleDocContent = googleDocApiClient.GetGoogleDocContentAsync(parameters.GoogleDocId).Result;
						annotations = annotationsParser.ParseAnnotations(googleDocContent);

						annotationsCache.Add(parameters.GoogleDocId, annotations);
					}
				}
			}

			var annotation = annotations.GetValueOrDefault(parameters.VideoId);
			return new AnnotationsResponse
			{
				Annotation = annotation,
			};
		}

		[HttpPost(Api.Urls.Clear)]
		public IActionResult Clear()
		{
			annotationsCache.Clear();
			return Ok(new SuccessResponseWithMessage("Cache successfully cleared"));
		}
	}
}