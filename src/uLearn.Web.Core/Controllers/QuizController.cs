using Database;
using Database.Models;
using Database.Models.Quizzes;
using Database.Repos;
using Database.Repos.Groups;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Core.Courses.Slides.Quizzes.Blocks;
using Ulearn.Core.Extensions;
using Ulearn.Core.Metrics;
using uLearn.Web.Core.Authorization;
using uLearn.Web.Core.Extensions;
using uLearn.Web.Core.Extensions.LTI;
using uLearn.Web.Core.Models;
using Vostok.Logging.Abstractions;

namespace uLearn.Web.Core.Controllers;

[Authorize(Policy = UlearnAuthorizationBuilder.StudentsPolicyName)]//[ULearnAuthorize]
public class QuizController : Controller
{
	private static ILog log => LogProvider.Get().ForContext(typeof(QuizController));

	private const int defaultMaxTriesCount = 2;
	public const int InfinityTriesCount = int.MaxValue - 1;
	public const int MaxFillInBlockSize = 8 * 1024;

	private readonly UlearnDb db;
	private readonly ICourseStorage courseStorage = WebCourseManager.CourseStorageInstance;
	private readonly MetricSender metricSender;

	private readonly IUserQuizzesRepo userQuizzesRepo;
	private readonly IVisitsRepo visitsRepo;
	private readonly IGroupsRepo groupsRepo;
	private readonly IGroupAccessesRepo groupAccessesRepo;
	private readonly ISlideCheckingsRepo slideCheckingsRepo;
	private readonly INotificationsRepo notificationsRepo;
	private readonly ICourseRolesRepo courseRolesRepo;
	private readonly IUnitsRepo unitsRepo;

	private readonly string baseUrlWeb;
	private readonly string baseUrlApi;

	public QuizController(IUserQuizzesRepo userQuizzesRepo, IVisitsRepo visitsRepo, IGroupsRepo groupsRepo, ISlideCheckingsRepo slideCheckingsRepo, INotificationsRepo notificationsRepo, IUnitsRepo unitsRepo, IGroupAccessesRepo groupAccessesRepo, UlearnDb db, ICourseRolesRepo courseRolesRepo)
	{
		this.userQuizzesRepo = userQuizzesRepo;
		this.visitsRepo = visitsRepo;
		this.groupsRepo = groupsRepo;
		this.slideCheckingsRepo = slideCheckingsRepo;
		this.notificationsRepo = notificationsRepo;
		this.unitsRepo = unitsRepo;
		this.groupAccessesRepo = groupAccessesRepo;
		this.db = db;
		this.courseRolesRepo = courseRolesRepo;
		var configuration = ApplicationConfiguration.Read<UlearnConfiguration>();
		baseUrlWeb = configuration.BaseUrl;
		baseUrlApi = configuration.BaseUrlApi;
		metricSender = new MetricSender(configuration.GraphiteServiceName);
	}

	[UsedImplicitly]
	private class QuizAnswer
	{
		public readonly string BlockType;
		public readonly string BlockId;
		public readonly string ItemId;
		public readonly string Text;

		public QuizAnswer(string type, string blockId, string itemId, string text)
		{
			BlockType = type;
			BlockId = blockId;
			ItemId = itemId;
			Text = text;
		}
	}

	private class QuizInfoForDb
	{
		public Type BlockType;
		public string BlockId;
		public string ItemId;
		public string Text;
		public bool IsRightAnswer;
		public int QuizBlockScore;
		public int QuizBlockMaxScore;
	}

	public bool CanUserFillQuiz(QuizStatus status)
	{
		return status is QuizStatus.ReadyToSend or QuizStatus.WaitsForManualChecking;
	}

