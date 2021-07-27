using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Database.Models;
using Ulearn.Common.Extensions;

namespace Ulearn.Web.Api.Models.Responses.Review
{
	[DataContract]
	public class FavouriteReviewsResponse
	{
		[DataMember]
		public List<FavouriteReviewResponse> FavouriteReviews { get; set; }

		[DataMember]
		public List<FavouriteReviewResponse> UserFavouriteReviews { get; set; }

		public static FavouriteReviewsResponse Build(List<FavouriteReview> favouriteReviews, List<FavouriteReview> userFavouriteReviews)
		{
			return new()
			{
				FavouriteReviews = favouriteReviews.Select(FavouriteReviewResponse.Build).ToList(),
				UserFavouriteReviews = userFavouriteReviews.Select(FavouriteReviewResponse.Build).ToList(),
			};
		}
	}

	[DataContract]
	public class FavouriteReviewResponse
	{
		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public string Text { get; set; }

		[DataMember]
		public string RenderedText { get; set; }

		public static FavouriteReviewResponse Build(FavouriteReview fr)
		{
			return new() { Id = fr.Id, Text = fr.Text, RenderedText = fr.Text.RenderSimpleMarkdown() };
		}

		public static FavouriteReviewResponse Build(FavouriteReviewByUser fr)
		{
			return new() { Id = fr.Id, Text = fr.FavouriteReview.Text, RenderedText = fr.FavouriteReview.Text.RenderSimpleMarkdown() };
		}
	}
}