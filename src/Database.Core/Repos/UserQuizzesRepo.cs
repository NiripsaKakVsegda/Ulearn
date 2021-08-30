using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models.Quizzes;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Core.Courses.Slides.Quizzes.Blocks;

namespace Database.Repos
{
	/* TODO (andgein): BE CAREFUL. This repo is not tested absolutely, it was just copied from legacy Database/UserQuizzesRepo and refactored. */
	public class UserQuizzesRepo : IUserQuizzesRepo
	{
		private readonly UlearnDb db;

		public UserQuizzesRepo(UlearnDb db)
		{
			this.db = db;
		}

		public async Task<UserQuizSubmission> FindLastUserSubmissionAsync(string courseId, Guid slideId, string userId)
		{
			return await db.UserQuizSubmissions.Where(s => s.CourseId == courseId && s.UserId == userId && s.SlideId == slideId).OrderByDescending(s => s.Timestamp).FirstOrDefaultAsync();
		}

		public async Task<UserQuizSubmission> AddSubmissionAsync(string courseId, Guid slideId, string userId, DateTime timestamp)
		{
			var submission = new UserQuizSubmission
			{
				CourseId = courseId,
				SlideId = slideId,
				UserId = userId,
				Timestamp = timestamp,
			};
			db.UserQuizSubmissions.Add(submission);
			await db.SaveChangesAsync().ConfigureAwait(false);
			return submission;
		}

		public async Task<UserQuizAnswer> AddUserQuizAnswerAsync(int submissionId, bool isRightAnswer, string blockId, string itemId, string text, int quizBlockScore, int quizBlockMaxScore)
		{
			var answer = new UserQuizAnswer
			{
				SubmissionId = submissionId,
				IsRightAnswer = isRightAnswer,
				BlockId = blockId,
				ItemId = itemId,
				Text = text,
				QuizBlockScore = quizBlockScore,
				QuizBlockMaxScore = quizBlockMaxScore
			};
			db.UserQuizAnswers.Add(answer);
			await db.SaveChangesAsync().ConfigureAwait(false);
			return answer;
		}

		public Task<bool> IsWaitingForManualCheckAsync(string courseId, Guid slideId, string userId)
		{
			return db.ManualQuizCheckings.AnyAsync(c => c.CourseId == courseId && c.SlideId == slideId && c.UserId == userId && !c.IsChecked);
		}

		public Task<List<Guid>> GetSlideIdsWaitingForManualCheckAsync(string courseId, string userId)
		{
			return db.ManualQuizCheckings
				.Where(c => c.CourseId == courseId && c.UserId == userId && !c.IsChecked)
				.Select(c => c.SlideId).Distinct().ToListAsync();
		}
		
		public async Task<Dictionary<string, List<Guid>>> GetSlideIdsWaitingForManualCheckAsync(string courseId, IEnumerable<string> userIds)
		{
			return (await db.ManualQuizCheckings
				.Where(c => c.CourseId == courseId && userIds.Contains(c.UserId) && !c.IsChecked)
				.Select(c => new {c.UserId, c.SlideId})
				.Distinct()
				.ToListAsync())
				.GroupBy(c => c.UserId)
				.ToDictionary(g => g.Key, g => g.Select(c => c.SlideId).ToList());
		}

		public Task<int> GetUsedAttemptsCountAsync(string courseId, string userId, Guid slideId)
		{
			return db.UserQuizSubmissions
				.CountAsync(s => s.CourseId == courseId && s.UserId == userId && s.SlideId == slideId);
		}

		public async Task<Dictionary<Guid, int>> GetUsedAttemptsCountAsync(string courseId, string userId)
		{
			return await db.UserQuizSubmissions
				.Where(s => s.CourseId == courseId && s.UserId == userId)
				.Select(s => s.SlideId)
				.GroupBy(s => s)
				.Select(g => new {g.Key, Count = g.Count()})
				.ToDictionaryAsync(g => g.Key, g=> g.Count);
		}
		
		public async Task<Dictionary<string, Dictionary<Guid, int>>> GetUsedAttemptsCountAsync(string courseId, IEnumerable<string> userIds)
		{
			return (await db.UserQuizSubmissions
					.Where(s => s.CourseId == courseId && userIds.Contains(s.UserId))
					.GroupBy(s => new { s.UserId, s.SlideId })
					.Select(g => new { g.Key.UserId, g.Key.SlideId, Count = g.Count() })
					.ToListAsync())
				.GroupBy(s => s.UserId)
				.ToDictionary(s => s.Key, s => s.ToDictionary(t => t.SlideId, t => t.Count));
		}