	[AllowAnonymous]
	public async Task<IActionResult> Quiz(Guid slideId, string courseId, bool isGuest, bool isLti = false, int? checkQueueItemId = null, int? send = null, bool attempt = false)
	{
		metricSender.SendCount("quiz.show");
		if (isLti)
			metricSender.SendCount("quiz.show.lti");
		metricSender.SendCount($"quiz.show.{courseId}");
		metricSender.SendCount($"quiz.show.{courseId}.{slideId.ToString()}");

		var course = courseStorage.FindCourse(courseId);
		if (course == null)
			return new NotFoundResult();

		var userId = User.GetUserId();
		var visibleUnitIds = (await unitsRepo.GetVisibleUnitIds(course, userId)).ToList();
		var isCourseAdmin = !isGuest && await courseRolesRepo.HasUserAccessToCourse(userId, courseId, CourseRoleType.CourseAdmin);
		var isInstructor = isCourseAdmin || !isGuest && await courseRolesRepo.HasUserAccessToCourse(userId, courseId, CourseRoleType.Instructor);

		var slide = course.FindSlideById(slideId, isInstructor, visibleUnitIds) as QuizSlide;
		
		ManualQuizChecking manualQuizCheckQueueItem = null;
		var isManualCheckingReadonly = false;
		if (isInstructor && checkQueueItemId.HasValue)
			manualQuizCheckQueueItem = await slideCheckingsRepo.FindManualCheckingById<ManualQuizChecking>(checkQueueItemId.Value);
		
		if (isGuest)
		{
			metricSender.SendCount("quiz.show.to.guest");
			return PartialView(GuestQuiz(course, slide));
		}

		var isManualCheckingEnabledForUser = slide.ManualChecking && await groupsRepo.IsManualCheckingEnabledForUserAsync(course, userId);
		var maxAttemptsCount = await GetMaxAttemptsCount(courseId, slide);
		var userScores = await GetUserScoresForBlocks(courseId, userId, slideId, manualQuizCheckQueueItem?.Submission);

		var score = userScores?.AsEnumerable().Sum(res => res.Value) ?? 0;

		var state = await GetQuizState(courseId, userId, slideId, score, slide.MaxScore, manualQuizCheckQueueItem?.Submission);

		log.Info($"Показываю тест для пользователя {userId} в слайде {courseId}:{slide.Id}, isLti = {isLti}");

		/* If it's manual checking, change quiz state to IsChecking for correct rendering */
		if (manualQuizCheckQueueItem != null)
			state.Status = QuizStatus.IsCheckingByInstructor;

		/* For manually checked quizzes show last attempt's answers until ?attempt=true is defined in query string */
		if (slide.ManualChecking && manualQuizCheckQueueItem == null && state.Status == QuizStatus.ReadyToSend && state.UsedAttemptsCount > 0 && !attempt)
			state.Status = QuizStatus.Sent;

		/* We also want to show user's answer if user sent answers just now */
		if (state.Status == QuizStatus.ReadyToSend && send.HasValue)
			state.Status = QuizStatus.Sent;

		var userAnswers = await userQuizzesRepo.GetAnswersForShowingOnSlideAsync(courseId, slide, userId, manualQuizCheckQueueItem?.Submission);
		var canUserFillQuiz = (!slide.ManualChecking || isManualCheckingEnabledForUser) && CanUserFillQuiz(state.Status);

		var questionAnswersFrequency = new DefaultDictionary<string, DefaultDictionary<string, int>>();
		if (isCourseAdmin)
		{
			var choiceBlocks = slide.Blocks.OfType<ChoiceBlock>().ToList();
			var answersFrequency = await userQuizzesRepo.GetAnswersFrequencyForChoiceBlocks(courseId, slide.Id, choiceBlocks);
			questionAnswersFrequency = answersFrequency.Keys.ToDictionary(
				blockId => blockId,
				blockId => answersFrequency[blockId].ToDefaultDictionary()
			).ToDefaultDictionary();
		}

		var model = new QuizModel
		{
			Course = course,
			Slide = slide,
			BaseUrlWeb = baseUrlWeb,
			BaseUrlApi = baseUrlApi,
			QuizState = state,
			MaxAttemptsCount = maxAttemptsCount,
			UserScores = userScores,
			AnswersToQuizzes = userAnswers,
			IsLti = isLti,
			Checking = manualQuizCheckQueueItem,
			ManualCheckingsLeftInQueue = manualQuizCheckQueueItem != null ? await GetManualCheckingsCountInQueue(course, slide) : 0,
			CanUserFillQuiz = canUserFillQuiz,
			GroupsIds = Request.GetMultipleValuesFromQueryString("group"),
			QuestionAnswersFrequency = questionAnswersFrequency,
			IsManualCheckingEnabledForUser = isManualCheckingEnabledForUser, 
			IsInstructor = isInstructor,
			IsCourseAdmin = isCourseAdmin, 
			IsGuest = isGuest
		};

		return PartialView(model);
	}

