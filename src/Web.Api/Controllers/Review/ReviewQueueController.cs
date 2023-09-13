using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Groups;
using Database.Repos.Users;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Core.Extensions;
using Ulearn.Web.Api.Models.Parameters.Review;
using Ulearn.Web.Api.Models.Responses.Review;
using Ulearn.Web.Api.Utils;

namespace Ulearn.Web.Api.Controllers.Review
{
	[Route("review-queue")]
	public class ReviewQueueController : BaseController
	{
		private readonly ISlideCheckingsRepo slideCheckingsRepo;
		private readonly IGroupsRepo groupsRepo;
		private readonly ICourseRolesRepo courseRolesRepo;
		private readonly IUserSolutionsRepo solutionsRepo;

		public ReviewQueueController(
			ICourseStorage courseStorage,
			UlearnDb db,
			IUsersRepo usersRepo,
			ISlideCheckingsRepo slideCheckingsRepo,
			IGroupsRepo groupsRepo,
			ICourseRolesRepo courseRolesRepo,
			IUserSolutionsRepo solutionsRepo
		)
			: base(courseStorage, db, usersRepo)
		{
			this.slideCheckingsRepo = slideCheckingsRepo;
			this.groupsRepo = groupsRepo;
			this.courseRolesRepo = courseRolesRepo;
			this.solutionsRepo = solutionsRepo;
		}

		[HttpGet]
		[Authorize]
		public async Task<ActionResult<ReviewQueueResponse>> GetReviewQueue(
			[FromQuery] ReviewQueueFilterParameters parameters
		)
		{
			var course = courseStorage.FindCourse(parameters.CourseId);
			if (course is null)
				return NotFound(new ErrorResponse($"Course with id {parameters.CourseId} not found"));

			var isCourseAdmin = await courseRolesRepo.HasUserAccessToCourse(UserId, parameters.CourseId, CourseRoleType.CourseAdmin);
			var hasAccess = isCourseAdmin || await courseRolesRepo.HasUserAccessToCourse(UserId, parameters.CourseId, CourseRoleType.Instructor);
			if (!hasAccess)
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You should be at least course instructor"));

			await ValidateFilter(parameters, isCourseAdmin);

			var filterOptions = await BuildQueryFilterOptions(parameters, false);
			var checkings = await GetMergedCheckingQueue(filterOptions, course, includeFields: true);

			return new ReviewQueueResponse
			{
				Checkings = checkings
					.Select(checking => BuildReviewQueueItem(checking, course))
					.ToList()
			};
		}

		[HttpGet("history")]
		public async Task<ActionResult<ReviewQueueResponse>> GetReviewQueueHistory(
			[FromQuery] ReviewQueueHistoryFilterParameters parameters
		)
		{
			var course = courseStorage.FindCourse(parameters.CourseId);
			if (course is null)
				return NotFound(new ErrorResponse($"Course with id {parameters.CourseId} not found"));

			var isCourseAdmin = await courseRolesRepo.HasUserAccessToCourse(UserId, parameters.CourseId, CourseRoleType.CourseAdmin);
			var hasAccess = isCourseAdmin || await courseRolesRepo.HasUserAccessToCourse(UserId, parameters.CourseId, CourseRoleType.Instructor);
			if (!hasAccess)
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You should be at least course instructor"));

			await ValidateFilter(parameters, isCourseAdmin);

			var filterOptions = await BuildQueryFilterOptions(parameters, true);
			var checkings = await GetMergedCheckingQueue(
				filterOptions,
				course,
				true,
				parameters.MinCheckedTimestamp,
				true
			);

			var exerciseIds = checkings
				.OfType<ManualExerciseChecking>()
				.Select(c => c.Id)
				.ToList();
			var reviews = exerciseIds.Count > 0
				? await BuildReviewsInfo(exerciseIds)
				: null;

			return new ReviewQueueResponse
			{
				Checkings = checkings
					.Select(checking => BuildReviewQueueItem(
						checking,
						course,
						reviews?.GetOrDefault(checking.Id, null)
					))
					.ToList()
			};
		}

		[HttpGet("meta")]
		public async Task<ActionResult<ReviewQueueMetaResponse>> GetReviewQueueMeta(
			[FromQuery] ReviewQueueMetaFilterParameters parameters
		)
		{
			var course = courseStorage.FindCourse(parameters.CourseId);
			if (course is null)
				return NotFound(new ErrorResponse($"Course with id {parameters.CourseId} not found"));

			var isCourseAdmin = await courseRolesRepo.HasUserAccessToCourse(UserId, parameters.CourseId, CourseRoleType.CourseAdmin);
			var hasAccess = isCourseAdmin || await courseRolesRepo.HasUserAccessToCourse(UserId, parameters.CourseId, CourseRoleType.Instructor);
			if (!hasAccess)
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You should be at least course instructor"));

			await ValidateFilter(parameters, isCourseAdmin);

			var filterOptions = await BuildQueryFilterOptions(parameters, parameters.IsHistory);
			var checkings = await GetMergedCheckingQueue(
				filterOptions,
				course,
				parameters.IsHistory,
				parameters.MinCheckedTimestamp
			);

			return new ReviewQueueMetaResponse
			{
				Checkings = checkings
					.Select(checking => new ShortReviewQueueItem
						{
							SubmissionId = checking.Id,
							SlideId = checking.SlideId,
							UserId = checking.UserId,
							LockedById = checking.LockedById,
							LockedUntil = checking.LockedUntil
						}
					)
					.ToList()
			};
		}

