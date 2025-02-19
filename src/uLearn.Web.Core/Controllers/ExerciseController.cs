﻿using System.Net;
using System.Runtime.Serialization;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Groups;
using Database.Repos.Users;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Ulearn.Common;
using Ulearn.Common.Api;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.Metrics;
using uLearn.Web.Core.Authorization;
using uLearn.Web.Core.Extensions;
using uLearn.Web.Core.Models;
using Vostok.Clusterclient.Core.Model;
using Vostok.Logging.Abstractions;
using Web.Api.Client;
using Web.Api.Configuration;

namespace uLearn.Web.Core.Controllers;

[Authorize(Policy = UlearnAuthorizationConstants.StudentsPolicyName)]
public class ExerciseController : Controller
{
	private readonly UlearnDb db;
	private readonly ICourseStorage courseStorage;

	private readonly IUserSolutionsRepo userSolutionsRepo;
	private readonly ISlideCheckingsRepo slideCheckingsRepo;
	private readonly IGroupsRepo groupsRepo;
	private readonly ICoursesRepo coursesRepo;
	private readonly ICourseRolesRepo courseRolesRepo;
	private readonly IGroupAccessesRepo groupAccessesRepo;
	private readonly IVisitsRepo visitsRepo;
	private readonly INotificationsRepo notificationsRepo;
	private readonly IUsersRepo usersRepo;
	private readonly IUnitsRepo unitsRepo;

	private readonly string baseUrlApi;
	private readonly string authCookieName;

	private static ILog log => LogProvider.Get().ForContext(typeof(ExerciseController));

	public ExerciseController(UlearnDb db,
		ICourseStorage courseStorage,
		WebConfiguration configuration,
		IUserSolutionsRepo userSolutionsRepo,
		ISlideCheckingsRepo slideCheckingsRepo,
		IGroupsRepo groupsRepo,
		IVisitsRepo visitsRepo,
		INotificationsRepo notificationsRepo,
		IUsersRepo usersRepo,
		IUnitsRepo unitsRepo,
		IGroupAccessesRepo groupAccessesRepo,
		ICoursesRepo coursesRepo,
		ICourseRolesRepo courseRolesRepo)
	{
		this.db = db;
		this.courseStorage = courseStorage;
		this.userSolutionsRepo = userSolutionsRepo;
		this.slideCheckingsRepo = slideCheckingsRepo;
		this.groupsRepo = groupsRepo;
		this.visitsRepo = visitsRepo;
		this.notificationsRepo = notificationsRepo;
		this.usersRepo = usersRepo;
		this.unitsRepo = unitsRepo;
		this.groupAccessesRepo = groupAccessesRepo;
		this.coursesRepo = coursesRepo;
		this.courseRolesRepo = courseRolesRepo;
		baseUrlApi = configuration.BaseUrlApi;
		authCookieName = configuration.Web.CookieName;
	}

	[Authorize(Policy = UlearnAuthorizationConstants.InstructorsPolicyName)]
	[HttpPost]
	public async Task<ActionResult> AddExerciseCodeReview(string courseId, int checkingId, [FromBody] ReviewInfo reviewInfo)
	{
		var checking = await slideCheckingsRepo.FindManualCheckingById<ManualExerciseChecking>(checkingId);
		if (!string.Equals(checking.CourseId, courseId, StringComparison.OrdinalIgnoreCase))
			return new ForbidResult();

		/* Make start position less than finish position */
		if (reviewInfo.StartLine > reviewInfo.FinishLine || (reviewInfo.StartLine == reviewInfo.FinishLine && reviewInfo.StartPosition > reviewInfo.FinishPosition))
		{
			var tmp = reviewInfo.StartLine;
			reviewInfo.StartLine = reviewInfo.FinishLine;
			reviewInfo.FinishLine = tmp;

			tmp = reviewInfo.StartPosition;
			reviewInfo.StartPosition = reviewInfo.FinishPosition;
			reviewInfo.FinishPosition = tmp;
		}

		var review = await slideCheckingsRepo.AddExerciseCodeReview(checking, User.GetUserId(), reviewInfo.StartLine, reviewInfo.StartPosition, reviewInfo.FinishLine, reviewInfo.FinishPosition, reviewInfo.Comment).ConfigureAwait(false);
		var currentUser = await usersRepo.FindUserById(User.GetUserId());

		return PartialView("_ExerciseReview", new ExerciseCodeReviewModel
		{
			Review = review,
			ManualChecking = checking,
			CurrentUser = currentUser,
			CanReply = true,
		});
	}