	private async Task<int> GetManualCheckingsCountInQueue(ICourse course, QuizSlide slide)
	{
		var groupsIds = Request.GetMultipleValuesFromQueryString("group");
		return await ControllerUtils.GetManualCheckingsCountInQueue(slideCheckingsRepo, groupsRepo, groupAccessesRepo, User, course.Id, slide, groupsIds);
	}

	[HttpPost]
	public async Task<IActionResult> SubmitQuiz([FromForm] SubmitQuizRequest request)
	{
		var isLti = request.IsLti;
		var courseId = request.CourseId;
		var slideId = request.SlideId;
		var answer = request.Answer;

		metricSender.SendCount("quiz.submit");
		if (isLti)
			metricSender.SendCount("quiz.submit.lti");
		metricSender.SendCount($"quiz.submit.{courseId}");
		metricSender.SendCount($"quiz.submit.{courseId}.{slideId}");

		var course = courseStorage.GetCourse(courseId);
		var isInstructor = User.HasAccessFor(courseId, CourseRoleType.Instructor);
		var userId = User.GetUserId();
		var visibleUnits = await unitsRepo.GetVisibleUnitIds(course, userId);
		if (course.FindSlideById(slideId, isInstructor, visibleUnits) is not QuizSlide slide)
			return NotFound();

		var maxTriesCount = await GetMaxAttemptsCount(courseId, slide);

		/* Not it's not important what user's score is, so just pass 0 */
		var state = await GetQuizState(courseId, userId, slideId, 0, slide.MaxScore);
		if (!CanUserFillQuiz(state.Status))
			return Ok("Already answered");

		var attemptNumber = state.UsedAttemptsCount;
		metricSender.SendCount($"quiz.submit.try.{attemptNumber}");
		metricSender.SendCount($"quiz.submit.{courseId}.try.{attemptNumber}");
		metricSender.SendCount($"quiz.submit.{courseId}.{slideId}.try.{attemptNumber}");

		if (slide.ManualChecking && !await groupsRepo.IsManualCheckingEnabledForUserAsync(course, userId))
			return Ok("Manual checking is disabled for you");

		var answers = JsonConvert.DeserializeObject<List<QuizAnswer>>(answer).GroupBy(x => x.BlockId);
		var quizBlockWithTaskCount = slide.Blocks.Count(x => x is AbstractQuestionBlock);
		var allQuizInfos = new List<QuizInfoForDb>();
		foreach (var ans in answers)
		{
			var quizInfos = CreateQuizInfo(slide, ans);
			if (quizInfos != null)
				allQuizInfos.AddRange(quizInfos);
		}

		var blocksInAnswerCount = allQuizInfos.Select(x => x.BlockId).Distinct().Count();
		if (blocksInAnswerCount != quizBlockWithTaskCount)
			return Forbid("Has empty blocks");

		UserQuizSubmission submission;
		await using (var transaction = await db.Database.BeginTransactionAsync())
		{
			submission = await userQuizzesRepo.AddSubmissionAsync(courseId, slideId, userId, DateTime.Now).ConfigureAwait(false);

			foreach (var quizInfoForDb in allQuizInfos)
				await userQuizzesRepo.AddUserQuizAnswerAsync(
					submission.Id,
					quizInfoForDb.IsRightAnswer,
					quizInfoForDb.BlockId,
					quizInfoForDb.ItemId,
					quizInfoForDb.Text,
					quizInfoForDb.QuizBlockScore,
					quizInfoForDb.QuizBlockMaxScore
				).ConfigureAwait(false);

			await transaction.CommitAsync();
		}

		if (slide.ManualChecking)
		{
			/* If this quiz is already queued for checking for this user, remove waiting checkings */
			if (state.Status == QuizStatus.WaitsForManualChecking)
				await slideCheckingsRepo.RemoveWaitingManualCheckings<ManualQuizChecking>(courseId, slideId, userId).ConfigureAwait(false);

			await slideCheckingsRepo.AddManualQuizChecking(submission, courseId, slideId, userId).ConfigureAwait(false);
			await visitsRepo.MarkVisitsAsWithManualChecking(courseId, slideId, userId).ConfigureAwait(false);
		}
		/* Recalculate score for quiz if this attempt is allowed. Don't recalculate score if this attempt number is more then maxTriesCount */
		else if (attemptNumber < maxTriesCount)
		{
			var score = allQuizInfos
				.Deprecated_DistinctBy(forDb => forDb.BlockId)
				.Sum(forDb => forDb.QuizBlockScore);

			metricSender.SendCount($"quiz.submit.try.{attemptNumber}.score", score);
			metricSender.SendCount($"quiz.submit.{courseId}.try.{attemptNumber}.score", score);
			metricSender.SendCount($"quiz.submit.{courseId}.{slideId}.try.{attemptNumber}.score", score);
			metricSender.SendCount($"quiz.submit.score", score);
			metricSender.SendCount($"quiz.submit.{courseId}.score", score);
			metricSender.SendCount($"quiz.submit.{courseId}.{slideId}.score", score);

			if (score == slide.MaxScore)
			{
				metricSender.SendCount($"quiz.submit.try.{attemptNumber}.full_passed");
				metricSender.SendCount($"quiz.submit.{courseId}.try.{attemptNumber}.full_passed");
				metricSender.SendCount($"quiz.submit.{courseId}.{slideId}.try.{attemptNumber}.full_passed");
				metricSender.SendCount($"quiz.submit.full_passed");
				metricSender.SendCount($"quiz.submit.{courseId}.full_passed");
				metricSender.SendCount($"quiz.submit.{courseId}.{slideId}.full_passed");
			}

			await slideCheckingsRepo.AddAutomaticQuizChecking(submission, courseId, slideId, userId, score).ConfigureAwait(false);
			await visitsRepo.UpdateScoreForVisit(courseId, slide, userId).ConfigureAwait(false);
			if (isLti)
				LtiUtils.SubmitScore(courseId, slide, userId);
		}

		return Json(new
		{
			url = isLti
				? Url.Action("LtiSlide", "Course", new { courseId = courseId, slideId = slide.Id, send = 1 })
				: Url.RouteUrl("Course.SlideById", new { courseId = courseId, slideId = slide.Url, send = 1 })
		});
	}