		[HttpPut("{submissionId:int}/lock")]
		public async Task<ActionResult> LockSubmission([FromRoute] int submissionId)
		{
			var checking = await slideCheckingsRepo.FindManualCheckingById(submissionId);

			if (checking == null)
				return NotFound(new ErrorResponse($"Submission {submissionId} not found"));

			var hasAccess = await courseRolesRepo.HasUserAccessToCourse(UserId, checking.CourseId, CourseRoleType.Instructor);
			if (!hasAccess)
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You should be at least course instructor"));

			await slideCheckingsRepo.LockManualChecking(checking, UserId);
			return Ok(new SuccessResponseWithMessage($"Submission with id {submissionId} locked by you for 30 minutes"));
		}

		private async Task ValidateFilter(ReviewQueueFilterParameters parameters, bool isCourseAdmin)
		{
			if (parameters.StudentsFilter is StudentsFilter.All && !isCourseAdmin)
				parameters.StudentsFilter = StudentsFilter.MyGroups;

			if (parameters.StudentsFilter is StudentsFilter.GroupIds)
			{
				parameters.GroupIds = parameters.GroupIds is { Count: > 0 }
					? await groupsRepo.FilterGroupIdsAvailableForUser<SingleGroup>(parameters.GroupIds, UserId, parameters.CourseId)
					: new List<int>();
			}

			if (parameters.StudentsFilter is StudentsFilter.StudentIds)
			{
				parameters.StudentIds = parameters.StudentIds is { Count: > 0 }
					? await usersRepo.FilterUserIdsAvailableForUser(parameters.StudentIds, UserId, parameters.CourseId)
					: new List<string>();
			}
		}

		private async Task<ManualCheckingQueueFilterOptions> BuildQueryFilterOptions(ReviewQueueFilterParameters parameters, bool reviewed)
		{
			var studentIds = parameters.StudentsFilter switch
			{
				StudentsFilter.All => null,
				StudentsFilter.StudentIds => parameters.StudentIds,
				StudentsFilter.GroupIds => await groupsRepo.GetGroupsMembersAsUserIds(parameters.GroupIds),
				StudentsFilter.MyGroups => await groupsRepo.GetMyGroupsMembers(parameters.CourseId, UserId),
				_ => null
			};

			return new ManualCheckingQueueFilterOptions
			{
				CourseId = parameters.CourseId,
				UserIds = studentIds,
				SlidesIds = parameters.SlideIds,
				Count = parameters.Count,
				DateSort = parameters.DateSort,
				OnlyReviewed = reviewed
			};
		}

		private async Task<List<AbstractManualSlideChecking>> GetMergedCheckingQueue(
			ManualCheckingQueueFilterOptions filterOptions,
			ICourse course,
			bool isHistory = false,
			DateTime? minTimestamp = null,
			bool includeFields = false
		)
		{
			var exercises = await GetCheckingQueue<ManualExerciseChecking>(filterOptions, course, isHistory, minTimestamp, includeFields);
			var quizzes = await GetCheckingQueue<ManualQuizChecking>(filterOptions, course, isHistory, minTimestamp, includeFields);

			var result = exercises
				.Cast<AbstractManualSlideChecking>()
				.Concat(quizzes);

			result = filterOptions.DateSort is DateSort.Ascending
				? result.OrderBy(c => isHistory ? c.CheckedTimestamp : c.Timestamp)
				: result.OrderByDescending(c => isHistory ? c.CheckedTimestamp : c.Timestamp);

			if (filterOptions.Count > 0)
				result = result.Take(filterOptions.Count);

			return result.ToList();
		}