	[HttpPost]
	[Authorize(Policy = UlearnAuthorizationConstants.InstructorsPolicyName)]
	public async Task<ActionResult> DeleteExerciseCodeReview(string courseId, int reviewId)
	{
		var review = await slideCheckingsRepo.FindExerciseCodeReviewById(reviewId);
		var reviewCourseId = review.ExerciseCheckingId.HasValue ? review.ExerciseChecking.CourseId : review.Submission.CourseId;
		if (!reviewCourseId.EqualsIgnoreCase(courseId))
			return new ForbidResult();
		if (review.AuthorId != User.GetUserId() && !User.HasAccessFor(courseId, CourseRoleType.CourseAdmin))
			return new ForbidResult();

		await slideCheckingsRepo.DeleteExerciseCodeReview(review).ConfigureAwait(false);

		return Json(new CodeReviewOperationResult { Status = "ok" });
	}

	[Authorize(Policy = UlearnAuthorizationConstants.InstructorsPolicyName)]
	[HttpPost]
	public async Task<ActionResult> UpdateExerciseCodeReview(string courseId, int reviewId, string comment)
	{
		var review = await slideCheckingsRepo.FindExerciseCodeReviewById(reviewId);
		if (!review.ExerciseChecking.CourseId.EqualsIgnoreCase(courseId))
			return new ForbidResult();
		if (review.AuthorId != User.GetUserId())
			return new ForbidResult();

		await slideCheckingsRepo.UpdateExerciseCodeReview(review, comment).ConfigureAwait(false);

		return Json(new CodeReviewOperationResult { Status = "ok" });
	}

	[Authorize(Policy = UlearnAuthorizationConstants.InstructorsPolicyName)]
	[HttpPost]
	public async Task<ActionResult> HideFromTopCodeReviewComments(string courseId, Guid slideId, string comment)
	{
		var slide = courseStorage.FindCourse(courseId)?.FindSlideByIdNotSafe(slideId) as ExerciseSlide;
		if (slide == null)
			return new NotFoundResult();

		var userId = User.GetUserId();
		await slideCheckingsRepo.HideFromTopCodeReviewComments(courseId, slideId, userId, comment).ConfigureAwait(false);

		var topUserReviewComments = await slideCheckingsRepo.GetTopUserReviewComments(courseId, slideId, userId, 20);
		var topOtherUsersReviewComments = await slideCheckingsRepo.GetTopOtherUsersReviewComments(courseId, slideId, userId, 10, topUserReviewComments);
		return PartialView("_TopUserReviewComments", new ExerciseBlockData(courseId, slide, false)
		{
			TopUserReviewComments = topUserReviewComments,
			TopOtherUsersReviewComments = topOtherUsersReviewComments,
		});
	}

	[Authorize(Policy = UlearnAuthorizationConstants.CourseAdminsPolicyName)]
	public async Task<ActionResult> SlideCodeReviewComments(string courseId, Guid slideId)
	{
		var comments = await slideCheckingsRepo.GetLastYearReviewComments(courseId, slideId);
		return PartialView("_SlideCodeReviewComments", comments);
	}

	[HttpPost]
	public async Task<ActionResult> AddExerciseCodeReviewComment(int reviewId, string text)
	{
		var review = await slideCheckingsRepo.FindExerciseCodeReviewById(reviewId);
		var currentUserId = User.GetUserId();

		var submissionUserId = review.ExerciseCheckingId.HasValue ? review.ExerciseChecking.UserId : review.Submission.UserId;
		var submissionCourseId = review.ExerciseCheckingId.HasValue ? review.ExerciseChecking.CourseId : review.Submission.CourseId;
		var isInstructor = User.HasAccessFor(submissionCourseId, CourseRoleType.Instructor);
		if (submissionUserId != currentUserId && !isInstructor)
			return new ForbidResult();

		var canReply = isInstructor || !review.Author.IsUlearnBot() || review.NotDeletedComments.Any(c => !c.Author.IsUlearnBot());
		if (!canReply)
			return new ForbidResult();

		var comment = await slideCheckingsRepo.AddExerciseCodeReviewComment(currentUserId, reviewId, text).ConfigureAwait(false);

		if (review.ExerciseCheckingId.HasValue && review.ExerciseChecking.IsChecked)
		{
			var course = courseStorage.FindCourse(submissionCourseId);
			var slideId = review.ExerciseChecking.SlideId;
			var unit = course?.FindUnitBySlideIdNotSafe(slideId, isInstructor);
			if (unit != null && await unitsRepo.IsUnitVisibleForStudents(course, unit.Id))
				await NotifyAboutCodeReviewComment(comment).ConfigureAwait(false);
		}

		return PartialView("_ExerciseReviewComment", comment);
	}