	private async Task NotifyAboutManualQuizChecking(ManualQuizChecking checking)
	{
		var notification = new PassedManualQuizCheckingNotification
		{
			Checking = checking,
		};
		await notificationsRepo.AddNotification(checking.CourseId, notification, User.GetUserId()).ConfigureAwait(false);
	}

	[HttpPost]
	[Authorize(Policy = UlearnAuthorizationBuilder.InstructorsPolicyName)] //[ULearnAuthorize(MinAccessLevel = CourseRoleType.Instructor)]
	public async Task<IActionResult> ScoreQuiz(int id, string nextUrl, string errorUrl = "")
	{
		metricSender.SendCount("quiz.manual_score");

		if (string.IsNullOrEmpty(errorUrl))
			errorUrl = nextUrl;

		var checking = await slideCheckingsRepo.FindManualCheckingById<ManualQuizChecking>(id);
		if (!await groupAccessesRepo.CanInstructorViewStudent(User.GetUserId(), checking.UserId, checking.CourseId))
			return Forbid();

		using (var transaction = db.Database.BeginTransaction())
		{
			var course = courseStorage.GetCourse(checking.CourseId);
			var unit = course.FindUnitBySlideIdNotSafe(checking.SlideId, true);
			var slide = course.GetSlideByIdNotSafe(checking.SlideId);

			metricSender.SendCount($"quiz.manual_score.{checking.CourseId}");
			metricSender.SendCount($"quiz.manual_score.{checking.CourseId}.{checking.SlideId}");

			var totalScore = 0;

			var quiz = course.FindSlideByIdNotSafe(checking.SlideId);
			if (quiz == null)
				return Redirect(errorUrl + "Этого теста больше нет в курсе");

			foreach (var question in quiz.Blocks.OfType<AbstractQuestionBlock>())
			{
				var scoreFieldName = "quiz__score__" + question.Id;
				var scoreStr = Request.Form[scoreFieldName];
				/* Invalid form: score isn't integer */
				if (!int.TryParse(scoreStr, out var score))
					return Redirect(errorUrl + $"Неверное количество баллов в задании «{question.QuestionIndex}. {question.Text.TruncateWithEllipsis(50)}»");
				/* Invalid form: score isn't from range 0..MAX_SCORE */
				if (score < 0 || score > question.MaxScore)
					return Redirect(errorUrl + $"Неверное количество баллов в задании «{question.QuestionIndex}. {question.Text.TruncateWithEllipsis(50)}»: {score}");

				await userQuizzesRepo.SetScoreForQuizBlock(checking.Submission.Id, question.Id, score).ConfigureAwait(false);
				totalScore += score;
			}

			await slideCheckingsRepo.MarkManualQuizCheckingAsChecked(checking, totalScore).ConfigureAwait(false);

			await visitsRepo.UpdateScoreForVisit(checking.CourseId, slide, checking.UserId).ConfigureAwait(false);

			metricSender.SendCount($"quiz.manual_score.score", totalScore);
			metricSender.SendCount($"quiz.manual_score.{checking.CourseId}.score", totalScore);
			metricSender.SendCount($"quiz.manual_score.{checking.CourseId}.{checking.SlideId}.score", totalScore);
			if (totalScore == quiz.MaxScore)
			{
				metricSender.SendCount($"quiz.manual_score.full_scored");
				metricSender.SendCount($"quiz.manual_score.{checking.CourseId}.full_scored");
				metricSender.SendCount($"quiz.manual_score.{checking.CourseId}.{checking.SlideId}.full_scored");
			}

			if (unit != null && await unitsRepo.IsUnitVisibleForStudents(course, unit.Id))
				await NotifyAboutManualQuizChecking(checking).ConfigureAwait(false);

			transaction.Commit();
		}

		return Redirect(nextUrl);
	}

