using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Database;
using Database.Extensions;
using Database.Models;
using Database.Repos;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models.Parameters.Review;
using Ulearn.Web.Api.Models.Responses.Review;

namespace Ulearn.Web.Api.Controllers.Review
{
	public class ReviewCommentsController : BaseController
	{
		private readonly ISlideCheckingsRepo slideCheckingsRepo;
		private readonly ICourseRolesRepo courseRolesRepo;
		private readonly IUnitsRepo unitsRepo;
		private readonly INotificationsRepo notificationsRepo;

		public ReviewCommentsController(ICourseStorage courseStorage, UlearnDb db, IUsersRepo usersRepo,
			ISlideCheckingsRepo slideCheckingsRepo, ICourseRolesRepo courseRolesRepo, IUnitsRepo unitsRepo, INotificationsRepo notificationsRepo)
			: base(courseStorage, db, usersRepo)
		{
			this.slideCheckingsRepo = slideCheckingsRepo;
			this.courseRolesRepo = courseRolesRepo;
			this.unitsRepo = unitsRepo;
			this.notificationsRepo = notificationsRepo;
			this.notificationsRepo = notificationsRepo;
		}

		/// <summary>
		/// Добавить комментарий к сделанному преподавателем ревью (т.е. замечанию к коду)
		/// </summary>
		[HttpPost("reviews/{reviewId}/comments")]
		[Authorize]
		[SwaggerResponse((int)HttpStatusCode.Forbidden, "You don't have access to this comment")]
		[SwaggerResponse((int)HttpStatusCode.TooManyRequests, "You are commenting too fast. Please wait some time")]
		[SwaggerResponse((int)HttpStatusCode.RequestEntityTooLarge, "Your comment is too large")]
		public async Task<ActionResult<ReviewCommentResponse>> AddExerciseCodeReviewComment([FromRoute] int reviewId, [FromBody] ReviewCreateCommentParameters parameters)
		{
			var review = await slideCheckingsRepo.FindExerciseCodeReviewById(reviewId);

			var submissionUserId = review.ExerciseCheckingId.HasValue ? review.ExerciseChecking.UserId : review.Submission.UserId;
			var submissionCourseId = review.ExerciseCheckingId.HasValue ? review.ExerciseChecking.CourseId : review.Submission.CourseId;
			var isInstructor = await courseRolesRepo.HasUserAccessToCourse(UserId, submissionCourseId, CourseRoleType.Instructor);
			if (submissionUserId != UserId && !isInstructor)
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You can't comment this review"));

			var canReply = isInstructor || !review.Author.IsUlearnBot() || review.NotDeletedComments.Any(c => !c.Author.IsUlearnBot());
			if (!canReply)
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You can't reply this review"));

			var comment = await slideCheckingsRepo.AddExerciseCodeReviewComment(UserId, reviewId, parameters.Text);

			if (review.ExerciseCheckingId.HasValue && review.ExerciseChecking.IsChecked)
			{
				var course = courseStorage.FindCourse(submissionCourseId);
				var slideId = review.ExerciseChecking.SlideId;
				var unit = course?.FindUnitBySlideIdNotSafe(slideId, isInstructor);
				if (unit != null && await unitsRepo.IsUnitVisibleForStudents(course, unit.Id))
					await NotifyAboutCodeReviewComment(comment);
			}

			return ReviewCommentResponse.Build(comment);
		}

		/// <summary>
		/// Изменить содержание ревью
		/// </summary>
		[HttpPatch("reviews/{reviewId}/comments/{commentId}")]
		[Authorize]
		public async Task<ActionResult<ReviewCommentResponse>> EditReviewComment([FromRoute] int commentId, [FromBody] ReviewCreateCommentParameters parameters)
		{
			var comment = await slideCheckingsRepo.FindExerciseCodeReviewCommentById(commentId);
			if (comment == null)
				return NotFound(new ErrorResponse($"Comment {commentId} not found"));

			var courseId = comment.Review.ExerciseCheckingId.HasValue ? comment.Review.ExerciseChecking.CourseId : comment.Review.Submission.CourseId;
			if (comment.AuthorId != UserId && !await courseRolesRepo.HasUserAccessToCourse(UserId, courseId, CourseRoleType.CourseAdmin))
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You can't delete this comment"));

			await slideCheckingsRepo.EditExerciseCodeReviewComment(comment, parameters.Text);

			return ReviewCommentResponse.Build(comment);
		}

		/// <summary>
		/// Удалить комментарий к ревью
		/// </summary>
		[HttpDelete("reviews/{reviewId}/comments/{commentId:int:min(0)}")]
		[Authorize]
		[SwaggerResponse((int)HttpStatusCode.Forbidden, "You don't have access to this comment")]
		public async Task<ActionResult> DeleteExerciseCodeReviewComment([FromRoute] int reviewId, [FromRoute] int commentId)
		{
			var comment = await slideCheckingsRepo.FindExerciseCodeReviewCommentById(commentId);
			if (comment == null)
				return NotFound(new ErrorResponse($"Comment {commentId} not found"));

			var courseId = comment.Review.ExerciseCheckingId.HasValue ? comment.Review.ExerciseChecking.CourseId : comment.Review.Submission.CourseId;
			if (comment.AuthorId != UserId && !await courseRolesRepo.HasUserAccessToCourse(UserId, courseId, CourseRoleType.CourseAdmin))
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You can't delete this comment"));

			await slideCheckingsRepo.DeleteExerciseCodeReviewComment(comment).ConfigureAwait(false);

			return Ok(new SuccessResponseWithMessage($"Review comment {commentId} successfully deleted"));
		}

		// Оповещает о создании комментария к ревью, а не самого ревью (т.е. замечения к коду)
		private async Task NotifyAboutCodeReviewComment(ExerciseCodeReviewComment comment)
		{
			var courseId = comment.Review.ExerciseCheckingId.HasValue ? comment.Review.ExerciseChecking.CourseId : comment.Review.Submission.CourseId;
			await notificationsRepo.AddNotification(courseId, new ReceivedCommentToCodeReviewNotification
			{
				CommentId = comment.Id,
			}, comment.AuthorId);
		}
	}
}