	[HttpPost]
	public async Task<ActionResult> DeleteExerciseCodeReviewComment(int commentId)
	{
		var comment = await slideCheckingsRepo.FindExerciseCodeReviewCommentById(commentId);
		if (comment == null)
			return new NotFoundResult();

		var currentUserId = User.GetUserId();
		var courseId = comment.Review.ExerciseCheckingId.HasValue ? comment.Review.ExerciseChecking.CourseId : comment.Review.Submission.CourseId;
		if (comment.AuthorId != currentUserId && !User.HasAccessFor(courseId, CourseRoleType.CourseAdmin))
			return new ForbidResult();

		await slideCheckingsRepo.DeleteExerciseCodeReviewComment(comment).ConfigureAwait(false);

		return Json(new CodeReviewOperationResult { Status = "ok" });
	}


	/* Call NotifyAboutCodeReviewComment() only for checking's comment, not for submission's ones */
	private async Task NotifyAboutCodeReviewComment(ExerciseCodeReviewComment comment)
	{
		var courseId = comment.Review.ExerciseCheckingId.HasValue ? comment.Review.ExerciseChecking.CourseId : comment.Review.Submission.CourseId;
		await notificationsRepo.AddNotification(courseId, new ReceivedCommentToCodeReviewNotification
		{
			CommentId = comment.Id,
		}, comment.AuthorId).ConfigureAwait(false);
	}

	[HttpPost]
	[Authorize(Policy = UlearnAuthorizationConstants.InstructorsPolicyName)]
	public async Task<ActionResult> ScoreExercise(int id, string nextUrl, string exercisePercent, bool prohibitFurtherReview, string errorUrl = "", bool recheck = false)
	{
		if (string.IsNullOrEmpty(errorUrl))
			errorUrl = nextUrl;

		var checking = await slideCheckingsRepo.FindManualCheckingById<ManualExerciseChecking>(id);
		if (!await groupAccessesRepo.CanInstructorViewStudent(User.GetUserId(), checking.UserId, checking.CourseId))
			return new ForbidResult();

		using (var transaction = db.Database.BeginTransaction())
		{
			var course = courseStorage.GetCourse(checking.CourseId);
			var slide = (ExerciseSlide)course.FindSlideByIdNotSafe(checking.SlideId);

			/* Invalid form: percent isn't integer */
			if (!int.TryParse(exercisePercent, out var percent))
				return Json(new ScoreExerciseOperationResult { Status = "error", Redirect = errorUrl + "Неверное количество процентов" });

			/* Invalid form: score isn't from range 0..100 */
			if (percent < 0 || percent > 100)
				return Json(new ScoreExerciseOperationResult { Status = "error", Redirect = errorUrl + $"Неверное количество процентов: {percent}" });

			checking.ProhibitFurtherManualCheckings = prohibitFurtherReview;
			await slideCheckingsRepo.MarkManualExerciseCheckingAsChecked(checking, percent, User.GetUserId()).ConfigureAwait(false);
			await slideCheckingsRepo.MarkManualExerciseCheckingAsCheckedBeforeThis(checking).ConfigureAwait(false);
			if (prohibitFurtherReview)
				await slideCheckingsRepo.ProhibitFurtherExerciseManualChecking(checking.CourseId, checking.UserId, checking.SlideId).ConfigureAwait(false);
			else
				await slideCheckingsRepo.EnableFurtherManualCheckings(checking.CourseId, checking.UserId, checking.SlideId).ConfigureAwait(false);
			await visitsRepo.UpdateScoreForVisit(checking.CourseId, slide, checking.UserId).ConfigureAwait(false);

			var unit = course.FindUnitBySlideIdNotSafe(checking.SlideId, true);
			if (unit != null && await unitsRepo.IsUnitVisibleForStudents(course, unit.Id))
				await NotifyAboutManualExerciseChecking(checking).ConfigureAwait(false);

			transaction.Commit();
		}

		return Json(new ScoreExerciseOperationResult { Status = "ok" });
	}