	private IEnumerable<QuizInfoForDb> CreateQuizInfo(QuizSlide slide, IGrouping<string, QuizAnswer> answer)
	{
		var block = slide.FindBlockById(answer.Key);
		return block switch
		{
			FillInBlock inBlock => CreateQuizInfoForDb(inBlock, answer.First().Text),
			ChoiceBlock choiceBlock => CreateQuizInfoForDb(choiceBlock, answer),
			OrderingBlock orderingBlock => CreateQuizInfoForDb(orderingBlock, answer),
			MatchingBlock matchingBlock => CreateQuizInfoForDb(matchingBlock, answer),
			IsTrueBlock trueBlock => CreateQuizInfoForDb(trueBlock, answer),
			_ => null
		};
	}

	private IEnumerable<QuizInfoForDb> CreateQuizInfoForDb(IsTrueBlock isTrueBlock, IGrouping<string, QuizAnswer> data)
	{
		// Здесь двойной баг. В запросе из браузера текст преедается в ItemId, а не Text, поэтому работает. В базу правильно: ItemId всегда null, а значение в Text
		var isTrue = isTrueBlock.IsRight(data.First().ItemId);
		var blockScore = isTrue ? isTrueBlock.MaxScore : 0;
		return new List<QuizInfoForDb>
		{
			new QuizInfoForDb
			{
				BlockId = isTrueBlock.Id,
				ItemId = null,
				IsRightAnswer = isTrue,
				Text = data.First().ItemId,
				BlockType = typeof(IsTrueBlock),
				QuizBlockScore = blockScore,
				QuizBlockMaxScore = isTrueBlock.MaxScore
			}
		};
	}

	private IEnumerable<QuizInfoForDb> CreateQuizInfoForDb(ChoiceBlock choiceBlock, IGrouping<string, QuizAnswer> answers)
	{
		int blockScore;
		if (!choiceBlock.Multiple)
		{
			var answerItemId = answers.First().ItemId;
			var isCorrect = choiceBlock.Items.First(x => x.Id == answerItemId).IsCorrect.IsTrueOrMaybe();
			blockScore = isCorrect ? choiceBlock.MaxScore : 0;
			return new List<QuizInfoForDb>
			{
				new QuizInfoForDb
				{
					BlockId = choiceBlock.Id,
					ItemId = answerItemId,
					IsRightAnswer = isCorrect,
					Text = null,
					BlockType = typeof(ChoiceBlock),
					QuizBlockScore = blockScore,
					QuizBlockMaxScore = choiceBlock.MaxScore
				}
			};
		}

		var ans = answers.Select(x => x.ItemId).ToList()
			.Select(x => new QuizInfoForDb
			{
				BlockId = choiceBlock.Id,
				IsRightAnswer = choiceBlock.Items.Where(y => y.IsCorrect.IsTrueOrMaybe()).Any(y => y.Id == x),
				ItemId = x,
				Text = null,
				BlockType = typeof(ChoiceBlock),
				QuizBlockScore = 0,
				QuizBlockMaxScore = choiceBlock.MaxScore
			}).ToList();

		var mistakesCount = GetChoiceBlockMistakesCount(choiceBlock, ans);
		var isRightQuizBlock = mistakesCount.HasNotMoreThatAllowed(choiceBlock.AllowedMistakesCount);

		blockScore = isRightQuizBlock ? choiceBlock.MaxScore : 0;
		foreach (var info in ans)
			info.QuizBlockScore = blockScore;
		return ans;
	}

