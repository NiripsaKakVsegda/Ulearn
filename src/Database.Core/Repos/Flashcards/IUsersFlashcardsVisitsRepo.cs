using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;

namespace Database.Repos.Flashcards
{
	public interface IUsersFlashcardsVisitsRepo
	{
		Task<UserFlashcardsVisit> AddFlashcardVisitAsync(string userId, string courseId, Guid unitId, string flashcardId, Rate rate);
		Task<List<UserFlashcardsVisit>> GetLastUserFlashcardsVisitsAsync(string userId, string courseId, Guid unitId);
		Task<List<UserFlashcardsVisit>> GetAllFlashcardsVisitsByCourseAsync(string courseId);
		[ItemCanBeNull] Task<UserFlashcardsVisit> FindLastUserFlashcardVisitAsync(string userId, string courseId, string flashcardId);
		Task<List<UserFlashcardsVisit>> GetLastUserFlashcardsVisitsAsync(string userId, string courseId);
	}
}