	private async Task NotifyAboutManualExerciseChecking(ManualExerciseChecking checking)
	{
		var isRecheck = (await notificationsRepo.FindNotifications<PassedManualExerciseCheckingNotification>(n => n.CheckingId == checking.Id)).Any();
		var notification = new PassedManualExerciseCheckingNotification
		{
			Checking = checking,
			IsRecheck = isRecheck,
		};
		await notificationsRepo.AddNotification(checking.CourseId, notification, User.GetUserId()).ConfigureAwait(false);
	}

	[HttpPost]
	[Authorize(Policy = UlearnAuthorizationConstants.InstructorsPolicyName)]
	public async Task<ActionResult> SimpleScoreExercise(int submissionId, int exercisePercent, bool ignoreNewestSubmission = false)
	{
		var submission = await userSolutionsRepo.FindSubmissionById(submissionId);
		var courseId = submission.CourseId;
		var slideId = submission.SlideId;
		var userId = submission.UserId;

		if (!User.HasAccessFor(courseId, CourseRoleType.Instructor))
			return new ForbidResult();

		var slide = courseStorage.FindCourse(courseId)?.FindSlideByIdNotSafe(slideId) as ExerciseSlide;
		if (slide == null)
			return new NotFoundResult();

		if (!ignoreNewestSubmission && submission.ManualChecking == null)
		{
			var lastAcceptedSubmission = userSolutionsRepo.GetAllAcceptedSubmissionsByUser(courseId, slideId, userId).OrderByDescending(s => s.Timestamp).FirstOrDefault();
			if (lastAcceptedSubmission != null && lastAcceptedSubmission.Id != submission.Id)
				return Json(
					new SimpleScoreExerciseResult
					{
						Status = "error",
						Error = "has_newest_submission",
						SubmissionId = lastAcceptedSubmission.Id,
						SubmissionDate = lastAcceptedSubmission.Timestamp.ToAgoPrettyString(true)
					});
		}

		if (exercisePercent is < 0 or > 100)
		{
			return Json(
				new SimpleScoreExerciseResult
				{
					Status = "error",
					Error = "invalid_score",
				});
		}

		await slideCheckingsRepo.RemoveWaitingManualCheckings<ManualExerciseChecking>(courseId, slideId, userId).ConfigureAwait(false);
		ManualExerciseChecking checking;
		if (submission.ManualChecking != null)
			checking = submission.ManualChecking;
		else
			checking = await slideCheckingsRepo.AddManualExerciseChecking(courseId, slideId, userId, submission.Id).ConfigureAwait(false);

		if (!await groupAccessesRepo.CanInstructorViewStudent(User.GetUserId(), checking.UserId, checking.CourseId))
			return new ForbidResult();

		await slideCheckingsRepo.LockManualChecking(checking, User.GetUserId()).ConfigureAwait(false);
		await slideCheckingsRepo.MarkManualExerciseCheckingAsChecked(checking, exercisePercent, User.GetUserId()).ConfigureAwait(false);
		/* 100%-score sets ProhibitFurtherChecking to true */
		if (exercisePercent == 100)
			await slideCheckingsRepo.ProhibitFurtherExerciseManualChecking(checking.CourseId, checking.UserId, checking.SlideId).ConfigureAwait(false);

		await visitsRepo.UpdateScoreForVisit(courseId, slide, userId).ConfigureAwait(false);

		await NotifyAboutManualExerciseChecking(checking).ConfigureAwait(false);

		return Json(
			new SimpleScoreExerciseResult
			{
				Status = "ok",
				Percent = exercisePercent,
				CheckingId = checking.Id,
			});
	}

	public async Task<ActionResult> SubmissionsPanel(string courseId, Guid slideId, string userId = "", int? currentSubmissionId = null, bool canTryAgain = true)
	{
		var isInstructor = User.HasAccessFor(courseId, CourseRoleType.Instructor);
		if (!isInstructor)
			userId = "";

		if (userId == "")
			userId = User.GetUserId();

		var course = courseStorage.GetCourse(courseId);
		var visibleUnits = await unitsRepo.GetVisibleUnitIds(course, User.GetUserId());
		var slide = course.FindSlideById(slideId, isInstructor, visibleUnits);
		var submissions = await userSolutionsRepo.GetAllAcceptedSubmissionsByUser(courseId, slideId, userId).ToListAsync();

		return PartialView(new ExerciseSubmissionsPanelModel(courseId, slide)
		{
			Submissions = submissions,
			CurrentSubmissionId = currentSubmissionId,
			CanTryAgain = canTryAgain,
		});
	}

