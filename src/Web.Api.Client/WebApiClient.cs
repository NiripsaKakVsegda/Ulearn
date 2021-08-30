using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Ulearn.Common.Api;
using Vostok.Clusterclient.Core.Model;

namespace Web.Api.Client
{
	public class WebApiClient : BaseApiClient, IWebApiClient
	{
		private Uri endpointUrl;

		public WebApiClient(ApiClientSettings settings)
			: base(settings)
		{
			endpointUrl = settings.EndpointUrl;
		}

		[ItemCanBeNull]
		public async Task<Response> GetCourseStaticFile(string courseId, string filePathRelativeToCourse)
		{
			var builder = new UriBuilder(endpointUrl + $"courses/{courseId}/files/{filePathRelativeToCourse}");
			var request = Request.Get(builder.Uri);
			return (await MakeRequestAsync(request).ConfigureAwait(false)).Response;
		}

		public async Task<Response> GetStudentZipFile(string courseId, Guid slideId, string studentZipName, Header? cookieHeader)
		{
			var builder = new UriBuilder(endpointUrl + $"slides/{courseId}/{slideId}/exercise/student-zip/{studentZipName}");
			var request = Request
				.Get(builder.Uri);
			if (cookieHeader != null)
				request = request.WithHeader(cookieHeader.Value.Name, cookieHeader.Value.Value);
			return (await MakeRequestAsync(request).ConfigureAwait(false)).Response;
		}
	}
}