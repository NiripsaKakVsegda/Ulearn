using Database;
using Database.Extensions;
using Database.Migrations;
using Database.Models;
using Database.Repos;
using Database.Repos.Groups;
using Database.Utils;
using ElmahCore;
using LtiLibrary.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Extensions;
using Vostok.Logging.Abstractions;
using Ulearn.Core;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Core.Courses.Units;
using Ulearn.Core.Markdown;
using uLearn.Web.Core.Authorization;
using uLearn.Web.Core.Extensions;
using uLearn.Web.Core.Extensions.LTI;
using uLearn.Web.Core.Models;

namespace uLearn.Web.Core.Controllers;

[Authorize(Policy = UlearnAuthorizationConstants.StudentsPolicyName)]
public class CourseController : BaseController
{
	private static ILog log => LogProvider.Get().ForContext(typeof(CourseController));

	private readonly ICourseStorage courseStorage = WebCourseManager.CourseStorageInstance;

	private readonly string baseUrlApi;
	private readonly string baseUrlWeb;

	private readonly UlearnDb db;
	private readonly IUserSolutionsRepo solutionsRepo;
	private readonly IUnitsRepo unitsRepo;
	private readonly IVisitsRepo visitsRepo;
	private readonly ILtiRequestsRepo ltiRequestsRepo;
	private readonly ILtiConsumersRepo consumersRepo;
	private readonly ISlideCheckingsRepo slideCheckingsRepo;
	private readonly IGroupsRepo groupsRepo;
	private readonly IGroupAccessesRepo groupAccessesRepo;
	private readonly IUserQuizzesRepo userQuizzesRepo;
	private readonly IAdditionalContentPublicationsRepo additionalContentPublicationsRepo;
	private readonly ICoursesRepo coursesRepo;
	private readonly ITempCoursesRepo tempCoursesRepo;
	private readonly ICourseRolesRepo courseRoleTypesRepo;
	private readonly IGroupMembersRepo groupMembersRepo;
	private readonly LtiAuthentication ltiAuthentication;

	public CourseController(
		UlearnDb db,
		IUserSolutionsRepo solutionsRepo,
		IUnitsRepo unitsRepo,
		IVisitsRepo visitsRepo,
		ILtiRequestsRepo ltiRequestsRepo,
		ILtiConsumersRepo consumersRepo,
		ISlideCheckingsRepo slideCheckingsRepo,
		IGroupsRepo groupsRepo,
		IUserQuizzesRepo userQuizzesRepo,
		ICoursesRepo coursesRepo,
		ITempCoursesRepo tempCoursesRepo,
		IGroupMembersRepo groupMembersRepo,
		LtiAuthentication ltiAuthentication,
		ICourseRolesRepo courseRoleTypesRepo,
		IAdditionalContentPublicationsRepo additionalContentPublicationsRepo,
		IGroupAccessesRepo groupAccessesRepo)
	{
		this.db = db;
		this.solutionsRepo = solutionsRepo;
		this.unitsRepo = unitsRepo;
		this.visitsRepo = visitsRepo;
		this.ltiRequestsRepo = ltiRequestsRepo;
		this.consumersRepo = consumersRepo;
		this.slideCheckingsRepo = slideCheckingsRepo;
		this.groupsRepo = groupsRepo;
		this.userQuizzesRepo = userQuizzesRepo;
		this.coursesRepo = coursesRepo;
		this.tempCoursesRepo = tempCoursesRepo;
		this.courseRoleTypesRepo = courseRoleTypesRepo;
		this.groupAccessesRepo = groupAccessesRepo;
		this.groupMembersRepo = groupMembersRepo;
		this.ltiAuthentication = ltiAuthentication;
		this.additionalContentPublicationsRepo = additionalContentPublicationsRepo;

		var configuration = ApplicationConfiguration.Read<UlearnConfiguration>();
		baseUrlWeb = configuration.BaseUrl;
		baseUrlApi = configuration.BaseUrlApi;
	}