	private async Task<ExerciseBlockData> CreateExerciseBlockData(Course course, Slide slide, UserExerciseSubmission submission, bool onlyAccepted, string currentUserId)
	{
		var userId = submission?.UserId ?? currentUserId;
		var visit = await visitsRepo.FindVisit(course.Id, slide.Id, userId);

		var solution = submission?.SolutionCode.Text;
		if (string.IsNullOrEmpty(solution))
		{
			/* I.e. after clicking on `Try again` button solution is empty. Let's try to show last sent submission */
			var lastSubmission = userSolutionsRepo.GetAllSubmissionsByUser(course.Id, slide.Id, currentUserId).OrderByDescending(s => s.Timestamp).FirstOrDefault();
			solution = lastSubmission?.SolutionCode.Text;
		}

		var submissionReviews = submission?.GetAllReviews();

		var hasCheckedReview = submission?.ManualChecking?.IsChecked;
		var reviewState = hasCheckedReview == true ? ExerciseReviewState.Reviewed :
			hasCheckedReview == false ? ExerciseReviewState.WaitingForReview :
			ExerciseReviewState.NotReviewed;

		var submissions = onlyAccepted ?
			userSolutionsRepo.GetAllAcceptedSubmissionsByUser(course.Id, slide.Id, userId) :
			userSolutionsRepo.GetAllSubmissionsByUser(course.Id, slide.Id, userId);

		return new ExerciseBlockData(course.Id, (ExerciseSlide)slide, (visit?.IsSkipped ?? false) || (visit?.IsPassed ?? false), solution)
		{
			Url = Url,
			Reviews = submissionReviews?.ToList() ?? new List<ExerciseCodeReview>(),
			ReviewState = reviewState,
			IsGuest = string.IsNullOrEmpty(currentUserId),
			SubmissionSelectedByUser = submission,
			Submissions = submissions.OrderByDescending(s => s.Timestamp).ToList(),
			CurrentUser = await usersRepo.FindUserById(User.GetUserId())
		};
	}

	private UserExerciseSubmission GetExerciseSubmissionShownByDefault(string courseId, Guid slideId, string userId, bool allowNotAccepted = false)
	{
		var submissions = userSolutionsRepo
			.GetAllAcceptedSubmissionsByUser(courseId, slideId, userId)
			.OrderByDescending(s => s.Timestamp)
			.ToList();
		var lastSubmission = submissions.FirstOrDefault(s => s.ManualChecking != null) ??
							submissions.FirstOrDefault(s => s.AutomaticCheckingIsRightAnswer);
		if (lastSubmission == null && allowNotAccepted)
			lastSubmission = userSolutionsRepo
				.GetAllSubmissionsByUser(courseId, slideId, userId)
				.OrderByDescending(s => s.Timestamp)
				.ToList()
				.FirstOrDefault();
		return lastSubmission;
	}

	[AllowAnonymous]
	[Authorize(Policy = UlearnAuthorizationConstants.InstructorsPolicyName)]
	public async Task<ActionResult> Submission(string courseId, Guid slideId, string userId = null, int? submissionId = null, int? manualCheckingId = null, bool isLti = false, bool showOutput = false, bool instructorView = false, bool onlyAccepted = true)
	{
		var currentUserId = userId ?? (User.Identity.IsAuthenticated ? User.GetUserId() : "");
		UserExerciseSubmission submission = null;
		if (submissionId.HasValue && submissionId.Value > 0)
		{
			submission = await userSolutionsRepo.FindSubmissionById(submissionId.Value);
			if (submission == null)
				return new NotFoundResult();
			if (!string.Equals(courseId, submission.CourseId, StringComparison.OrdinalIgnoreCase))
				return new NotFoundResult();
			if (slideId != submission.SlideId)
				return new NotFoundResult();
			if (!User.HasAccessFor(courseId, CourseRoleType.Instructor) && submission.UserId != currentUserId)
				return new ForbidResult();
		}
		else if (!submissionId.HasValue && !manualCheckingId.HasValue)
		{
			submission = GetExerciseSubmissionShownByDefault(courseId, slideId, currentUserId, instructorView);
		}

		var course = courseStorage.GetCourse(courseId);
		var slide = course.FindSlideByIdNotSafe(slideId);
		if (slide == null)
			return new NotFoundResult();

		ManualExerciseChecking manualChecking = null;
		if (User.HasAccessFor(courseId, CourseRoleType.Instructor) && manualCheckingId.HasValue)
		{
			manualChecking = await slideCheckingsRepo.FindManualCheckingById<ManualExerciseChecking>(manualCheckingId.Value);
		}

		if (manualChecking != null && !submissionId.HasValue)
			submission = manualChecking.Submission;

		var model = await CreateExerciseBlockData(course, slide, submission, onlyAccepted, currentUserId);
		model.IsLti = isLti;
		model.ShowOutputImmediately = showOutput;
		model.InstructorView = instructorView;
		model.ShowOnlyAccepted = onlyAccepted;
		if (manualChecking != null)
		{
			if (manualChecking.CourseId.EqualsIgnoreCase(courseId))
			{
				model.ManualChecking = manualChecking;
				model.Reviews = submission?.GetAllReviews() ?? new List<ExerciseCodeReview>();
			}

			model.TopUserReviewComments = await slideCheckingsRepo.GetTopUserReviewComments(course.Id, slide.Id, currentUserId, 20);
			model.TopOtherUsersReviewComments = await slideCheckingsRepo.GetTopOtherUsersReviewComments(course.Id, slide.Id, currentUserId, 10, model.TopUserReviewComments);
		}

		return PartialView(model);
	}

