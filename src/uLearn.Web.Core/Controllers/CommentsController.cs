using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using Database;
using Database.Models;
using Database.Models.Comments;
using Database.Repos;
using Database.Repos.Comments;
using Database.Repos.SystemAccessesRepo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Quizzes;
using uLearn.Web.Core.Authorization;
using uLearn.Web.Core.Extensions;
using uLearn.Web.Core.Models;

namespace uLearn.Web.Core.Controllers;

public class CommentsController : Controller
{
	private readonly ICourseStorage courseStorage = WebCourseManager.CourseStorageInstance;
	private readonly ICommentsRepo commentsRepo;
	private readonly ICommentLikesRepo commentLikesRepo;
	private readonly ICommentPoliciesRepo commentPoliciesRepo;
	private readonly INotificationsRepo notificationsRepo;
	private readonly ICoursesRepo coursesRepo;
	private readonly ISystemAccessesRepo systemAccessesRepo;
	private readonly IUnitsRepo unitsRepo;
	private readonly UlearnUserManager userManager;

	public CommentsController(ICommentsRepo commentsRepo, INotificationsRepo notificationsRepo, ICoursesRepo coursesRepo, ISystemAccessesRepo systemAccessesRepo, IUnitsRepo unitsRepo, UlearnUserManager userManager, ICommentPoliciesRepo commentPoliciesRepo, ICommentLikesRepo commentLikesRepo)
	{
		this.commentsRepo = commentsRepo;
		this.notificationsRepo = notificationsRepo;
		this.coursesRepo = coursesRepo;
		this.systemAccessesRepo = systemAccessesRepo;
		this.unitsRepo = unitsRepo;
		this.userManager = userManager;
		this.commentPoliciesRepo = commentPoliciesRepo;
		this.commentLikesRepo = commentLikesRepo;
	}

	public async Task<ActionResult> SlideComments(string courseId, Guid slideId, bool openInstructorsComments = false)
	{
		var course = courseStorage.GetCourse(courseId);
		var visibleUnits = await unitsRepo.GetVisibleUnitIds(course, User.GetUserId());
		var slide = course.FindSlideById(slideId, false, visibleUnits);
		if (slide == null)
			return Content("");

		var userId = User.GetUserId();
		var systemAccesses = await systemAccessesRepo.GetSystemAccessesAsync(userId);
		var courseAccesses = await coursesRepo.GetCourseAccesses(courseId, userId);
		var slideType = GetSlideType(slide);

		var model = new SlideCommentsModel
		{
			CourseId = courseId,
			Slide = slide,
			CurrentUser = User.Identity.IsAuthenticated ? await userManager.FindByIdAsync(userId) : null,
			OpenInstructorsComments = openInstructorsComments,
			CourseAccesses = courseAccesses,
			SystemAccesses = systemAccesses,
			SlideType = slideType
		};
		return PartialView(model);
	}

	private static SlideType GetSlideType(Slide slide)
	{
		switch (slide)
		{
			case ExerciseSlide _:
				return SlideType.Exercise;
			case QuizSlide _:
				return SlideType.Quiz;
			default:
				return SlideType.Lesson;
		}
	}

	private async Task<bool> CanModerateComments(ClaimsPrincipal user, string courseId)
	{
		if (!user.Identity.IsAuthenticated)
			return false;

		var hasCourseAccessForCommentEditing = await coursesRepo.HasCourseAccess(user.GetUserId(), courseId, CourseAccessType.EditPinAndRemoveComments);
		return user.HasAccessFor(courseId, CourseRoleType.CourseAdmin) || hasCourseAccessForCommentEditing;
	}

	private async Task<bool> CanAddCommentHere(ClaimsPrincipal user, string courseId, bool isReply)
	{
		if (!User.Identity.IsAuthenticated)
			return false;

		var commentsPolicy = await commentPoliciesRepo.GetCommentsPolicyAsync(courseId);
		var isInstructor = user.HasAccessFor(courseId, CourseRoleType.Instructor);

		if (!isInstructor && !commentsPolicy.IsCommentsEnabled)
			return false;

		if (isReply && !isInstructor && commentsPolicy.OnlyInstructorsCanReply)
			return false;

		return true;
	}

