using System.Runtime.Serialization;
using Ulearn.Common.Api.Models.Responses;

namespace AntiPlagiarism.Api.Models.Results
{
	public class GetProcessingStatusResponse : SuccessResponse
	{
		
		[DataMember(Name = "submissionId")]
		public int SubmissionId { get; set; }
		
		[DataMember(Name = "inQueue")]
		public bool InQueue { get; set; }
	}
}