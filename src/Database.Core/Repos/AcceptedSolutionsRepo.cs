using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos.Users;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common;
using Ulearn.Core.Extensions;
using Z.EntityFramework.Plus;

namespace Database.Repos
{
	public interface IAcceptedSolutionsRepo
	{
		Task<List<(int SubmissionId, string Code, Language Language, int LikesCount)>> GetNewestSubmissions(string courseId, Guid slideId, int count);
		Task<List<(int SubmissionId, string Code, Language Language, int LikesCount, ApplicationUser UserWhoPromote)>> GetPromotedSubmissions(string courseId, Guid slideId);
		Task<(int SubmissionId, string Code, Language Language, int LikesCount)?> GetRandomLikedSubmission(string courseId, Guid slideId);
		Task<List<(int SubmissionId, string Code, Language Language, int LikesCount)>> GetLikedAcceptedSolutions(string courseId, Guid slideId, int offset, int count);
		Task<HashSet<int>> GetSubmissionsLikedByMe(string courseId, Guid slideId, string userId);
		Task<bool> DidUserLikeSubmission(int submissionId, string userId);
		Task TryLikeSubmission(string courseId, Guid slideId, int submissionId, string userId);
		Task TryUnlikeSubmission(int submissionId, string userId);
		Task<bool> HasSubmissionBeenPromoted(int submissionId);
		Task TryPromoteSubmission(string courseId, Guid slideId, int submissionId, string userId);
		Task TryUnpromoteSubmission(int submissionId, string userId);
	}

	public class AcceptedSolutionsRepo : IAcceptedSolutionsRepo
	{
		private readonly UlearnDb db;
		private readonly IUserSolutionsRepo userSolutionsRepo;

		public AcceptedSolutionsRepo(UlearnDb db, IUserSolutionsRepo userSolutionsRepo)
		{
			this.db = db;
			this.userSolutionsRepo = userSolutionsRepo;
		}

		public async Task<List<(int SubmissionId, string Code, Language Language, int LikesCount)>> GetNewestSubmissions(string courseId, Guid slideId, int count)
		{
			var ulearnBotId = db.Users.FirstOrDefault(u => u.UserName == UsersRepo.UlearnBotUsername).Id;
			var allAcceptedSubmissions = userSolutionsRepo.GetAllAcceptedSubmissions(courseId, slideId);
			var submissionsFromDb = await allAcceptedSubmissions
				.Where(x => x.Reviews.All(r => r.AuthorId != ulearnBotId))
				.OrderByDescending(s => s.Timestamp)
				.Take(count * 10)
				.Select(s => new { SubmissionId = s.Id, Code = s.SolutionCode.Text, s.Language, s.Timestamp, Likes = s.Likes.Count, s.UserId })
				.ToListAsync();
			return submissionsFromDb
				.GroupBy(s => s.UserId)
				.Select(g => g.MaxBy(m => m.Timestamp))
				.GroupBy(s => s.Code)
				.Select(g => g.MaxBy(m => m.Timestamp))
				.Select(t => (t.SubmissionId, t.Code, t.Language, t.Likes))
				.ToList();
		}

		public async Task<List<(int SubmissionId, string Code, Language Language, int LikesCount, ApplicationUser UserWhoPromote)>> GetPromotedSubmissions(string courseId, Guid slideId)
		{
			return (await db.AcceptedSolutionsPromotes.Where(p => p.Submission.CourseId == courseId && p.Submission.SlideId == slideId)
					.Select(s => new { s.SubmissionId, Code = s.Submission.SolutionCode.Text, s.Submission.Language, Likes = s.Submission.Likes.Count, UserWhoPromote = s.User })
					.ToListAsync())
				.Select(s => (s.SubmissionId, s.Code, s.Language, LikesCount: s.Likes, s.UserWhoPromote))
				.ToList();
		}