	private bool CanViewAndAddCommentsForInstructorsOnly(ClaimsPrincipal user, string courseId)
	{
		return user.HasAccessFor(courseId, CourseRoleType.Instructor);
	}

	private async Task<bool> CanAddCommentNow(ClaimsPrincipal user, string courseId)
	{
		// Instructors have unlimited comments
		if (user.HasAccessFor(courseId, CourseRoleType.Instructor))
			return true;

		var commentsPolicy = await commentPoliciesRepo.GetCommentsPolicyAsync(courseId);
		return !await commentsRepo.IsUserAddedMaxCommentsInLastTimeAsync(user.GetUserId(),
			commentsPolicy.MaxCommentsCountInLastTime,
			commentsPolicy.LastTimeForMaxCommentsLimit);
	}

	[Authorize(Policy = UlearnAuthorizationBuilder.StudentsPolicyName)]
	[HttpPost]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public async Task<ActionResult> AddComment(string courseId, Guid slideId, bool forInstructorsOnly, string commentText, string parentCommentId)
	{
		var parentCommentIdInt = -1;
		if (parentCommentId != null)
			int.TryParse(parentCommentId, out parentCommentIdInt);

		if (!await CanAddCommentHere(User, courseId, parentCommentIdInt != -1))
			return new ForbidResult();

		if (!await CanAddCommentNow(User, courseId))
		{
			return Json(new
			{
				status = "too-fast",
				message = "Вы комментируете слишком быстро. Подождите немного...",
			});
		}

		if (commentText.Length > CommentsPolicy.MaxCommentLength)
		{
			return Json(new
			{
				status = "too-long",
				message = "Слишком длинный комментарий. Попробуйте сократить мысль.",
			});
		}

		if (forInstructorsOnly && !CanViewAndAddCommentsForInstructorsOnly(User, courseId))
		{
			forInstructorsOnly = false;
		}

		var comment = await commentsRepo.AddCommentAsync(User.GetUserId(), courseId, slideId, parentCommentIdInt, forInstructorsOnly, commentText);
		if (comment.IsApproved)
			await NotifyAboutNewComment(comment);
		var canReply = await CanAddCommentHere(User, courseId, isReply: true);

		var userId = User.GetUserId();
		var canViewAuthorSubmissions = await coursesRepo.HasCourseAccess(userId, courseId, CourseAccessType.ViewAllStudentsSubmissions) || User.HasAccessFor(courseId, CourseRoleType.CourseAdmin);
		var canViewProfiles = await systemAccessesRepo.HasSystemAccessAsync(userId, SystemAccessType.ViewAllProfiles) || User.IsSystemAdministrator();

		return PartialView("_Comment", new CommentViewModel
		{
			Comment = comment,
			LikesCount = 0,
			IsLikedByUser = false,
			Replies = new List<CommentViewModel>(),
			IsCommentVisibleForUser = true,
			CanEditAndDeleteComment = true,
			CanModerateComment = User.HasAccessFor(courseId, CourseRoleType.Instructor),
			CanReply = canReply,
			CurrentUser = await userManager.FindByIdAsync(User.GetUserId()),
			CanViewAuthorProfile = canViewProfiles,
			CanViewAuthorSubmissions = canViewAuthorSubmissions,
		});
	}

	private async Task NotifyAboutNewComment(Comment comment)
	{
		var courseId = comment.CourseId;

		if (!comment.IsTopLevel)
		{
			var parentComment = await commentsRepo.FindCommentByIdAsync(comment.ParentCommentId);
			if (parentComment != null)
			{
				var replyNotification = new RepliedToYourCommentNotification
				{
					Comment = comment,
					ParentComment = parentComment,
				};
				await notificationsRepo.AddNotification(courseId, replyNotification, comment.AuthorId);
			}
		}

		/* Create NewCommentFromStudentFormYourGroupNotification later than RepliedToYourCommentNotification, because the last one is blocker for the first one.
		* We don't send NewCommentNotification if there is a RepliedToYouCommentNotification */
		var commentFromYourGroupStudentNotification = new NewCommentFromYourGroupStudentNotification { Comment = comment };
		await notificationsRepo.AddNotification(courseId, commentFromYourGroupStudentNotification, comment.AuthorId);

		/* Create NewCommentNotification later than RepliedToYourCommentNotification and NewCommentFromYourGroupStudentNotification, because the last one is blocker for the first one.
		* We don't send NewCommentNotification if there is a RepliedToYouCommentNotification or NewCommentFromYourGroupStudentNotification */
		var notification = comment.IsForInstructorsOnly
			? (Notification)new NewCommentForInstructorsOnlyNotification { Comment = comment }
			: new NewCommentNotification { Comment = comment };
		await notificationsRepo.AddNotification(courseId, notification, comment.AuthorId);
	}

