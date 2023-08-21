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
using Microsoft.EntityFrameworkCore;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Courses.Slides.Exercises;
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

		public ReviewQueueController(
			ICourseStorage courseStorage,
			UlearnDb db,
			IUsersRepo usersRepo,
			ISlideCheckingsRepo slideCheckingsRepo,
			IGroupsRepo groupsRepo,
			ICourseRolesRepo courseRolesRepo
		)
			: base(courseStorage, db, usersRepo)
		{
			this.slideCheckingsRepo = slideCheckingsRepo;
			this.groupsRepo = groupsRepo;
			this.courseRolesRepo = courseRolesRepo;
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

			if (parameters.StudentsFilter is StudentsFilter.All && !isCourseAdmin)
				parameters.StudentsFilter = StudentsFilter.MyGroups;

			if (parameters.StudentsFilter is StudentsFilter.GroupIds)
			{
				parameters.GroupIds = parameters.GroupIds is null
					? new List<int>()
					: await GetValidGroupIds(parameters.GroupIds.Distinct().ToList(), isCourseAdmin);
			}

			if (parameters.StudentsFilter is StudentsFilter.StudentIds)
			{
				parameters.StudentIds = parameters.StudentIds is null
					? new List<string>()
					: await GetValidUserIds(parameters.StudentIds.Distinct().ToList(), isCourseAdmin);
			}

			var filterOptions = await BuildQueryFilterOptions(parameters, false);
			var checkings = await GetMergedCheckingQueue(filterOptions);

			return new ReviewQueueResponse
			{
				Checkings = checkings
					.Select(checking => BuildReviewQueueItem(
						course,
						checking,
						null
					))
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

			if (parameters.StudentsFilter is StudentsFilter.All && !isCourseAdmin)
				parameters.StudentsFilter = StudentsFilter.MyGroups;

			if (parameters.StudentsFilter is StudentsFilter.GroupIds)
			{
				parameters.GroupIds = parameters.GroupIds is null
					? new List<int>()
					: await GetValidGroupIds(parameters.GroupIds.Distinct().ToList(), isCourseAdmin);
			}

			if (parameters.StudentsFilter is StudentsFilter.StudentIds)
			{
				parameters.StudentIds = parameters.StudentIds is null
					? new List<string>()
					: await GetValidUserIds(parameters.StudentIds.Distinct().ToList(), isCourseAdmin);
			}

			var filterOptions = await BuildQueryFilterOptions(parameters, true);
			var checkings = await GetMergedCheckingQueue(filterOptions, true, parameters.MinCheckedTimestamp);

			var exercises = checkings.OfType<ManualExerciseChecking>().ToList();
			var reviews = exercises.Count > 0
				? BuildReviewsInfo(exercises)
				: null;

			return new ReviewQueueResponse
			{
				Checkings = checkings
					.Select(checking => BuildReviewQueueItem(
						course,
						checking,
						reviews?.GetOrDefault(checking.Id, null)
					))
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

		private Task<List<int>> GetValidGroupIds(ICollection<int> groupIds, bool isCourseAdmin)
		{
			var validGroupsQuery = db.Groups
				.Where(g => groupIds.Contains(g.Id) && !g.IsDeleted);
			if (!isCourseAdmin)
				validGroupsQuery = validGroupsQuery
					.GroupJoin(
						db.GroupAccesses,
						g => g.Id,
						ga => ga.GroupId,
						(group, accesses) => new { group, accesses }
					)
					.SelectMany(
						e => e.accesses.DefaultIfEmpty(),
						(e, access) => new { e.group, access }
					)
					.Where(e => e.group.OwnerId == UserId || (e.access.UserId == UserId && e.access.IsEnabled))
					.Select(e => e.group);

			return validGroupsQuery
				.Select(g => g.Id)
				.Distinct()
				.ToListAsync();
		}

		private Task<List<string>> GetValidUserIds(ICollection<string> userIds, bool isCourseAdmin)
		{
			var validUsersQuery = db.Users
				.Where(u => userIds.Contains(u.Id) && !u.IsDeleted);
			if (!isCourseAdmin)
				validUsersQuery = validUsersQuery
					.GroupJoin(
						db.GroupMembers,
						u => u.Id,
						member => member.UserId,
						(user, members) => new { user, members }
					)
					.SelectMany(
						e => e.members,
						(e, member) => new { e.user, member.Group }
					)
					.GroupJoin(
						db.GroupAccesses,
						e => e.Group.Id,
						ga => ga.GroupId,
						(e, accesses) => new { e.user, e.Group, accesses }
					)
					.SelectMany(
						e => e.accesses.DefaultIfEmpty(),
						(e, access) => new { e.user, e.Group, access }
					)
					.Where(e => e.Group.OwnerId == UserId || (e.access.UserId == UserId && e.access.IsEnabled))
					.Select(e => e.user);

			return validUsersQuery
				.Select(u => u.Id)
				.Distinct()
				.ToListAsync();
		}

		private async Task<ManualCheckingQueueFilterOptions> BuildQueryFilterOptions(ReviewQueueFilterParameters parameters, bool reviewed)
		{
			var studentIds = parameters.StudentsFilter switch
			{
				StudentsFilter.All => null,
				StudentsFilter.StudentIds => parameters.StudentIds,
				StudentsFilter.GroupIds => await groupsRepo.GetGroupsMembersAsUserIds(parameters.GroupIds),
				StudentsFilter.MyGroups => await groupsRepo.GetMyGroupsUsersIdsFilterAccessibleToUserAsync(parameters.CourseId, UserId),
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

		private async Task<List<AbstractManualSlideChecking>> GetMergedCheckingQueue(ManualCheckingQueueFilterOptions filterOptions, bool isHistory = false, DateTime? minTimestamp = null)
		{
			var exercises = filterOptions.OnlyReviewed ?? false
				? await slideCheckingsRepo.GetCheckingQueueHistory<ManualExerciseChecking>(filterOptions, minTimestamp)
				: await slideCheckingsRepo.GetManualCheckingQueue<ManualExerciseChecking>(filterOptions);

			var quizzes = filterOptions.OnlyReviewed ?? false
				? await slideCheckingsRepo.GetCheckingQueueHistory<ManualQuizChecking>(filterOptions, minTimestamp)
				: await slideCheckingsRepo.GetManualCheckingQueue<ManualQuizChecking>(filterOptions);

			var result = exercises
				.Cast<AbstractManualSlideChecking>()
				.Concat(quizzes);

			result = isHistory
				? filterOptions.DateSort is DateSort.Ascending
					? result.OrderBy(c => c.CheckedTimestamp)
					: result.OrderByDescending(c => c.CheckedTimestamp)
				: filterOptions.DateSort is DateSort.Ascending
					? result.OrderBy(c => c.Timestamp)
					: result.OrderByDescending(c => c.Timestamp);

			if (filterOptions.Count > 0)
				result = result.Take(filterOptions.Count);

			return result.ToList();
		}

		private static Dictionary<int, List<ShortReviewInfo>> BuildReviewsInfo(IEnumerable<ManualExerciseChecking> exercises)
		{
			var result = new Dictionary<int, List<ShortReviewInfo>>();
			foreach (var exercise in exercises)
			{
				var solution = exercise.Submission.SolutionCode.Text;
				if (string.IsNullOrEmpty(solution))
					continue;
				result[exercise.Id] = exercise.Reviews.Select(r => (
						review: r,
						startPos: solution.FindPositionByLineAndCharacter(r.StartLine, r.StartPosition),
						finishPos: solution.FindPositionByLineAndCharacter(r.FinishLine, r.FinishPosition)
					))
					.Where(reviewInfo => reviewInfo.finishPos - reviewInfo.startPos > 0)
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
			ICourse course,
			AbstractManualSlideChecking checking,
			[CanBeNull] List<ShortReviewInfo> reviews
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