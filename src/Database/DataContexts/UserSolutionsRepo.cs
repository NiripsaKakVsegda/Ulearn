using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using Database.Models;
using Ulearn.Common;
using Ulearn.Common.Extensions;

namespace Database.DataContexts
{
	public class UserSolutionsRepo
	{
		private readonly ULearnDb db;
		private readonly TextsRepo textsRepo;

		public UserSolutionsRepo(ULearnDb db)
		{
			this.db = db;
			textsRepo = new TextsRepo(db);
		}

		public IQueryable<UserExerciseSubmission> GetAllSubmissions(string courseId)
		{
			return db.UserExerciseSubmissions.Include(s => s.ManualChecking).Include(s => s.AutomaticChecking).Where(x => x.CourseId == courseId);
		}

		public IQueryable<UserExerciseSubmission> GetAllSubmissions(string courseId, IEnumerable<Guid> slidesIds)
		{
			return GetAllSubmissions(courseId).Where(x => slidesIds.Contains(x.SlideId));
		}

		public IQueryable<UserExerciseSubmission> GetAllSubmissions(string courseId, IEnumerable<Guid> slidesIds, DateTime periodStart, DateTime periodFinish)
		{
			return GetAllSubmissions(courseId, slidesIds)
				.Where(x =>
					periodStart <= x.Timestamp &&
					x.Timestamp <= periodFinish
				);
		}

		public IQueryable<UserExerciseSubmission> GetAllAcceptedSubmissions(string courseId, IEnumerable<Guid> slidesIds, DateTime periodStart, DateTime periodFinish)
		{
			return GetAllSubmissions(courseId, slidesIds, periodStart, periodFinish).Where(s => s.AutomaticCheckingIsRightAnswer);
		}

		public IQueryable<UserExerciseSubmission> GetAllAcceptedSubmissions(string courseId, IEnumerable<Guid> slidesIds)
		{
			return GetAllSubmissions(courseId, slidesIds).Where(s => s.AutomaticCheckingIsRightAnswer);
		}

		public IQueryable<UserExerciseSubmission> GetAllAcceptedSubmissions(string courseId)
		{
			return GetAllSubmissions(courseId).Where(s => s.AutomaticCheckingIsRightAnswer);
		}

		public IQueryable<UserExerciseSubmission> GetAllAcceptedSubmissionsByUser(string courseId, IEnumerable<Guid> slideIds, string userId)
		{
			return GetAllAcceptedSubmissions(courseId, slideIds).Where(s => s.UserId == userId);
		}

		public IQueryable<UserExerciseSubmission> GetAllAcceptedSubmissionsByUser(string courseId, string userId)
		{
			return GetAllAcceptedSubmissions(courseId).Where(s => s.UserId == userId);
		}

		public IQueryable<UserExerciseSubmission> GetAllAcceptedSubmissionsByUser(string courseId, Guid slideId, string userId)
		{
			return GetAllAcceptedSubmissionsByUser(courseId, new List<Guid> { slideId }, userId);
		}

		public IQueryable<UserExerciseSubmission> GetAllSubmissionsByUser(string courseId, Guid slideId, string userId)
		{
			return GetAllSubmissions(courseId, new List<Guid> { slideId }).Where(s => s.UserId == userId);
		}

		public IQueryable<UserExerciseSubmission> GetAllSubmissionsByUsers(SubmissionsFilterOptions filterOptions)
		{
			var submissions = GetAllSubmissions(filterOptions.CourseId, filterOptions.SlidesIds);
			if (filterOptions.IsUserIdsSupplement)
				submissions = submissions.Where(s => !filterOptions.UserIds.Contains(s.UserId));
			else
				submissions = submissions.Where(s => filterOptions.UserIds.Contains(s.UserId));
			return submissions;
		}

		public IQueryable<AutomaticExerciseChecking> GetAutomaticExerciseCheckingsByUsers(string courseId, Guid slideId, List<string> userIds)
		{
			var query = db.AutomaticExerciseCheckings.Where(c => c.CourseId == courseId && c.SlideId == slideId);
			if (userIds != null)
				query = query.Where(v => userIds.Contains(v.UserId));
			return query;
		}