	private MistakesCount GetChoiceBlockMistakesCount(ChoiceBlock choiceBlock, List<QuizInfoForDb> ans)
	{
		var checkedUnnecessary = ans.Count(x => !x.IsRightAnswer);

		var totallyTrueItemIds = choiceBlock.Items.Where(x => x.IsCorrect == ChoiceItemCorrectness.True).Select(x => x.Id);
		var userItemIds = ans.Select(y => y.ItemId).ToHashSet();
		var notCheckedNecessary = totallyTrueItemIds.Count(x => !userItemIds.Contains(x));

		return new MistakesCount(checkedUnnecessary, notCheckedNecessary);
	}

	private IEnumerable<QuizInfoForDb> CreateQuizInfoForDb(OrderingBlock orderingBlock, IGrouping<string, QuizAnswer> answers)
	{
		var ans = answers.Select(x => x.ItemId).ToList()
			.Select(x => new QuizInfoForDb
			{
				BlockId = orderingBlock.Id,
				IsRightAnswer = true,
				ItemId = x,
				Text = null,
				BlockType = typeof(OrderingBlock),
				QuizBlockScore = 0,
				QuizBlockMaxScore = orderingBlock.MaxScore
			}).ToList();

		var isRightQuizBlock = answers.Count() == orderingBlock.Items.Length &&
								answers.Zip(orderingBlock.Items, (answer, item) => answer.ItemId == item.GetHash()).All(x => x);
		var blockScore = isRightQuizBlock ? orderingBlock.MaxScore : 0;
		foreach (var info in ans)
			info.QuizBlockScore = blockScore;

		return ans;
	}

	private IEnumerable<QuizInfoForDb> CreateQuizInfoForDb(MatchingBlock matchingBlock, IGrouping<string, QuizAnswer> answers)
	{
		var ans = answers.ToList()
			.Select(x => new QuizInfoForDb
			{
				BlockId = matchingBlock.Id,
				IsRightAnswer = matchingBlock.Matches.FirstOrDefault(m => m.GetHashForFixedItem() == x.ItemId)?.GetHashForMovableItem() == x.Text,
				ItemId = x.ItemId,
				Text = x.Text,
				BlockType = typeof(MatchingBlock),
				QuizBlockScore = 0,
				QuizBlockMaxScore = matchingBlock.MaxScore
			}).ToList();

		var isRightQuizBlock = ans.All(x => x.IsRightAnswer);
		var blockScore = isRightQuizBlock ? matchingBlock.MaxScore : 0;
		foreach (var info in ans)
			info.QuizBlockScore = blockScore;

		return ans;
	}

	private IEnumerable<QuizInfoForDb> CreateQuizInfoForDb(FillInBlock fillInBlock, string data)
	{
		if (data.Length > MaxFillInBlockSize)
			data = data.Substring(0, MaxFillInBlockSize);
		var isRightAnswer = false;
		if (fillInBlock.Regexes != null)
			isRightAnswer = fillInBlock.Regexes.Any(regex => regex.Regex.IsMatch(data));
		var blockScore = isRightAnswer ? fillInBlock.MaxScore : 0;
		return new List<QuizInfoForDb>
		{
			new QuizInfoForDb
			{
				BlockId = fillInBlock.Id,
				ItemId = null,
				IsRightAnswer = isRightAnswer,
				Text = data,
				BlockType = typeof(FillInBlock),
				QuizBlockScore = blockScore,
				QuizBlockMaxScore = fillInBlock.MaxScore
			}
		};
	}