	[AllowAnonymous]
	public async Task<ActionResult> SlideById(string courseId, string slideId = "", int? checkQueueItemId = null, int? version = null, int autoplay = 0)
	{
		if (slideId.Contains("_"))
			slideId = slideId.Substring(slideId.LastIndexOf('_') + 1);

		// По крайней мере одно из мест использования groupsIds: переход на следующее ревью после выполнения предыдущего.
		var groupsIds = Request.Query["group"].ToList();

		if (!Guid.TryParse(slideId, out var slideGuid))
			return NotFound();

		if (string.IsNullOrWhiteSpace(courseId))
			return RedirectToAction("Index", "Home");

		var course = courseStorage.FindCourse(courseId);
		if (course == null)
			return NotFound();

		var visibleUnitIds = (await unitsRepo.GetVisibleUnitIds(course, User.GetUserId())).ToList();
		var visibleUnits = course.GetUnits(visibleUnitIds);
		var isGuest = !User.Identity.IsAuthenticated;
		var isInstructor = !isGuest && User.HasAccessFor(course.Id, CourseRoleType.Instructor);
		var slide = slideGuid == Guid.Empty
			? await GetInitialSlideForStartup(courseId, visibleUnits, isInstructor, User.GetUserId())
			: course.FindSlideById(slideGuid, isInstructor, visibleUnitIds);

		if (slide == null)
		{
			var instructorNote = course.FindInstructorNoteByIdNotSafe(slideGuid);
			if (instructorNote != null && isInstructor)
				slide = instructorNote;
		}

		if (slide == null)
			return NotFound();

		AbstractManualSlideChecking queueItem = null;
		var isManualCheckingReadonly = false;
		if (User.HasAccessFor(courseId, CourseRoleType.Instructor) && checkQueueItemId != null)
		{
			if (slide is QuizSlide)
				queueItem = await slideCheckingsRepo.FindManualCheckingById<ManualQuizChecking>(checkQueueItemId.Value);
			if (slide is ExerciseSlide)
				queueItem = await slideCheckingsRepo.FindManualCheckingById<ManualExerciseChecking>(checkQueueItemId.Value);

			if (queueItem == null)
			{
				/* It's possible when checking has not been fully checked, lock has been released, but after it user re-send his solution and we removed old waiting checking */
				return RedirectToAction("CheckingQueue", "Admin", new
				{
					courseId = courseId,
					message = "checking_removed"
				});
			}
		}

		var model = isGuest ?
			CreateGuestCoursePageModel(course, slide, autoplay > 0) :
			await CreateCoursePageModel(course, slide, queueItem, version, groupsIds, autoplay > 0, isManualCheckingReadonly);

		if (!string.IsNullOrEmpty(Request.Query["error"]))
			model.Error = Request.Query["error"];

		if (!visibleUnits.Contains(model.Slide.Unit))
			return NotFound("Slide is hidden " + slideGuid);
		return View("Slide", model);
	}

	[AllowAnonymous]
	public async Task<ActionResult> Slide(string courseId)
	{
		var course = courseStorage.FindCourse(courseId);
		if (course == null)
			return NotFound();
		var visibleUnitIds = await unitsRepo.GetVisibleUnitIds(course, User.GetUserId());
		var visibleUnits = course.GetUnits(visibleUnitIds);
		var isInstructor = User.HasAccessFor(course.Id, CourseRoleType.Instructor);
		var slide = await GetInitialSlideForStartup(courseId, visibleUnits, isInstructor, User.GetUserId());
		if (slide == null)
			return NotFound();
		return RedirectToRoute("Course.SlideById", new { courseId, slideId = slide.Url });
	}

	[AllowAnonymous]
	public async Task<ActionResult> LtiSlide(string courseId, Guid slideId)
	{
		if (string.IsNullOrWhiteSpace(courseId))
			return RedirectToAction("Index", "Home");

		var course = courseStorage.GetCourse(courseId);
		var visibleUnitIds = await unitsRepo.GetVisibleUnitIds(course, User.GetUserId());
		var slide = course.GetSlideById(slideId, false, visibleUnitIds);

		string userId;
		if (await Request.IsAuthenticatedWithLtiAsync())
		{
			var ltiRequest = await Request.ParseLtiRequestAsync();
			userId = await ltiAuthentication.Authenticate(HttpContext, ltiRequest);
			var ulearnLtiRequest = new Ulearn.Core.Model.LtiRequest
			{
				ConsumerKey = ltiRequest.ConsumerKey,
				LisOutcomeServiceUrl = ltiRequest.LisOutcomeServiceUrl,
				LisResultSourcedId = ltiRequest.LisResultSourcedId,
			};

			log.Info($"Нашёл LTI request в запросе: {ulearnLtiRequest.JsonSerialize()}");

			await ltiRequestsRepo.Update(courseId, userId, slide.Id, ulearnLtiRequest.JsonSerialize());

			var uriBuilder = new UriBuilder(ltiRequest.Url)
			{
				Scheme = Request.GetRealScheme(),
				Port = Request.GetRealPort()
			};

			return Redirect(uriBuilder.Uri.AbsoluteUri);
		}

		/* For now user should be authenticated */
		if (!User.Identity.IsAuthenticated)
			return Forbid();

		userId = User.GetUserId();
		var visit = await VisitSlide(courseId, slide.Id, userId);

		/* Try to send score via LTI immediately after slide visiting */
		try
		{
			if (visit.IsPassed)
				await LtiUtils.SubmitScore(ltiRequestsRepo, consumersRepo, visitsRepo, courseId, slide, userId, visit);
		}
		catch (Exception e)
		{
			await HttpContext.RaiseError(e);
		}

		userId = User.GetUserId();
		// Exercise обрабатывается реактом

		var quizSlide = slide as QuizSlide;
		if (quizSlide != null)
		{
			var model = new LtiQuizModel
			{
				CourseId = courseId,
				Slide = quizSlide,
				UserId = userId
			};
			return View("LtiQuizSlide", model);
		}

		return View();
	}