	[Authorize(Policy = UlearnAuthorizationBuilder.StudentsPolicyName)]
	[HttpPost]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public async Task<ActionResult> LikeComment(int commentId)
	{
		var userId = User.GetUserId();
		await commentLikesRepo.LikeAsync(commentId, userId);
		var likesCount = await commentLikesRepo.GetLikesAsync(commentId);
		var liked = await commentLikesRepo.DidUserLikeComment(commentId, userId);

		await NotifyAboutLikedComment(commentId);

		return Json(new { likesCount, liked });
	}

	private async Task NotifyAboutLikedComment(int commentId)
	{
		var comment = await commentsRepo.FindCommentByIdAsync(commentId);
		if (comment != null)
		{
			var userId = User.GetUserId();
			var notification = new LikedYourCommentNotification
			{
				Comment = comment,
				LikedUserId = userId,
			};
			await notificationsRepo.AddNotification(comment.CourseId, notification, userId);
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public async Task<ActionResult> ApproveComment(int commentId, bool isApproved = true)
	{
		var comment = await commentsRepo.FindCommentByIdAsync(commentId);
		if (comment == null)
			return new NotFoundResult();

		if (!await CanModerateComments(User, comment.CourseId))
			return new ForbidResult();

		await commentsRepo.ApproveCommentAsync(commentId, isApproved);
		if (isApproved)
			await NotifyAboutNewComment(comment);
		return new OkResult();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public async Task<ActionResult> PinComment(int commentId, bool isPinned)
	{
		var comment = await commentsRepo.FindCommentByIdAsync(commentId);
		if (comment == null)
			return new NotFoundResult();

		if (!await CanModerateComments(User, comment.CourseId))
			return new ForbidResult();

		await commentsRepo.PinCommentAsync(commentId, isPinned);
		return new OkResult();
	}

	private async Task<bool> CanEditAndDeleteComment(ClaimsPrincipal user, Comment comment)
	{
		if (comment == null)
			return false;

		return await CanModerateComments(user, comment.CourseId) || user.GetUserId() == comment.AuthorId;
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public async Task<ActionResult> DeleteComment(int commentId)
	{
		var comment = await commentsRepo.FindCommentByIdAsync(commentId);
		if (!await CanEditAndDeleteComment(User, comment))
			return new ForbidResult();

		await commentsRepo.DeleteCommentAsync(commentId);
		return new OkResult();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public async Task<ActionResult> RestoreComment(int commentId)
	{
		var comment = await commentsRepo.FindCommentByIdAsync(commentId);
		if (!await CanEditAndDeleteComment(User, comment))
			return new ForbidResult();

		await commentsRepo.RestoreCommentAsync(commentId);
		return new OkResult();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public async Task<ActionResult> EditCommentText(int commentId, string newText)
	{
		var comment = await commentsRepo.FindCommentByIdAsync(commentId);
		if (!await CanEditAndDeleteComment(User, comment))
			return new ForbidResult();

		var newComment = await commentsRepo.EditCommentTextAsync(commentId, newText);
		return PartialView("_CommentText", newComment);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public async Task<ActionResult> MarkAsCorrectAnswer(int commentId, bool isCorrect = true)
	{
		var comment = await commentsRepo.FindCommentByIdAsync(commentId);
		if (!await CanEditAndDeleteComment(User, comment))
			return new ForbidResult();

		await commentsRepo.MarkCommentAsCorrectAnswerAsync(commentId, isCorrect);
		return new OkResult();
	}
}

public class SlideCommentsModel
{
	public string CourseId { get; set; }
	public Slide Slide { get; set; }
	public ApplicationUser CurrentUser { get; set; }
	public bool OpenInstructorsComments { get; set; }
	public List<CourseAccess> CourseAccesses { get; set; }
	public List<SystemAccess> SystemAccesses { get; set; }
	public SlideType SlideType { get; set; }
}