	public ActionResult LastReviewComments(string courseId, Guid slideId, string userId)
	{
		var reviewedSubmission = userSolutionsRepo
			.GetAllAcceptedSubmissionsByUser(courseId, new[] { slideId }, userId)
			.Where(s => s.ManualChecking != null && s.ManualChecking.IsChecked)
			.OrderByDescending(s => s.Timestamp)
			.FirstOrDefault();
		var manualChecking = reviewedSubmission?.ManualChecking;

		if (manualChecking == null || !manualChecking.NotDeletedReviews.Any())
			return new EmptyResult();
		return PartialView("~/Views/Exercise/_ExerciseLastReviewComments.cshtml",
			new ExerciseLastReviewCommentModel
			{
				ReviewedSubmission = reviewedSubmission,
				NotDeletedReviews = manualChecking.NotDeletedReviews
			});
	}

	public async Task<ActionResult> ExerciseScoreForm(BlockRenderContext context)
	{
		var checking = (ManualExerciseChecking)context.ManualChecking;
		var prevReviewPercent = await slideCheckingsRepo.GetLastReviewPercentForExerciseSlide(
			context.Course.Id,
			checking.SlideId,
			checking.UserId,
			checking.Submission.Timestamp);
		var model = new ExerciseScoreFormModel(
			context.Course.Id,
			(ExerciseSlide)context.Slide,
			checking,
			context.ManualCheckingsLeftInQueue,
			prevReviewPercent,
			context.GroupsIds,
			isCurrentSubmissionChecking: (context.VersionId == null || checking.Submission.Id == context.VersionId) && !context.IsManualCheckingReadonly,
			defaultProhibitFurtherReview: context.DefaultProhibitFurtherReview
		);
		return PartialView("~/Views/Exercise/_ExerciseScoreForm.cshtml", model);
	}

	[Authorize(Policy = UlearnAuthorizationConstants.InstructorsPolicyName)]
	public async Task<ActionResult> StudentSubmissions(string courseId, Guid slideId)
	{
		var model = await GetStudentSubmissionsModel(courseId, slideId, "");
		return PartialView(model.Value);
	}

	[Authorize(Policy = UlearnAuthorizationConstants.InstructorsPolicyName)]
	public async Task<ActionResult> StudentSubmissionsTable(string courseId, Guid slideId, string name)
	{
		var model = await GetStudentSubmissionsModel(courseId, slideId, name);
		model.Value.ShowAll = true;
		return PartialView(model.Value);
	}

