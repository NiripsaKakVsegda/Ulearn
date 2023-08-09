using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Database.Repos.Flashcards;

public class UserGeneratedFlashcardsRepo : IUserGeneratedFlashcardsRepo
{
	private readonly UlearnDb db;

	public UserGeneratedFlashcardsRepo(UlearnDb db)
	{
		this.db = db;
	}

	public async Task<UserGeneratedFlashcard> GetFlashcardById(Guid id)
	{
		var flashcard = await db.UserGeneratedFlashcards.FindAsync(id);
		return flashcard ?? throw new ArgumentException($@"Flashcard with id {id} not found", nameof(id));
	}

	public Task<UserGeneratedFlashcard> FindFlashcardById(Guid id)
	{
		return db.UserGeneratedFlashcards.FindAsync(id).AsTask();
	}

	public Task<List<UserGeneratedFlashcard>> GetUnitFlashcards(string courseId, Guid unitId, FlashcardModerationStatus? status = null)
	{
		var query = db.UserGeneratedFlashcards
			.Where(f => f.CourseId == courseId && f.UnitId == unitId);

		if (status is not null)
			query = query
				.Where(f => f.ModerationStatus == status);

		return query.ToListAsync();
	}

	public Task<List<UserGeneratedFlashcard>> GetCourseFlashcardsRatableForUser(string courseId, string userId)
	{
		return db.UserGeneratedFlashcards
			.Where(f => f.CourseId == courseId)
			.Where(f => f.OwnerId == userId || f.ModerationStatus == FlashcardModerationStatus.Approved)
			.ToListAsync();
	}

	public async Task<UserGeneratedFlashcard> AddFlashcard(
		string ownerId,
		string courseId,
		Guid unitId,
		string question,
		string answer,
		bool approved = false
	)
	{
		var flashcard = new UserGeneratedFlashcard
		{
			OwnerId = ownerId,
			CourseId = courseId,
			UnitId = unitId,
			Question = question,
			Answer = answer,
			LastUpdateTimestamp = DateTime.Now,
			ModerationStatus = FlashcardModerationStatus.New
		};

		if (approved)
		{
			flashcard.ModerationStatus = FlashcardModerationStatus.Approved;
			flashcard.ModeratorId = ownerId;
			flashcard.ModerationTimestamp = DateTime.Now;
		}

		db.UserGeneratedFlashcards.Add(flashcard);
		await db.SaveChangesAsync().ConfigureAwait(false);

		return flashcard;
	}

	public async Task<UserGeneratedFlashcard> EditFlashcard(Guid flashcardId, string question = null, string answer = null)
	{
		var flashcard = await GetFlashcardById(flashcardId);

		if (question is not null)
			flashcard.Question = question;
		if (answer is not null)
			flashcard.Answer = answer;

		flashcard.LastUpdateTimestamp = DateTime.Now;
		flashcard.ModerationStatus = FlashcardModerationStatus.New;
		flashcard.ModeratorId = null;
		flashcard.ModerationTimestamp = null;
		await db.SaveChangesAsync().ConfigureAwait(false);

		return flashcard;
	}

	public async Task DeleteFlashcard(Guid flashcardId)
	{
		var flashcard = await GetFlashcardById(flashcardId);
		db.UserGeneratedFlashcards.Remove(flashcard);
		await db.SaveChangesAsync().ConfigureAwait(false);
	}

	public async Task<UserGeneratedFlashcard> ApproveFlashcard(Guid flashcardId, string moderatorId, string question = null, string answer = null)
	{
		var flashcard = await GetFlashcardById(flashcardId);

		flashcard.ModerationStatus = FlashcardModerationStatus.Approved;
		flashcard.ModeratorId = moderatorId;
		flashcard.ModerationTimestamp = DateTime.Now;

		if (question is not null)
			flashcard.Question = question;
		if (answer is not null)
			flashcard.Answer = answer;

		await db.SaveChangesAsync().ConfigureAwait(false);
		return flashcard;
	}

	public async Task<UserGeneratedFlashcard> DeclineFlashcard(Guid flashcardId, string moderatorId)
	{
		var flashcard = await GetFlashcardById(flashcardId);

		flashcard.ModerationStatus = FlashcardModerationStatus.Declined;
		flashcard.ModeratorId = moderatorId;
		flashcard.ModerationTimestamp = DateTime.Now;

		await db.SaveChangesAsync().ConfigureAwait(false);
		return flashcard;
	}

	public async Task<UserGeneratedFlashcard> RestoreFlashcard(Guid flashcardId)
	{
		var flashcard = await GetFlashcardById(flashcardId);

		flashcard.ModerationStatus = FlashcardModerationStatus.New;
		flashcard.ModeratorId = null;
		flashcard.ModerationTimestamp = null;

		await db.SaveChangesAsync().ConfigureAwait(false);
		return flashcard;
	}
}