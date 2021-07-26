using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ulearn.Web.Api.Models.Responses.Review
{
	[DataContract]
	public class FavouriteReviewsResponse
	{
		public List<FavouriteReview> FavouriteReviews;
		public List<FavouriteReview> UserFavouriteReviews;
	}

	[DataContract]
	public class FavouriteReview
	{
		[DataMember]
		public int Id { get; set; }
		
		[DataMember]
		public string Text { get; set; }

		[DataMember]
		public string RenderedText { get; set; }

		[DataMember]
		public int AddedToFavouriteCount { get; set; }
	}
}