	private async Task<ActionResult<StudentSubmissionsModel>> GetStudentSubmissionsModel(string courseId, Guid slideId, string name)
	{
		const int maxUsersCount = 30;

		var course = courseStorage.GetCourse(courseId);
		var slide = course.GetSlideByIdNotSafe(slideId) as ExerciseSlide;

		if (slide == null)
			return new NotFoundResult();

		var canViewAllSubmissions = User.HasAccessFor(courseId, CourseRoleType.CourseAdmin) || await User.HasCourseAccess(coursesRepo, courseId, CourseAccessType.ViewAllStudentsSubmissions);
		var hasFilterByName = !string.IsNullOrEmpty(name);

		/* By default show members of `my` groups, but if filter is enabled then course admin's and users with special access can view any student's submissions */

		SubmissionsFilterOptions filterOptions;
		var slideIdInList = new List<Guid> { slideId };
		var visitedUserIds = visitsRepo.GetVisitsInPeriod(new VisitsFilterOptions { CourseId = courseId, SlidesIds = slideIdInList, PeriodStart = DateTime.MinValue, PeriodFinish = DateTime.MaxValue })
			.Select(v => v.UserId)
			.ToList();
		if (hasFilterByName && canViewAllSubmissions)
		{
			/* Get all members who has visits to this slide */
			filterOptions = new SubmissionsFilterOptions
			{
				CourseId = courseId,
				UserIds = visitedUserIds,
				SlideIds = slideIdInList,
			};
		}
		else
		{
			/* Get members of `my` groups */
			filterOptions = await ControllerUtils.GetFilterOptionsByGroup<SubmissionsFilterOptions>(groupsRepo, groupAccessesRepo, User, courseId, groupsIds: new List<string>());
			filterOptions.SlideIds = slideIdInList;
			/* Filter out only users with visits to this slide */
			filterOptions.UserIds = filterOptions.UserIds.Intersect(visitedUserIds).ToList();
		}

		if (hasFilterByName)
		{
			var filteredUserIds = await courseRolesRepo.FilterUsersByNamePrefix(name);
			filterOptions.UserIds = filterOptions.UserIds.Intersect(filteredUserIds).ToList();
		}

		filterOptions.UserIds = filterOptions.UserIds.Take(maxUsersCount).ToList();

		var submissions = userSolutionsRepo.GetAllSubmissionsByUsers(filterOptions);
		var submissionsByUser = submissions.ToList().GroupBy(s => s.UserId).ToDictionary(g => g.Key, g => g.ToList()).ToDefaultDictionary(); // NOTE: ToList because Include not work with GroupBy

		var scores = await visitsRepo.GetScore(courseId, slideId, filterOptions.UserIds);

		var userGroups = (await groupAccessesRepo.GetUsersGroupsNamesAsStrings(courseId, filterOptions.UserIds, User.GetUserId(), actual: true, archived: false)).ToDefaultDictionary();

		return new StudentSubmissionsModel
		{
			CourseId = courseId,
			Slide = slide,
			Users = (await usersRepo.GetUsersByIds(filterOptions.UserIds)).ToDictionary(u => u.Id),
			SubmissionsByUser = submissionsByUser,
			Scores = scores,
			HasFilterByName = hasFilterByName,
			UserGroups = userGroups,
		};
	}

	[Obsolete("Use api")] // Для openedu и stepik
	[AllowAnonymous]
	public async Task<ActionResult> StudentZip(string courseId, Guid slideId, string fileName)
	{
		if (string.IsNullOrEmpty(courseId) || string.IsNullOrEmpty(slideId.ToString()) || string.IsNullOrEmpty(fileName))
			return BadRequest();

		log.Warn("StudentZip request {courseId} {slideId}");
		var isInstructor = User.HasAccessFor(courseId, CourseRoleType.Instructor);
		var course = courseStorage.GetCourse(courseId);
		var visibleUnits = await unitsRepo.GetVisibleUnitIds(course, User.GetUserId());
		var slide = courseStorage.FindCourse(courseId)?.FindSlideById(slideId, isInstructor, visibleUnits);
		if (!(slide is ExerciseSlide))
			return new NotFoundResult();

		var exerciseSlide = slide as ExerciseSlide;
		if (exerciseSlide.Exercise is SingleFileExerciseBlock)
			return new NotFoundResult();
		if ((exerciseSlide.Exercise as UniversalExerciseBlock)?.NoStudentZip ?? false)
			return new NotFoundResult();

		var studentZipName = !string.IsNullOrEmpty(fileName)
			? fileName
			: ((exerciseSlide.Exercise as CsProjectExerciseBlock)?.CsprojFileName ?? new DirectoryInfo((exerciseSlide.Exercise as UniversalExerciseBlock).ExerciseDirPath).Name) + ".zip";

		var cookie = Request.Headers.Cookie;
		IWebApiClient webApiClient = new WebApiClient(new ApiClientSettings(baseUrlApi));
		var response = await webApiClient.GetStudentZipFile(courseId, slideId, studentZipName, cookie.Count == 0 ? null : new Header("Cookie", cookie));

		if (response == null)
			return new StatusCodeResult(StatusCodes.Status500InternalServerError);
		if (response.Code != ResponseCode.Ok)
			return new StatusCodeResult((int)response.Code);
		if (response.HasStream)
			return new FileStreamResult(response.Stream, "application/zip");
		if (response.HasContent)
			return new FileContentResult(response.Content.ToArray(), "application/zip");
		return new StatusCodeResult(StatusCodes.Status500InternalServerError);
	}
}