	[HttpPost]
	// Вызывается только для квизов без автопроверки
	public async Task<IActionResult> RestartQuiz(string courseId, Guid slideId, bool isLti)
	{
		var isInstructor = User.HasAccessFor(courseId, CourseRoleType.Instructor);
		var course = courseStorage.GetCourse(courseId);
		var visibleUnits = await unitsRepo.GetVisibleUnitIds(course, User.GetUserId());
		var slide = course.GetSlideById(slideId, isInstructor, visibleUnits);
		if (slide is QuizSlide)
		{
			var userId = User.GetUserId();
			var usedAttemptsCount = userQuizzesRepo.GetUsedAttemptsCountForQuizWithAutomaticChecking(courseId, userId, slideId);
			var maxTriesCount = await GetMaxAttemptsCount(courseId, slide as QuizSlide);
			var isQuizScoredMaximum = await userQuizzesRepo.IsQuizScoredMaximumAsync(courseId, slideId, userId);
			if (usedAttemptsCount < maxTriesCount && !isQuizScoredMaximum)
			{
				await visitsRepo.UpdateScoreForVisit(courseId, slide, userId).ConfigureAwait(false);
				if (isLti)
					LtiUtils.SubmitScore(courseId, slide, userId);
			}
		}

		var model = new { courseId, slideId = slide.Id, isLti, attempt = true };
		return RedirectToAction(isLti
				? "LtiSlide"
				: "SlideById",
			"Course",
			model);
	}

	private QuizModel GuestQuiz(Course course, QuizSlide slide)
	{
		return new QuizModel
		{
			Course = course,
			Slide = slide,
			BaseUrlWeb = baseUrlWeb,
			BaseUrlApi = baseUrlApi,
			IsGuest = true,
			QuizState = new QuizState(QuizStatus.ReadyToSend, 0, 0, slide.MaxScore),
		};
	}

	private async Task<int> GetMaxAttemptsCount(string courseId, QuizSlide quizSlide)
	{
		var isTester = await courseRolesRepo.HasUserAccessToCourse(User.GetUserId(), courseId, CourseRoleType.Tester);
		if (isTester)
			return InfinityTriesCount;

		if (quizSlide == null)
			return defaultMaxTriesCount;

		return quizSlide.MaxTriesCount;
	}

	private async Task<Dictionary<string, int>> GetUserScoresForBlocks(string courseId, string userId, Guid slideId, UserQuizSubmission submission)
	{
		return await userQuizzesRepo.GetUserScoresAsync(courseId, slideId, userId, submission);
	}

	private async Task<QuizState> GetQuizState(string courseId, string userId, Guid slideId, int userScore, int maxScore, UserQuizSubmission submission = null)
	{
		log.Info($"Ищу статус прохождения теста {courseId}:{slideId} для пользователя {userId}");

		var lastSubmission = await userQuizzesRepo.FindLastUserSubmissionAsync(courseId, slideId, userId);

		var manualChecking = submission?.ManualChecking ?? lastSubmission?.ManualChecking;
		if (manualChecking != null)
		{
			/* For manually checked quizzes attempts are counting by manual checkings, not by user quiz submissions
				(because user can resend quiz before instructor checked and score it) */
			var manualCheckingCount = await slideCheckingsRepo.GetQuizManualCheckingCount(courseId, slideId, userId, submission?.Timestamp);

			log.Info($"Статус прохождения теста {courseId}:{slideId} для пользователя {userId}: есть ручная проверка №{manualChecking.Id}, проверяется ли сейчас: {manualChecking.IsLocked}");
			if (manualChecking.IsChecked)
				return new QuizState(QuizStatus.ReadyToSend, manualCheckingCount, userScore, maxScore);
			return new QuizState(manualChecking.IsLocked ? QuizStatus.IsCheckingByInstructor : QuizStatus.WaitsForManualChecking, manualCheckingCount, userScore, maxScore);
		}

		var usedAttemptsCount = userQuizzesRepo.GetUsedAttemptsCountForQuizWithAutomaticChecking(courseId, userId, slideId);
		return new QuizState(QuizStatus.ReadyToSend, usedAttemptsCount, userScore, maxScore);
	}

	public class SubmitQuizRequest
	{
		public string CourseId { get; set; }
		public Guid SlideId { get; set; }
		public string Answer { get; set; }
		public bool IsLti { get; set; }
	}
}