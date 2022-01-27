using System.Collections.Generic;
using System.Runtime.Serialization;
using Ulearn.Web.Api.Controllers;

namespace Ulearn.Web.Api.Models.Responses.AdditionalContent
{

	[DataContract]
	public class AdditionalContentPublicationsResponse
	{
		[DataMember]
		public List<AdditionalContentPublicationResponse> Publications;
	}
}