		private Task<List<T>> GetCheckingQueue<T>(
			ManualCheckingQueueFilterOptions filterOptions,
			ICourse course,
			bool isHistory = false,
			DateTime? minTimestamp = null,
			bool includeFields = false
		) where T : AbstractManualSlideChecking
		{
			var slideIds = GetSlidesWithManualChecking<T>(course);
			var filterSlideIds = filterOptions.SlidesIds is null
				? slideIds.ToList()
				: slideIds.Intersect(filterOptions.SlidesIds).ToList();

			if (filterSlideIds.Count == 0)
				return Task.FromResult(new List<T>());

			var filterWithSlideIds = new ManualCheckingQueueFilterOptions
			{
				CourseId = filterOptions.CourseId,
				UserIds = filterOptions.UserIds,
				SlidesIds = filterSlideIds,
				OnlyReviewed = filterOptions.OnlyReviewed,
				DateSort = filterOptions.DateSort,
				Count = filterOptions.Count,
				IsUserIdsSupplement = filterOptions.IsUserIdsSupplement
			};

			return isHistory
				? slideCheckingsRepo.GetCheckingQueueHistory<T>(filterWithSlideIds, minTimestamp, includeFields)
				: slideCheckingsRepo.GetManualCheckingQueue<T>(filterWithSlideIds, includeFields);
		}

		private static IEnumerable<Guid> GetSlidesWithManualChecking<T>(ICourse course) where T : AbstractManualSlideChecking
		{
			if (typeof(T) == typeof(ManualExerciseChecking))
				return course.GetSlidesNotSafe()
					.OfType<ExerciseSlide>()
					.Where(s => s.Scoring.RequireReview)
					.Select(s => s.Id);
			if (typeof(T) == typeof(ManualQuizChecking))
				return course.GetSlidesNotSafe()
					.OfType<QuizSlide>()
					.Where(s => s.Scoring.ManualChecking)
					.Select(s => s.Id);
			throw new InvalidOperationException(
				$"Type of {nameof(T)} should be {nameof(ManualExerciseChecking)} " +
				$"or {nameof(ManualQuizChecking)}. " +
				$"Actual: {typeof(T)}"
			);
		}

		private async Task<Dictionary<int, List<ShortReviewInfo>>> BuildReviewsInfo(IEnumerable<int> exercisesIds)
		{
			var reviewsByIds = await slideCheckingsRepo.GetExerciseCodeReviewForCheckings(exercisesIds);
			var exercisesWithReviewsIds = reviewsByIds.Keys.ToList();
			var solutionsByIds = await solutionsRepo.GetSolutionsForSubmissions(exercisesWithReviewsIds);
			exercisesWithReviewsIds = solutionsByIds.Keys.ToList();

			var result = new Dictionary<int, List<ShortReviewInfo>>();
			foreach (var id in exercisesWithReviewsIds)
			{
				var solution = solutionsByIds[id];
				if (string.IsNullOrWhiteSpace(solution))
					continue;
				result[id] = reviewsByIds[id]
					.Select(r => (
						review: r,
						startPos: solution.FindPositionByLineAndCharacter(r.StartLine, r.StartPosition),
						finishPos: solution.FindPositionByLineAndCharacter(r.FinishLine, r.FinishPosition)
					))
					.Where(reviewInfo => reviewInfo.finishPos - reviewInfo.startPos > 0)
					.Where(reviewInfo => reviewInfo.startPos >= 0 && reviewInfo.finishPos < solution.Length)
					.OrderBy(reviewInfo => reviewInfo.startPos)
					.Select(reviewInfo => new ShortReviewInfo
					{
						CommentId = reviewInfo.review.Id,
						Author = BuildShortUserInfo(reviewInfo.review.Author),
						Comment = CommentTextHelper.RenderCommentTextToHtml(reviewInfo.review.Comment),
						CodeFragment = solution[reviewInfo.startPos..reviewInfo.finishPos]
					})
					.ToList();
			}

			return result;
		}

		private static ReviewQueueItem BuildReviewQueueItem(
			AbstractManualSlideChecking checking,
			ICourse course,
			[CanBeNull] List<ShortReviewInfo> reviews = null
		)
		{
			var slide = course.GetSlideByIdNotSafe(checking.SlideId);
			var maxScore = (slide as ExerciseSlide)?.Scoring.ScoreWithCodeReview ?? slide.MaxScore;
			var score = checking is ManualQuizChecking quizChecking
				? quizChecking.Score
				: ((ManualExerciseChecking)checking).Percent is { } percent
					? SlideCheckingsRepo.ConvertExerciseManualCheckingPercentToScore(percent, maxScore)
					: (int?)null;

			return new ReviewQueueItem
			{
				SubmissionId = checking.Id,
				SlideId = checking.SlideId,
				User = BuildShortUserInfo(checking.User),
				Timestamp = checking.Timestamp,
				Score = score,
				MaxScore = maxScore,
				LockedBy = checking.LockedBy is null ? null : BuildShortUserInfo(checking.LockedBy),
				LockedUntil = checking.LockedUntil,
				CheckedTimestamp = checking.CheckedTimestamp,
				CheckedBy = checking.CheckedBy is null ? null : BuildShortUserInfo(checking.CheckedBy),
				Reviews = reviews ?? new List<ShortReviewInfo>()
			};
		}
	}
}