		public async Task<HashSet<Guid>> GetPassedSlideIdsAsync(string courseId, string userId)
		{
			return new HashSet<Guid>(await db.UserQuizSubmissions.Where(x => x.CourseId == courseId && x.UserId == userId).Select(x => x.SlideId).Distinct().ToListAsync().ConfigureAwait(false));
		}

		public async Task<HashSet<Guid>> GetPassedSlideIdsWithMaximumScoreAsync(string courseId, string userId)
		{
			var passedQuizzes = await GetPassedSlideIdsAsync(courseId, userId).ConfigureAwait(false);
			var notScoredMaximumSlides = await db.UserQuizAnswers
				.Include(a => a.Submission)
				.Where(x => x.Submission.CourseId == courseId && x.Submission.UserId == userId && x.QuizBlockScore != x.QuizBlockMaxScore)
				.Select(x => x.Submission.SlideId)
				.Distinct()
				.ToListAsync()
				.ConfigureAwait(false);
			passedQuizzes.ExceptWith(notScoredMaximumSlides);
			return passedQuizzes;
		}

		public async Task<Dictionary<string, List<UserQuizAnswer>>> GetAnswersForShowingOnSlideAsync(string courseId, QuizSlide slide, string userId, UserQuizSubmission submission = null)
		{
			if (slide == null)
				return null;

			if (submission == null)
				submission = await FindLastUserSubmissionAsync(courseId, slide.Id, userId).ConfigureAwait(false);

			var answer = new Dictionary<string, List<UserQuizAnswer>>();
			if (submission == null)
			{
				foreach (var block in slide.Blocks.OfType<AbstractQuestionBlock>())
					answer[block.Id] = new List<UserQuizAnswer>();
				return answer;
			}
			
			var blocks2Answers = db.UserQuizAnswers
				.Where(q => q.SubmissionId == submission.Id)
				.OrderBy(x => x.Id)
				.AsEnumerable()
				.GroupBy(a => a.BlockId)
				.ToDictionary(g => g.Key, g => g.ToList());

			foreach (var block in slide.Blocks.OfType<AbstractQuestionBlock>())
				answer[block.Id] = blocks2Answers.ContainsKey(block.Id)
					? blocks2Answers[block.Id]
					: new List<UserQuizAnswer>();

			return answer;
		}

		public async Task<Dictionary<string, int>> GetUserScoresAsync(string courseId, Guid slideId, string userId, UserQuizSubmission submission = null)
		{
			if (submission == null)
			{
				submission = await FindLastUserSubmissionAsync(courseId, slideId, userId).ConfigureAwait(false);
				if (submission == null)
					return new Dictionary<string, int>();
			}

			var submissionAnswers = await db.UserQuizAnswers.Where(q => q.SubmissionId == submission.Id).ToListAsync().ConfigureAwait(false);
			return submissionAnswers
				.DistinctBy(q => q.BlockId)
				.ToDictionary(q => q.BlockId, q => q.QuizBlockScore);
		}

		public async Task<bool> IsQuizScoredMaximumAsync(string courseId, Guid slideId, string userId)
		{
			var submission = await FindLastUserSubmissionAsync(courseId, slideId, userId).ConfigureAwait(false);
			if (submission == null)
				return false;

			return await db.UserQuizAnswers
				.Where(q => q.SubmissionId == submission.Id)
				.AllAsync(q => q.QuizBlockScore == q.QuizBlockMaxScore)
				.ConfigureAwait(false);
		}

		public async Task SetScoreForQuizBlock(int submissionId, string blockId, int score)
		{
			var answers = await db.UserQuizAnswers
				.Where(q => q.SubmissionId == submissionId && q.BlockId == blockId)
				.ToListAsync()
				.ConfigureAwait(false);

			answers.ForEach(q => q.QuizBlockScore = score);

			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task<Dictionary<string, int>> GetAnswersFrequencyForChoiceBlock(string courseId, Guid slideId, string quizId)
		{
			var answers = db.UserQuizAnswers.Include(q => q.Submission).Where(q => q.Submission.CourseId == courseId && q.Submission.SlideId == slideId && q.BlockId == quizId);
			var totalTries = await answers.Select(q => new { q.Submission.UserId, q.Submission.Timestamp }).Distinct().CountAsync().ConfigureAwait(false);

			/* Due to performance issues don't call GroupBy().ToDictionary[Async](), call GroupBy().Select().ToDictionary() instead.
			   See http://code-ninja.org/blog/2014/07/24/entity-framework-never-call-groupby-todictionary/ for details */
			return (await answers
					.GroupBy(q => q.ItemId)
					.Select(g => new { g.Key, Count = g.Count() })
					.ToListAsync())
				.ToDictionary(p => p.Key, p => p.Count.PercentsOf(totalTries));
		}
	}
}