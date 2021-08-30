using Microsoft.AspNetCore.Mvc;

namespace Ulearn.Web.Api.Models.Parameters.Comments
{
	public class CommentParameters
	{
		[FromQuery(Name = "withReplies")]
		public bool WithReplies { get; set; } = false;
	}
}