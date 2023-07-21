using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;

namespace Database.Repos.Flashcards;

public interface IUserGeneratedFlashcardsRepo
{
	Task<UserGeneratedFlashcard> GetFlashcardById(Guid id);
	[ItemCanBeNull] Task<UserGeneratedFlashcard> FindFlashcardById(Guid id);
	Task<List<UserGeneratedFlashcard>> GetUnitFlashcards(string courseId, Guid unitId, [CanBeNull] FlashcardModerationStatus? status = null);
	Task<List<UserGeneratedFlashcard>> GetCourseFlashcardsRatableForUser(string courseId, string userId);
	Task<UserGeneratedFlashcard> AddFlashcard(string ownerId, string courseId, Guid unitId, string question, string answer, bool approved = false);
	Task<UserGeneratedFlashcard> EditFlashcard(Guid flashcardId, [CanBeNull] string question = null, [CanBeNull] string answer = null);
	Task DeleteFlashcard(Guid flashcardId);
	Task<UserGeneratedFlashcard> ApproveFlashcard(Guid flashcardId, string moderatorId, [CanBeNull] string question = null, [CanBeNull] string answer = null);
	Task<UserGeneratedFlashcard> DeclineFlashcard(Guid flashcardId, string moderatorId);
	Task<UserGeneratedFlashcard> RestoreFlashcard(Guid flashcardId);
}