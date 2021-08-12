using System;
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
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models.Responses.Submissions;

namespace Ulearn.Web.Api.Controllers.Submissions
{
	[Route("/submissions")]
	public class SubmissionsController : BaseController
	{
		private readonly IUserSolutionsRepo userSolutionsRepo;
		private readonly ISlideCheckingsRepo slideCheckingsRepo;
		private readonly ICourseRolesRepo courseRolesRepo;
		private readonly IVisitsRepo visitsRepo;
		private readonly IUnitsRepo unitsRepo;
		private readonly IGroupAccessesRepo groupAccessesRepo;
		private readonly INotificationsRepo notificationsRepo;

		public SubmissionsController(
			ICourseStorage courseStorage,
			UlearnDb db,
			IUsersRepo usersRepo,
			IUserSolutionsRepo userSolutionsRepo,
			ICourseRolesRepo courseRolesRepo,
			ISlideCheckingsRepo slideCheckingsRepo,
			IGroupAccessesRepo groupAccessesRepo,
			IVisitsRepo visitsRepo,
			INotificationsRepo notificationsRepo,
			IUnitsRepo unitsRepo)
			: base(courseStorage, db, usersRepo)
		{
			this.courseRolesRepo = courseRolesRepo;
			this.userSolutionsRepo = userSolutionsRepo;
			this.slideCheckingsRepo = slideCheckingsRepo;
			this.unitsRepo = unitsRepo;
			this.groupAccessesRepo = groupAccessesRepo;
			this.visitsRepo = visitsRepo;
			this.notificationsRepo = notificationsRepo;
		}

		[HttpGet]
		[Authorize]
		public async Task<ActionResult<SubmissionsResponse>> GetSubmissions([FromQuery] [CanBeNull] string userId, [FromQuery] string courseId, [FromQuery] Guid slideId)
		{
			var isCourseAdmin = await courseRolesRepo.HasUserAccessToCourse(UserId, courseId, CourseRoleType.CourseAdmin);
			if (userId != null && !isCourseAdmin)
			{
				var isInstructor = await courseRolesRepo.HasUserAccessToCourse(UserId, courseId, CourseRoleType.Instructor);
				if (!isInstructor)
					return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You don't have access to view submissions"));
			}
			else
				userId ??= UserId;

			var submissions = await userSolutionsRepo
				.GetAllSubmissionsByUserAllInclude(courseId, slideId, userId)
				.OrderByDescending(s => s.Timestamp)
				.ToListAsync();
			var submissionsScores = await slideCheckingsRepo.GetCheckedPercentsBySubmissions(courseId, slideId, userId, null);
			var codeReviewComments = await slideCheckingsRepo.GetExerciseCodeReviewComments(courseId, slideId, userId);
			var reviewId2Comments = codeReviewComments
				?.GroupBy(c => c.ReviewId)
				.ToDictionary(g => g.Key, g => g.AsEnumerable());
			var prohibitFurtherManualChecking = submissions.Any(s => s.ManualChecking?.ProhibitFurtherManualCheckings ?? false);

			return SubmissionsResponse.Build(submissions, submissionsScores, reviewId2Comments, isCourseAdmin, prohibitFurtherManualChecking);
		}

		[HttpPost("{submissionId}/manual-checking")]
		[Authorize]
		public async Task<ActionResult> EnableManualChecking([FromRoute] string submissionId)
		{
			var submission = await userSolutionsRepo.FindSubmissionById(submissionId);

			if (!await groupAccessesRepo.HasInstructorEditAccessToStudentGroup(UserId, submission.UserId))
				return StatusCode((int)HttpStatusCode.Forbidden, "You don't have access to edit ProhibitFurtherReview flag for this submission");

			if (submission.ManualChecking != null)
				Ok($"Manual checking already enabled for submission {submissionId}");

			await slideCheckingsRepo.AddManualExerciseChecking(submission.CourseId, submission.SlideId, submission.UserId, submission.Id);
			await visitsRepo.MarkVisitsAsWithManualChecking(submission.CourseId, submission.SlideId, submission.UserId);

			return Ok($"Manual checking enabled for submission {submissionId}");
		}

		[HttpPost("{submissionId}/score")]
		[Authorize]
		public async Task<ActionResult> Score([FromRoute] string submissionId, [FromQuery] int percent)
		{
			var submission = await userSolutionsRepo.FindSubmissionById(submissionId);
			var checking = submission.ManualChecking;

			if (!await groupAccessesRepo.HasInstructorEditAccessToStudentGroup(User.GetUserId(), submission.UserId))
				return StatusCode((int)HttpStatusCode.Forbidden, "You don't have access to score this submission");

			/* Invalid form: score isn't from range 0..100 */
			if (percent is < 0 or > 100)
			{
				return StatusCode((int)HttpStatusCode.BadRequest, $"Неверное количество процентов: {percent}");
			}

			var course = courseStorage.GetCourse(submission.CourseId);
			var slide = course.FindSlideByIdNotSafe(submission.SlideId);

			await using (var transaction = await db.Database.BeginTransactionAsync())
			{
				await slideCheckingsRepo.MarkManualExerciseCheckingAsChecked(checking, percent);
				await slideCheckingsRepo.MarkManualExerciseCheckingAsCheckedBeforeThis(checking);
				await visitsRepo.UpdateScoreForVisit(checking.CourseId, slide, checking.UserId);

				var visibleUnits = await unitsRepo.GetPublishedUnitIds(course);
				if (course.FindSlideById(submission.SlideId, false, visibleUnits) != null)
					await NotifyAboutManualExerciseChecking(checking);

				await transaction.CommitAsync();
			}

			return Ok($"Submission {submissionId} scored {percent} successfully");
		}

		private async Task NotifyAboutManualExerciseChecking(ManualExerciseChecking checking)
		{
			var isRecheck = (await notificationsRepo
					.FindNotifications<PassedManualExerciseCheckingNotification>(n => n.CheckingId == checking.Id))
				.Any();

			var notification = new PassedManualExerciseCheckingNotification
			{
				Checking = checking,
				IsRecheck = isRecheck,
			};

			await notificationsRepo.AddNotification(checking.CourseId, notification, UserId);
		}
	}
}