	private async Task<Slide> GetInitialSlideForStartup(
		string courseId,
		List<Unit> orderedVisibleUnits,
		bool isInstructor,
		string userId = null
	)
	{
		var (unitsPublications, slidesPublications)
			= await AdditionalContentPublicationUtils.GetPublications(groupMembersRepo, additionalContentPublicationsRepo, courseId, userId);

		var lastVisit = await visitsRepo.FindLastVisit(courseId, userId);
		var orderedVisibleSlides = orderedVisibleUnits
			.Where(u => isInstructor
						|| !u.Settings.IsExtraContent
						|| unitsPublications.TryGetValue(u.Id, out var unitPublication) && unitPublication.Date <= DateTime.Now)
			.SelectMany(u => u.GetSlides(isInstructor))
			.Where(s => isInstructor
						|| !s.IsExtraContent
						|| slidesPublications.TryGetValue(s.Id, out var slidePublication) && slidePublication.Date <= DateTime.Now)
			.ToList();
		if (lastVisit != null)
		{
			var lastVisitedSlide = orderedVisibleSlides.FirstOrDefault(s => s.Id == lastVisit.SlideId);
			if (lastVisitedSlide != null)
				return lastVisitedSlide;
			if (isInstructor)
			{
				var instructorNoteSlide = orderedVisibleUnits.FirstOrDefault(u => u.Id == lastVisit.SlideId)?.InstructorNote;
				if (instructorNoteSlide != null)
					return instructorNoteSlide;
			}
		}

		var unorderedVisitedIds = await visitsRepo.GetIdOfVisitedSlides(courseId, userId);
		var lastVisitedVisibleSlide = orderedVisibleSlides.LastOrDefault(slide => unorderedVisitedIds.Contains(slide.Id));
		if (lastVisitedVisibleSlide != null)
			return lastVisitedVisibleSlide;
		return orderedVisibleSlides.Any() ? orderedVisibleSlides.First() : null;
	}

	private CoursePageModel CreateGuestCoursePageModel(Course course, Slide slide, bool autoplay)
	{
		return new CoursePageModel
		{
			CourseId = course.Id,
			CourseTitle = course.Title,
			Slide = slide,
			BlockRenderContext = new BlockRenderContext(
				course,
				slide,
				baseUrlApi,
				baseUrlWeb,
				slide.Blocks.Select(block => block is AbstractExerciseBlock ? new ExerciseBlockData(course.Id, (ExerciseSlide)slide, false) { Url = Url } : (dynamic)null).ToArray(),
				isGuest: true,
				autoplay: autoplay),
			IsGuest = true,
		};
	}

	private async Task<CoursePageModel> CreateCoursePageModel(
		Course course, Slide slide,
		AbstractManualSlideChecking manualChecking, int? exerciseSubmissionId = null,
		List<string> groupsIds = null,
		bool autoplay = false,
		bool isManualCheckingReadonly = false)
	{
		var userId = User.GetUserId();

		if (manualChecking != null)
			userId = manualChecking.UserId;

		var defaultProhibitFurtherReview = await groupsRepo.GetDefaultProhibitFutherReviewForUser(course.Id, userId, User.GetUserId());
		var manualCheckingsLeftInQueue = manualChecking != null ? await ControllerUtils.GetManualCheckingsCountInQueue(slideCheckingsRepo, groupsRepo, groupAccessesRepo, User, course.Id, slide, groupsIds) : 0;

		var (notArchivedGroupNames, archivedGroupNames) = await GetGroupNames(course, manualChecking);

		var model = new CoursePageModel
		{
			UserId = userId,
			CourseId = course.Id,
			CourseTitle = course.Title,
			Slide = slide,
			BlockRenderContext = CreateRenderContext(
				course, slide, manualChecking, exerciseSubmissionId, groupsIds,
				autoplay: autoplay,
				isManualCheckingReadonly: isManualCheckingReadonly,
				defaultProhibitFurtherReview: defaultProhibitFurtherReview, manualCheckingsLeftInQueue: manualCheckingsLeftInQueue),
			ManualChecking = manualChecking,
			ContextManualCheckingUserGroups = notArchivedGroupNames,
			ContextManualCheckingUserArchivedGroups = archivedGroupNames,
			IsGuest = false,
		};
		return model;
	}