[DataContract]
public class CodeReviewOperationResult
{
	[DataMember(Name = "status")]
	public string Status { get; set; }
}

[DataContract]
public class ScoreExerciseOperationResult
{
	[DataMember(Name = "status")]
	public string Status { get; set; }

	[DataMember(Name = "redirect")]
	public string Redirect { get; set; }
}

[DataContract]
public class SimpleScoreExerciseResult
{
	[DataMember(Name = "status")]
	public string Status { get; set; }

	[DataMember(Name = "error")]
	public string Error { get; set; }

	[DataMember(Name = "submissionId")]
	public int SubmissionId { get; set; }

	[DataMember(Name = "submissionDate")]
	public string SubmissionDate { get; set; }

	[DataMember(Name = "percent")]
	public int Percent { get; set; }

	[DataMember(Name = "checkedQueueUrl")]
	public string CheckedQueueUrl { get; set; }

	[DataMember(Name = "checkingId")]
	public int CheckingId { get; set; }
}

public class ReviewInfo
{
	//[AllowHtml]
	public string Comment { get; set; }

	public int StartLine { get; set; }
	public int StartPosition { get; set; }
	public int FinishLine { get; set; }
	public int FinishPosition { get; set; }
}

public class ExerciseSubmissionsPanelModel
{
	public ExerciseSubmissionsPanelModel(string courseId, Slide slide)
	{
		CourseId = courseId;
		Slide = slide;

		Submissions = new List<UserExerciseSubmission>();
		CurrentSubmissionId = null;
		CanTryAgain = true;
		ShowButtons = true;
		SelectControlName = "version";
	}

	public string CourseId { get; set; }
	public Slide Slide { get; set; }
	public List<UserExerciseSubmission> Submissions { get; set; }
	public int? CurrentSubmissionId { get; set; }
	public bool CanTryAgain { get; set; }
	public bool ShowButtons { get; set; }
	public string SelectControlName { get; set; }

	[CanBeNull]
	public Func<UserExerciseSubmission, string> GetSubmissionDescription { get; set; }

	/* By default it's Url.RouteUrl("Course.SlideById", new { Model.CourseId, slideId = Model.Slide.Url }) */
	[CanBeNull]
	public string FormUrl { get; set; }
}

public class ExerciseScoreFormModel
{
	public ExerciseScoreFormModel(string courseId, ExerciseSlide slide, ManualExerciseChecking checking,
		int manualCheckingsLeftInQueueInQueue, int? prevReviewPercent, List<string> groupsIds = null,
		bool isCurrentSubmissionChecking = false, bool defaultProhibitFurtherReview = true)
	{
		CourseId = courseId;
		Slide = slide;
		Checking = checking;
		ManualCheckingsLeftInQueue = manualCheckingsLeftInQueueInQueue;
		GroupsIds = groupsIds;
		IsCurrentSubmissionChecking = isCurrentSubmissionChecking;
		DefaultProhibitFurtherReview = defaultProhibitFurtherReview;
		PrevReviewPercent = prevReviewPercent;
	}

	public string CourseId { get; set; }
	public ExerciseSlide Slide { get; set; }
	public ManualExerciseChecking Checking { get; set; }
	public List<string> GroupsIds { get; set; }
	public string GroupsIdsJoined => string.Join(",", GroupsIds ?? new List<string>());
	public bool IsCurrentSubmissionChecking { get; set; }
	public bool DefaultProhibitFurtherReview { get; set; }
	public int ManualCheckingsLeftInQueue { get; set; }
	public int? PrevReviewPercent { get; set; }
}

public class ExerciseLastReviewCommentModel
{
	public UserExerciseSubmission ReviewedSubmission { get; set; }
	public List<ExerciseCodeReview> NotDeletedReviews { get; set; }
}

public class StudentSubmissionsModel
{
	public string CourseId { get; set; }
	public ExerciseSlide Slide { get; set; }

	public Dictionary<string, ApplicationUser> Users { get; set; }

	public DefaultDictionary<string, List<UserExerciseSubmission>> SubmissionsByUser { get; set; }

	public Dictionary<string, int> Scores { get; set; }

	public bool HasFilterByName { get; set; }

	public DefaultDictionary<string, string> UserGroups { get; set; }

	public bool ShowAll { get; set; }
}