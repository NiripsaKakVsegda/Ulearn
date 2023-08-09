using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Database.Repos.Flashcards
{
	public class UsersFlashcardsVisitsRepo : IUsersFlashcardsVisitsRepo
	{
		private readonly UlearnDb db;

		public UsersFlashcardsVisitsRepo(UlearnDb db)
		{
			this.db = db;
		}

		public async Task<UserFlashcardsVisit> AddFlashcardVisitAsync(string userId, string courseId, Guid unitId, string flashcardId, Rate rate)
		{
			courseId = courseId.ToLower();
			var visit = new UserFlashcardsVisit
			{
				UserId = userId,
				CourseId = courseId,
				UnitId = unitId,
				FlashcardId = flashcardId,
				Rate = rate,
				Timestamp = DateTime.Now
			};
			db.UserFlashcardsVisits.Add(visit);

			await db.SaveChangesAsync();

			return visit;
		}

		public async Task<List<UserFlashcardsVisit>> GetAllFlashcardsVisitsByCourseAsync(string courseId)
		{
			courseId = courseId.ToLower();
			return await db.UserFlashcardsVisits.Where(c => c.CourseId == courseId).ToListAsync();
		}

		public Task<List<UserFlashcardsVisit>> GetLastUserFlashcardsVisitsAsync(string userId, string courseId, Guid? unitId)
		{
			courseId = courseId.ToLower();
			var query = db.UserFlashcardsVisits
				.Where(v => v.UserId == userId && v.CourseId == courseId);

			if (unitId is { } unitIdValue)
				query = query
					.Where(v => v.UnitId == unitIdValue);
			return query
				.GroupBy(v => new { v.UserId, v.FlashcardId })
				.Select(g => g.OrderByDescending(v => v.Timestamp).First())
				.ToListAsync();
		}

		public async Task<UserFlashcardsVisit> FindLastUserFlashcardVisitAsync(string userId, string courseId, string flashcardId)
		{
			courseId = courseId.ToLower();
			return await db.UserFlashcardsVisits
				.Where(v => v.CourseId == courseId && v.UserId == userId && v.FlashcardId == flashcardId)
				.OrderByDescending(v => v.Timestamp)
				.FirstOrDefaultAsync();
		}
	}
}