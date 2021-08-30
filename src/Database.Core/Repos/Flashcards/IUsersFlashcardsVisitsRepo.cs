using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos.Flashcards
{
	public interface IUsersFlashcardsVisitsRepo
	{
		Task<UserFlashcardsVisit> AddFlashcardVisitAsync(string userId, string courseId, Guid unitId, string flashcardId, Rate rate, DateTime timestamp);
		Task<List<UserFlashcardsVisit>> GetUserFlashcardsVisitsAsync(string userId, string courseId, Guid unitId);
		Task<List<UserFlashcardsVisit>> GetUserFlashcardsVisitsAsync(string courseId);
		Task<UserFlashcardsVisit> GetUserFlashcardVisitAsync(string userId, string courseId, Guid unitId, string flashcardId);
		Task<List<UserFlashcardsVisit>> GetUserFlashcardsVisitsAsync(string userId, string courseId);
	}
}