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

		public async Task<UserFlashcardsVisit> AddFlashcardVisitAsync(string userId, string courseId, Guid unitId, string flashcardId, Rate rate, DateTime timestamp)
		{
			courseId = courseId.ToLower();
			var record = new UserFlashcardsVisit
				{ UserId = userId, CourseId = courseId, UnitId = unitId, FlashcardId = flashcardId, Rate = rate, Timestamp = timestamp };
			db.UserFlashcardsVisits.Add(record);

			await db.SaveChangesAsync();

			return await GetUserFlashcardVisitAsync(userId, courseId, unitId, flashcardId);
		}

		public async Task<List<UserFlashcardsVisit>> GetUserFlashcardsVisitsAsync(string userId, string courseId, Guid unitId)
		{
			courseId = courseId.ToLower();
			return await db.UserFlashcardsVisits.Where(c => c.UserId == userId && c.CourseId == courseId && c.UnitId == unitId).ToListAsync();
		}

		public async Task<List<UserFlashcardsVisit>> GetUserFlashcardsVisitsAsync(string courseId)
		{
			courseId = courseId.ToLower();
			return await db.UserFlashcardsVisits.Where(c => c.CourseId == courseId).ToListAsync();
		}

		public async Task<UserFlashcardsVisit> GetUserFlashcardVisitAsync(string userId, string courseId, Guid unitId, string flashcardId)
		{
			courseId = courseId.ToLower();
			return await db.UserFlashcardsVisits.FirstOrDefaultAsync(c => c.UserId == userId && c.CourseId == courseId && c.UnitId == unitId && c.FlashcardId == flashcardId);
		}

		public async Task<List<UserFlashcardsVisit>> GetUserFlashcardsVisitsAsync(string userId, string courseId)
		{
			courseId = courseId.ToLower();
			return await db.UserFlashcardsVisits.Where(c => c.UserId == userId && c.CourseId == courseId).ToListAsync();
		}
	}
}