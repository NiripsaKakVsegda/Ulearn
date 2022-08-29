// using Database;
// using Database.Models;
// using Database.Repos;
// using Database.Repos.Groups;
// using Microsoft.AspNetCore.Mvc;
// using Telegram.Bot.Types;
// using Ulearn.Common;
// using Ulearn.Common.Extensions;
// using Ulearn.Core.Configuration;
// using Ulearn.Core.Courses.Manager;
// using Ulearn.Core.Courses.Slides.Quizzes;
// using Ulearn.Core.Courses.Slides.Quizzes.Blocks;
// using Ulearn.Core.Metrics;
// using uLearn.Web.Core.Controllers;
// using uLearn.Web.Core.Models;
// using Vostok.Logging.Abstractions;
//
// namespace uLearn.Web.Core.ViewComponents;
//
// public class Quiz
// {
// 	private static ILog log => LogProvider.Get().ForContext(typeof(QuizController));
//
// 	private const int defaultMaxTriesCount = 2;
// 	public const int InfinityTriesCount = int.MaxValue - 1;
// 	public const int MaxFillInBlockSize = 8 * 1024;
//
// 	private readonly UlearnDb db;
// 	private readonly ICourseStorage courseStorage = WebCourseManager.CourseStorageInstance;
// 	private readonly MetricSender metricSender;
//
// 	private readonly IUserQuizzesRepo userQuizzesRepo;
// 	private readonly IVisitsRepo visitsRepo;
// 	private readonly IGroupsRepo groupsRepo;
// 	private readonly IGroupAccessesRepo groupAccessesRepo;
// 	private readonly ISlideCheckingsRepo slideCheckingsRepo;
// 	private readonly INotificationsRepo notificationsRepo;
// 	private readonly ICourseRolesRepo courseRolesRepo;
// 	private readonly IUnitsRepo unitsRepo;
//
// 	private readonly string baseUrlWeb;
// 	private readonly string baseUrlApi;
//
// 	public Quiz(IUserQuizzesRepo userQuizzesRepo, IVisitsRepo visitsRepo, IGroupsRepo groupsRepo, ISlideCheckingsRepo slideCheckingsRepo, INotificationsRepo notificationsRepo, IUnitsRepo unitsRepo, IGroupAccessesRepo groupAccessesRepo, UlearnDb db, ICourseRolesRepo courseRolesRepo)
// 	{
// 		this.userQuizzesRepo = userQuizzesRepo;
// 		this.visitsRepo = visitsRepo;
// 		this.groupsRepo = groupsRepo;
// 		this.slideCheckingsRepo = slideCheckingsRepo;
// 		this.notificationsRepo = notificationsRepo;
// 		this.unitsRepo = unitsRepo;
// 		this.groupAccessesRepo = groupAccessesRepo;
// 		this.db = db;
// 		this.courseRolesRepo = courseRolesRepo;
// 		var configuration = ApplicationConfiguration.Read<UlearnConfiguration>();
// 		baseUrlWeb = configuration.BaseUrl;
// 		baseUrlApi = configuration.BaseUrlApi;
// 		metricSender = new MetricSender(configuration.GraphiteServiceName);
// 	}
//
// 	public async Task<IViewComponentResult> InvokeAsync(Guid slideId, string courseId, bool isGuest, bool isLti = false, int? checkQueueItemId = null, int? send = null, bool attempt = false)
// 	{ 
// 		metricSender.SendCount("quiz.show");
// 		if (isLti)
// 			metricSender.SendCount("quiz.show.lti");
// 		metricSender.SendCount($"quiz.show.{courseId}");
// 		metricSender.SendCount($"quiz.show.{courseId}.{slideId.ToString()}");
//
// 		var course = courseStorage.FindCourse(courseId);
// 		if (course == null)
// 			return new NotFoundResult();
//
// 		var userId = User.GetUserId();
// 		var visibleUnitIds = (await unitsRepo.GetVisibleUnitIds(course, userId)).ToList();
// 		var isCourseAdmin = !isGuest && await courseRolesRepo.HasUserAccessToCourse(userId, courseId, CourseRoleType.CourseAdmin);
// 		var isInstructor = isCourseAdmin || !isGuest && await courseRolesRepo.HasUserAccessToCourse(userId, courseId, CourseRoleType.Instructor);
//
// 		var slide = course.FindSlideById(slideId, isInstructor, visibleUnitIds) as QuizSlide;
// 		
// 		ManualQuizChecking manualQuizCheckQueueItem = null;
// 		var isManualCheckingReadonly = false;
// 		if (isInstructor && checkQueueItemId.HasValue)
// 			manualQuizCheckQueueItem = await slideCheckingsRepo.FindManualCheckingById<ManualQuizChecking>(checkQueueItemId.Value);
// 		
// 		if (isGuest)
// 		{
// 			metricSender.SendCount("quiz.show.to.guest");
// 			return PartialView(GuestQuiz(course, slide));
// 		}
//
// 		var isManualCheckingEnabledForUser = slide.ManualChecking && await groupsRepo.IsManualCheckingEnabledForUserAsync(course, userId);
// 		var maxAttemptsCount = GetMaxAttemptsCount(courseId, slide);
// 		var userScores = await GetUserScoresForBlocks(courseId, userId, slideId, manualQuizCheckQueueItem?.Submission);
//
// 		var score = userScores?.AsEnumerable().Sum(res => res.Value) ?? 0;
//
// 		var state = await GetQuizState(courseId, userId, slideId, score, slide.MaxScore, manualQuizCheckQueueItem?.Submission);
//
// 		log.Info($"Показываю тест для пользователя {userId} в слайде {courseId}:{slide.Id}, isLti = {isLti}");
//
// 		/* If it's manual checking, change quiz state to IsChecking for correct rendering */
// 		if (manualQuizCheckQueueItem != null)
// 			state.Status = QuizStatus.IsCheckingByInstructor;
//
// 		/* For manually checked quizzes show last attempt's answers until ?attempt=true is defined in query string */
// 		if (slide.ManualChecking && manualQuizCheckQueueItem == null && state.Status == QuizStatus.ReadyToSend && state.UsedAttemptsCount > 0 && !attempt)
// 			state.Status = QuizStatus.Sent;
//
// 		/* We also want to show user's answer if user sent answers just now */
// 		if (state.Status == QuizStatus.ReadyToSend && send.HasValue)
// 			state.Status = QuizStatus.Sent;
//
// 		var userAnswers = await userQuizzesRepo.GetAnswersForShowingOnSlideAsync(courseId, slide, userId, manualQuizCheckQueueItem?.Submission);
// 		var canUserFillQuiz = (!slide.ManualChecking || isManualCheckingEnabledForUser) && CanUserFillQuiz(state.Status);
//
// 		var questionAnswersFrequency = new DefaultDictionary<string, DefaultDictionary<string, int>>();
// 		if (isCourseAdmin)
// 		{
// 			var choiceBlocks = slide.Blocks.OfType<ChoiceBlock>().ToList();
// 			var answersFrequency = await userQuizzesRepo.GetAnswersFrequencyForChoiceBlocks(courseId, slide.Id, choiceBlocks);
// 			questionAnswersFrequency = answersFrequency.Keys.ToDictionary(
// 				blockId => blockId,
// 				blockId => answersFrequency[blockId].ToDefaultDictionary()
// 			).ToDefaultDictionary();
// 		}
//
// 		var model = new QuizModel
// 		{
// 			Course = course,
// 			Slide = slide,
// 			BaseUrlWeb = baseUrlWeb,
// 			BaseUrlApi = baseUrlApi,
// 			QuizState = state,
// 			MaxAttemptsCount = maxAttemptsCount,
// 			UserScores = userScores,
// 			AnswersToQuizzes = userAnswers,
// 			IsLti = isLti,
// 			Checking = manualQuizCheckQueueItem,
// 			ManualCheckingsLeftInQueue = manualQuizCheckQueueItem != null ? await GetManualCheckingsCountInQueue(course, slide) : 0,
// 			CanUserFillQuiz = canUserFillQuiz,
// 			GroupsIds = Request.GetMultipleValuesFromQueryString("group"),
// 			QuestionAnswersFrequency = questionAnswersFrequency,
// 			IsManualCheckingEnabledForUser = isManualCheckingEnabledForUser, 
// 			IsInstructor = isInstructor,
// 			IsCourseAdmin = isCourseAdmin, 
// 			IsGuest = isGuest
// 		};
//
// 		return PartialView(model);
// 	}
// }