	private async Task<(string, string)> GetGroupNames(Course course, AbstractManualSlideChecking manualChecking)
	{
		var notArchivedGroupNames = "";
		var archivedGroupNames = "";
		if (manualChecking != null)
		{
			var userGroups = await groupMembersRepo.GetUsersGroupsAsync(course.Id, new List<string> { manualChecking.UserId }, true);
			if (userGroups.ContainsKey(manualChecking.UserId))
			{
				notArchivedGroupNames = string.Join(", ", userGroups[manualChecking.UserId].Where(g => !g.IsArchived), manualChecking.UserId, false);
				archivedGroupNames = string.Join(", ", userGroups[manualChecking.UserId].Where(g => g.IsArchived));
			}
		}

		return (notArchivedGroupNames, archivedGroupNames);
	}

	// returns null if user can't edit git
	private async Task<string> GetGitEditLink(Course course, string slideFilePathRelativeToCourse)
	{
		var courseRole = User.GetCourseRole(course.Id);
		var canEditGit = courseRole != null && courseRole <= CourseRoleType.CourseAdmin;
		if (!canEditGit)
			return null;
		var publishedCourseVersion = await coursesRepo.GetPublishedCourseVersion(course.Id);
		if (publishedCourseVersion?.RepoUrl == null)
			return null;
		if (publishedCourseVersion.PathToCourseXml == null)
			return null;
		var branch = (await coursesRepo.GetCourseRepoSettings(course.Id))?.Branch ?? "master";
		return GitUtils.GetSlideEditLink(publishedCourseVersion.RepoUrl, branch, publishedCourseVersion.PathToCourseXml, slideFilePathRelativeToCourse);
	}

	private async Task<int> GetMaxSlideScoreForUser(Course course, Slide slide, string userId)
	{
		var isSlideSolved = await ControllerUtils.IsSlideSolved(slideCheckingsRepo, course, userId, slide.Id);
		var hasManualChecking = visitsRepo.HasManualChecking(course.Id, userId, slide.Id);
		var enabledManualCheckingForUser = await groupsRepo.IsManualCheckingEnabledForUserAsync(course, userId);
		var maxSlideScore = ControllerUtils.GetMaxScoreForUsersSlide(slide, isSlideSolved, hasManualChecking, enabledManualCheckingForUser);
		return maxSlideScore;
	}

	private BlockRenderContext CreateRenderContext(Course course, Slide slide,
		AbstractManualSlideChecking manualChecking = null,
		int? exerciseSubmissionId = null, List<string> groupsIds = null, bool isLti = false,
		bool autoplay = false, bool isManualCheckingReadonly = false, bool defaultProhibitFurtherReview = true,
		int manualCheckingsLeftInQueue = 0)
	{
		/* ExerciseController will fill blockDatas later */
		var blockData = slide.Blocks.Select(b => (dynamic)null).ToArray();
		return new BlockRenderContext(
			course,
			slide,
			baseUrlApi,
			baseUrlWeb,
			blockData,
			isGuest: false,
			revealHidden: User.HasAccessFor(course.Id, CourseRoleType.Instructor),
			manualChecking: manualChecking,
			manualCheckingsLeftInQueue: manualCheckingsLeftInQueue,
			canUserFillQuiz: false,
			groupsIds: groupsIds,
			isLti: isLti,
			autoplay: autoplay,
			isManualCheckingReadonly: isManualCheckingReadonly,
			defaultProhibitFurtherReview: defaultProhibitFurtherReview
		)
		{
			VersionId = exerciseSubmissionId
		};
	}

	public async Task<Visit> VisitSlide(string courseId, Guid slideId, string userId)
	{
		if (string.IsNullOrEmpty(userId))
			return null;
		await visitsRepo.AddVisit(courseId, slideId, userId, GetRealClientIp());
		return await visitsRepo.FindVisit(courseId, slideId, userId);
	}

