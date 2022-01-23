using System.Collections.Generic;
using System.Runtime.Serialization;
using Database.Models;
using Ulearn.Common.Api.Models.Responses;

namespace Ulearn.Web.Api.Models.Responses
{
	[DataContract]
	public class DeadLinesResponse : SuccessResponse
	{
		[DataMember]
		public List<DeadLine> DeadLines;
	}
}