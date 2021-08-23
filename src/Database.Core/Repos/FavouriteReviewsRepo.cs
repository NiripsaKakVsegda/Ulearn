using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Database.Repos
{
	public class FavouriteReviewsRepo : IFavouriteReviewsRepo
	{
		private readonly UlearnDb db;

		public FavouriteReviewsRepo(UlearnDb db)
		{
			this.db = db;
		}

		public async Task<List<FavouriteReview>> GetFavouriteReviewsForUser(string courseId, Guid slideId, string userId)
		{
			return await db.FavouriteReviewsByUsers
				.Where(fr => fr.CourseId == courseId && fr.SlideId == slideId && fr.UserId == userId)
				.OrderBy(fr => fr.Timestamp)
				.Select(fr => fr.FavouriteReview)
				.ToListAsync();
		}

		public async Task<List<FavouriteReview>> GetFavouriteReviewsForOtherUsers(string courseId, Guid slideId, string userIdToExcept, DateTime startDate)
		{
			return await db.FavouriteReviews
				.Where(fr => fr.CourseId == courseId && fr.SlideId == slideId && !fr.FavouriteReviewsByUser.Any(fbu => fbu.UserId == userIdToExcept))
				.Select(fr => new
				{
					FavouriteReview = fr,
					Count = fr.FavouriteReviewsByUser.Count(fbu =>
						fbu.FavouriteReviewId == fr.Id && fbu.Timestamp >= startDate)
				})
				.Where(t => t.Count > 0)
				.OrderBy(t => t.Count)
				.Select(t => t.FavouriteReview)
				.Take(10)
				.ToListAsync();
		}

		public async Task<FavouriteReviewByUser> AddFavouriteReviewByUser(string courseId, Guid slideId, string userId, string text)
		{
			var favouriteReview = await FindFavouriteReviewByText(courseId, slideId, text);
			if (favouriteReview == null)
				favouriteReview = AddFavouriteReview(courseId, slideId, text);

			var favouriteReviewByUser = await FindFavouriteReviewByUser(courseId, slideId, userId, favouriteReview.Id);

			if (favouriteReviewByUser == null)
			{
				favouriteReviewByUser = new FavouriteReviewByUser
				{
					CourseId = courseId,
					SlideId = slideId,
					UserId = userId,
					Timestamp = DateTime.Now,
					FavouriteReview = favouriteReview,
				};
				db.FavouriteReviewsByUsers.Add(favouriteReviewByUser);
			}
			else
				favouriteReviewByUser.Timestamp = DateTime.Now;

			await db.SaveChangesAsync();

			return favouriteReviewByUser;
		}

		private FavouriteReview AddFavouriteReview(string courseId, Guid slideId, string text)
		{
			var favouriteReviewByUser = new FavouriteReview
			{
				CourseId = courseId,
				SlideId = slideId,
				Text = text,
			};

			db.FavouriteReviews.Add(favouriteReviewByUser);
			return favouriteReviewByUser;
		}

		public async Task DeleteFavouriteReviewByUser(FavouriteReviewByUser favouriteReviewByUser)
		{
			db.FavouriteReviewsByUsers.Remove(favouriteReviewByUser);
			await db.SaveChangesAsync();

			var hasUserWithThisFavouriteReview = await HasUserWithThisFavouriteReview(favouriteReviewByUser.FavouriteReviewId);

			if (!hasUserWithThisFavouriteReview)
			{
				var favouriteReview = await FindFavouriteReview(favouriteReviewByUser.FavouriteReviewId);
				if (favouriteReview != null)
				{
					db.FavouriteReviews.Remove(favouriteReview);
					await db.SaveChangesAsync();
				}
			}
		}

		private async Task<bool> HasUserWithThisFavouriteReview(int id)
		{
			return await db.FavouriteReviewsByUsers.AnyAsync(fbu => fbu.FavouriteReviewId == id);
		}

		private async Task<FavouriteReview> FindFavouriteReviewByText(string courseId, Guid slideId, string text)
		{
			return await db.FavouriteReviews
				.FirstOrDefaultAsync(fr =>
					fr.CourseId == courseId
					&& fr.SlideId == slideId
					&& fr.Text == text);
		}

		public async Task<FavouriteReviewByUser> FindFavouriteReviewByUser(string courseId, Guid slideId, string userId, int favouriteReviewId)
		{
			return await db.FavouriteReviewsByUsers
				.FirstOrDefaultAsync(fbu =>
					fbu.CourseId == courseId
					&& fbu.SlideId == slideId
					&& fbu.UserId == userId
					&& fbu.FavouriteReviewId == favouriteReviewId
				);
		}

		private async Task<FavouriteReview> FindFavouriteReview(int id)
		{
			return await db.FavouriteReviews.FindAsync(id);
		}
	}
}