		public async Task<(int SubmissionId, string Code, Language Language, int LikesCount)?> GetRandomLikedSubmission(string courseId, Guid slideId)
		{
			var submissionIds = (await db.SolutionLikes
					.Where(l => l.CourseId == courseId && l.SlideId == slideId)
					.OrderByDescending(s => s.Timestamp)
					.Select(l => l.SubmissionId)
					.Take(100)
					.ToListAsync())
				.Distinct()
				.ToList();
			if (submissionIds.Count == 0)
				return null;
			var rnd = new Random();
			var selectedId = submissionIds[rnd.Next(0, submissionIds.Count)];
			var result = await db.UserExerciseSubmissions
				.Where(s => s.Id == selectedId)
				.Select(s => new { SubmissionId = s.Id, Code = s.SolutionCode.Text, s.Language, Likes = s.Likes.Count })
				.FirstOrDefaultAsync();
			return (result.SubmissionId, result.Code, result.Language, result.Likes);
		}

		public async Task<List<(int SubmissionId, string Code, Language Language, int LikesCount)>> GetLikedAcceptedSolutions(string courseId, Guid slideId, int offset, int count)
		{
			var lastLikedSubmissions = await db.SolutionLikes
				.Where(l => l.CourseId == courseId && l.SlideId == slideId)
				.GroupBy(l => l.SubmissionId)
				.Select(g => new { SubmissionId = g.Key, Timestamp = g.Max(e => e.Timestamp) })
				.OrderByDescending(e => e.Timestamp)
				.Select(e => e.SubmissionId)
				.Skip(offset)
				.Take(count)
				.ToListAsync();
			var allAcceptedSubmissions = userSolutionsRepo.GetAllAcceptedSubmissions(courseId, slideId);
			return (await allAcceptedSubmissions.Where(s => lastLikedSubmissions.Contains(s.Id))
					.OrderByDescending(s => s.Timestamp)
					.Select(s => new { SubmissionId = s.Id, Code = s.SolutionCode.Text, s.Language, Likes = s.Likes.Count })
					.ToListAsync())
				.Select(s => (s.SubmissionId, s.Code, s.Language, s.Likes))
				.ToList();
		}

		public async Task<HashSet<int>> GetSubmissionsLikedByMe(string courseId, Guid slideId, string userId)
		{
			return (await db.SolutionLikes.Where(s => s.Submission.CourseId == courseId && s.Submission.SlideId == slideId && s.UserId == userId)
					.Select(l => l.SubmissionId)
					.ToListAsync())
				.ToHashSet();
		}

		public async Task<bool> DidUserLikeSubmission(int submissionId, string userId)
		{
			return await db.SolutionLikes.Where(like => like.UserId == userId && like.SubmissionId == submissionId).AnyAsync();
		}

		public async Task TryLikeSubmission(string courseId, Guid slideId, int submissionId, string userId)
		{
			await db.SolutionLikes.Where(like => like.UserId == userId && like.SubmissionId == submissionId).DeleteAsync();

			db.SolutionLikes.Add(new Like
			{
				UserId = userId,
				SubmissionId = submissionId,
				CourseId = courseId,
				SlideId = slideId,
				Timestamp = DateTime.Now,
			});

			try
			{
				await db.SaveChangesAsync();
			}
			catch (Exception)
			{
				/* Somebody other have added like already. Ok, it's not a problem */
			}
		}

		public async Task TryUnlikeSubmission(int submissionId, string userId)
		{
			await db.SolutionLikes.Where(like => like.UserId == userId && like.SubmissionId == submissionId).DeleteAsync();
		}

		public async Task<bool> HasSubmissionBeenPromoted(int submissionId)
		{
			return await db.AcceptedSolutionsPromotes.Where(like => like.SubmissionId == submissionId).AnyAsync();
		}

		public async Task TryPromoteSubmission(string courseId, Guid slideId, int submissionId, string userId)
		{
			await db.AcceptedSolutionsPromotes.Where(like => like.SubmissionId == submissionId).DeleteAsync();

			db.AcceptedSolutionsPromotes.Add(new AcceptedSolutionsPromote
			{
				UserId = userId,
				SubmissionId = submissionId,
				CourseId = courseId,
				SlideId = slideId,
				Timestamp = DateTime.Now
			});

			try
			{
				await db.SaveChangesAsync();
			}
			catch (Exception)
			{
				/* Somebody other have added like already. Ok, it's not a problem */
			}
		}

		public async Task TryUnpromoteSubmission(int submissionId, string userId)
		{
			await db.AcceptedSolutionsPromotes.Where(like => like.SubmissionId == submissionId).DeleteAsync();
		}
	}
}