	[Authorize(Policy = UlearnAuthorizationConstants.InstructorsPolicyName)]
	public async Task<ActionResult> InstructorNote(string courseId, Guid unitId)
	{
		var course = courseStorage.GetCourse(courseId);
		var slide = course.GetUnitByIdNotSafe(unitId).InstructorNote;
		if (slide == null)
			return NotFound("No instructor note for this unit");
		var gitEditUrl = await GetGitEditLink(course, slide.SlideFilePathRelativeToCourse);
		return View(new InstructorNoteModel(slide, gitEditUrl, new MarkdownRenderContext(baseUrlApi, baseUrlWeb, courseId, slide.Unit.UnitDirectoryRelativeToCourse)));
	}

	[Authorize(Policy = UlearnAuthorizationConstants.InstructorsPolicyName)]
	public async Task<ActionResult> ForgetAll(string courseId, Guid slideId)
	{
		var slide = courseStorage.GetCourse(courseId).GetSlideByIdNotSafe(slideId);
		var userId = User.GetUserId();
		db.SolutionLikes.RemoveRange(db.SolutionLikes.Where(q => q.UserId == userId && q.Submission.SlideId == slideId));

		db.UserExerciseSubmissions.RemoveSlideAction(courseId, slideId, userId);
		db.UserQuizSubmissions.RemoveSlideAction(courseId, slideId, userId);
		db.Visits.RemoveSlideAction(courseId, slideId, userId);
		await slideCheckingsRepo.RemoveAttempts(courseId, slideId, userId, false);

		db.Hints.RemoveSlideAction(courseId, slideId, userId);
		await db.SaveChangesAsync();

		return RedirectToAction("SlideById", new { courseId, slideId = slide.Id });
	}

	public async Task<ActionResult> Courses(string courseId = null, string courseTitle = null)
	{
		var isSystemAdministrator = User.IsSystemAdministrator();
		var userId = User.GetUserId();
		var courses = courseStorage.GetCourses();

		// Неопубликованные курсы не покажем тем, кто не имеет роли в них.
		if (!isSystemAdministrator)
		{
			var visibleCourses = await unitsRepo.GetVisibleCourses();
			var coursesInWhichUserHasAnyRole = await courseRoleTypesRepo.GetCoursesWhereUserIsInRole(userId, CourseRoleType.Tester);
			var coursesInWhichUserHasVisit = db.LastVisits
				.Where(v => visibleCourses.Contains(v.CourseId) && v.UserId == userId)
				.Select(v => v.CourseId)
				.Distinct()
				.Select(i => db.LastVisits
					.Where(v => v.CourseId == i && v.UserId == userId)
					.OrderByDescending(v => v.Timestamp)
					.FirstOrDefault())
				.ToDictionary(v => v!.CourseId, v => v.Timestamp != null, StringComparer.OrdinalIgnoreCase);
			var coursesWhereIAmStudent = (await groupMembersRepo.GetUserGroupsAsync(userId))
				.Select(g => g.CourseId)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.Where(c => visibleCourses.Contains(c)).ToList();
			courses = courses.Where(c => coursesInWhichUserHasVisit.ContainsKey(c.Id)
										|| coursesInWhichUserHasAnyRole.Contains(c.Id, StringComparer.OrdinalIgnoreCase)
										|| coursesWhereIAmStudent.Contains(c.Id, StringComparer.OrdinalIgnoreCase));
		}

		var incorrectChars = new string(CourseManager.InvalidForCourseIdCharacters.OrderBy(c => c).Where(c => 32 <= c).ToArray());
		if (isSystemAdministrator)
			courses = courses.OrderBy(course => course.Id, StringComparer.InvariantCultureIgnoreCase);
		else
			courses = courses.OrderBy(course => course.Title, StringComparer.InvariantCultureIgnoreCase);

		var allTempCourses = (await tempCoursesRepo.GetAllTempCourses())
			.ToDictionary(t => t.CourseId, t => t, StringComparer.InvariantCultureIgnoreCase);
		var model = new CourseListViewModel
		{
			Courses = courses
				.Select(course => new CourseViewModel
				{
					Id = course.Id,
					Title = course.Title,
					TempCourse = allTempCourses.GetOrDefault(course.Id)
				})
				.ToList(),
			LastTryCourseId = courseId,
			LastTryCourseTitle = courseTitle,
			InvalidCharacters = incorrectChars
		};
		return View(model);
	}
}