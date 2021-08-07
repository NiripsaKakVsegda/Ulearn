using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Database.Models;
using Ulearn.Common.Api.Models.Responses;

namespace Ulearn.Web.Api.Models.Responses.Review
{
	[DataContract]
	public class ReviewQueueResponse : SuccessResponse
	{
		[DataMember]
		public List<ReviewQueueItem> Checkings { get; set; }

		public static ReviewQueueResponse Build(IEnumerable<AbstractManualSlideChecking> checkings)
		{
			return new ReviewQueueResponse
			{
				Checkings = checkings
					.Select(c => new ReviewQueueItem
					{
						SubmissionId = c.Id, 
						UserId = c.UserId,
						IsLocked = c.IsLocked,
					})
					.ToList(),
			};
		}
	}

	[DataContract]
	public class ReviewQueueItem
	{
		[DataMember]
		public int SubmissionId { get; set; }
		
		[DataMember]
		public string UserId { get; set; }
		
		[DataMember]
		public bool IsLocked { get; set; }
	}
}