	public int GetAcceptedSolutionsCount(string courseId, Guid slideId)
		{
			return GetAllAcceptedSubmissions(courseId, new List<Guid> { slideId }).Select(x => x.UserId).Distinct().Count();
		}

		public bool IsCheckingSubmissionByUser(string courseId, Guid slideId, string userId, DateTime periodStart, DateTime periodFinish)
		{
			var automaticCheckingsIds = GetAllSubmissions(courseId, new List<Guid> { slideId }, periodStart, periodFinish)
				.Where(s => s.UserId == userId)
				.Select(s => s.AutomaticCheckingId)
				.ToList();
			return db.AutomaticExerciseCheckings.Any(c => automaticCheckingsIds.Contains(c.Id) && c.Status != AutomaticExerciseCheckingStatus.Done);
		}

		public HashSet<Guid> GetIdOfPassedSlides(string courseId, string userId)
		{
			return new HashSet<Guid>(db.AutomaticExerciseCheckings
				.Where(x => x.IsRightAnswer && x.CourseId == courseId && x.UserId == userId)
				.Select(x => x.SlideId)
				.Distinct());
		}

		public bool IsSlidePassed(string courseId, string userId, Guid slideId)
		{
			return db.AutomaticExerciseCheckings.Any(x => x.IsRightAnswer && x.CourseId == courseId && x.UserId == userId && x.SlideId == slideId);
		}

		public IQueryable<UserExerciseSubmission> GetAllSubmissions(int max, int skip)
		{
			return db.UserExerciseSubmissions
				.OrderByDescending(x => x.Timestamp)
				.Skip(skip)
				.Take(max);
		}

		public UserExerciseSubmission FindNoTrackingSubmission(int id)
		{
			return FuncUtils.TrySeveralTimes(() => TryFindNoTrackingSubmission(id), 3, () => Thread.Sleep(TimeSpan.FromMilliseconds(200)));
		}

		private UserExerciseSubmission TryFindNoTrackingSubmission(int id)
		{
			var submission = db.UserExerciseSubmissions.AsNoTracking().SingleOrDefault(x => x.Id == id);
			if (submission == null)
				return null;
			submission.SolutionCode = textsRepo.GetText(submission.SolutionCodeHash);

			if (submission.AutomaticChecking != null)
			{
				submission.AutomaticChecking.Output = textsRepo.GetText(submission.AutomaticChecking.OutputHash);
				submission.AutomaticChecking.CompilationError = textsRepo.GetText(submission.AutomaticChecking.CompilationErrorHash);
			}

			return submission;
		}

		public UserExerciseSubmission FindSubmissionById(int id)
		{
			return db.UserExerciseSubmissions.Find(id);
		}

		private UserExerciseSubmission FindSubmissionById(string idString)
		{
			return int.TryParse(idString, out var id) ? FindSubmissionById(id) : null;
		}

		public List<UserExerciseSubmission> FindSubmissionsByIds(IEnumerable<int> checkingsIds)
		{
			return db.UserExerciseSubmissions.Where(c => checkingsIds.Contains(c.Id)).ToList();
		}

		private void UpdateIsRightAnswerForSubmission(AutomaticExerciseChecking checking)
		{
			db.UserExerciseSubmissions
				.Where(s => s.AutomaticCheckingId == checking.Id)
				.ForEach(s => s.AutomaticCheckingIsRightAnswer = checking.IsRightAnswer);
		}

		public Dictionary<int, string> GetSolutionsForSubmissions(IEnumerable<int> submissionsIds)
		{
			var solutionsHashes = db.UserExerciseSubmissions
				.Where(s => submissionsIds.Contains(s.Id))
				.Select(s => new { Hash = s.SolutionCodeHash, SubmissionId = s.Id }).ToList();
			var textsByHash = textsRepo.GetTextsByHashes(solutionsHashes.Select(s => s.Hash));
			return solutionsHashes.ToDictSafe(s => s.SubmissionId, s => textsByHash.GetOrDefault(s.Hash, ""));
		}
	}
}