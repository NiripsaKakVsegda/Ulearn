using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models.Quizzes;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Core.Courses.Slides.Quizzes.Blocks;

namespace Database.Repos
{
	public interface IUserQuizzesRepo
	{
		Task<UserQuizSubmission> FindLastUserSubmissionAsync(string courseId, Guid slideId, string userId);
		Task<UserQuizSubmission> AddSubmissionAsync(string courseId, Guid slideId, string userId, DateTime timestamp);
		Task<UserQuizAnswer> AddUserQuizAnswerAsync(int submissionId, bool isRightAnswer, string blockId, string itemId, string text, int quizBlockScore, int quizBlockMaxScore);
		Task<bool> IsWaitingForManualCheckAsync(string courseId, Guid slideId, string userId);
		Task<List<Guid>> GetSlideIdsWaitingForManualCheckAsync(string courseId, string userId);
		Task<Dictionary<string, List<Guid>>> GetSlideIdsWaitingForManualCheckAsync(string courseId, IEnumerable<string> userIds);
		Task<int> GetUsedAttemptsCountAsync(string courseId, string userId, Guid slideId);
		Task<Dictionary<string, Dictionary<Guid, int>>> GetUsedAttemptsCountAsync(string courseId, IEnumerable<string> userIds);
		Task<Dictionary<Guid, int>> GetUsedAttemptsCountAsync(string courseId, string userId);
		Task<HashSet<Guid>> GetPassedSlideIdsAsync(string courseId, string userId);
		Task<HashSet<Guid>> GetPassedSlideIdsWithMaximumScoreAsync(string courseId, string userId);
		Task<Dictionary<string, List<UserQuizAnswer>>> GetAnswersForShowingOnSlideAsync(string courseId, QuizSlide slide, string userId, UserQuizSubmission submission = null);
		Task<Dictionary<string, int>> GetUserScoresAsync(string courseId, Guid slideId, string userId, UserQuizSubmission submission = null);
		Task<bool> IsQuizScoredMaximumAsync(string courseId, Guid slideId, string userId);
		Task SetScoreForQuizBlock(int submissionId, string blockId, int score);
		Task<Dictionary<string, int>> GetAnswersFrequencyForChoiceBlock(string courseId, Guid slideId, string quizId);
		Task<Dictionary<string, Dictionary<string, int>>> GetAnswersFrequencyForChoiceBlocks(string courseId, Guid slideId, List<ChoiceBlock> choiceBlock);
		int GetUsedAttemptsCountForQuizWithAutomaticChecking(string courseId, string userId, Guid slideId);
	}
}