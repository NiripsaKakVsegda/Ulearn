using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos
{
	public interface IFavouriteReviewsRepo
	{
		Task<List<FavouriteReview>> GetFavouriteReviewsForUser(string courseId, Guid slideId, string userId);
		Task<List<FavouriteReview>> GetFavouriteReviewsForOtherUsers(string courseId, Guid slideId, string userIdToExcept, DateTime startDate);
		Task<FavouriteReviewByUser> AddFavouriteReviewByUser(string courseId, Guid slideId, string userId, string text);
		Task DeleteFavouriteReviewByUser(int favouriteReviewByUserId);
		Task DeleteFavouriteReviewByUser(FavouriteReviewByUser favouriteReviewByUser);
		Task<FavouriteReviewByUser> FindFavouriteReviewByUser(int favouriteReviewByUserId);
	}
}