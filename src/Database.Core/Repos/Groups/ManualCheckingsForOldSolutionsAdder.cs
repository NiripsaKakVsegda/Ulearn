using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ulearn.Core.Courses.Manager;
using Vostok.Logging.Abstractions;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Quizzes;

namespace Database.Repos.Groups
{
	public class ManualCheckingsForOldSolutionsAdder : IManualCheckingsForOldSolutionsAdder
	{
		private readonly IUserSolutionsRepo userSolutionsRepo;
		private readonly ISlideCheckingsRepo slideCheckingsRepo;
		private readonly IVisitsRepo visitsRepo;
		private readonly IUserQuizzesRepo userQuizzesRepo;
		private readonly IUnitsRepo unitsRepo;
		private readonly ICourseStorage courseStorage;
		private static ILog log => LogProvider.Get().ForContext(typeof(ManualCheckingsForOldSolutionsAdder));

		public ManualCheckingsForOldSolutionsAdder(
			IUserSolutionsRepo userSolutionsRepo, ISlideCheckingsRepo slideCheckingsRepo, IVisitsRepo visitsRepo, IUserQuizzesRepo userQuizzesRepo,
			IUnitsRepo unitsRepo, ICourseStorage courseStorage)
		{
			this.userSolutionsRepo = userSolutionsRepo;
			this.slideCheckingsRepo = slideCheckingsRepo;
			this.visitsRepo = visitsRepo;
			this.userQuizzesRepo = userQuizzesRepo;
			this.unitsRepo = unitsRepo;
			this.courseStorage = courseStorage;
		}

		public async Task AddManualCheckingsForOldSolutionsAsync(string courseId, IEnumerable<string> usersIds)
		{
			foreach (var userId in usersIds)
				await AddManualCheckingsForOldSolutionsAsync(courseId, userId).ConfigureAwait(false);
		}

		public async Task AddManualCheckingsForOldSolutionsAsync(string courseId, string userId)
		{
			log.Info($"Создаю ручные проверки для всех решения пользователя {userId} в курсе {courseId}");

			var course = courseStorage.GetCourse(courseId);
			var visibleUnitsIds = await unitsRepo.GetVisibleUnitIds(course, userId);

			/* For exercises */
			var acceptedSubmissionsBySlide = userSolutionsRepo.GetAllAcceptedSubmissionsByUser(courseId, userId)
				.ToList()
				.GroupBy(s => s.SlideId)
				.ToDictionary(g => g.Key, g => g.ToList());
			foreach (var acceptedSubmissionsForSlide in acceptedSubmissionsBySlide.Values)
				/* If exists at least one manual checking for at least one submissions on slide, then ignore this slide */
				if (acceptedSubmissionsForSlide.All(s => s.ManualChecking == null))
				{
					/* Otherwise found the latest accepted submission */
					var lastSubmission = acceptedSubmissionsForSlide.OrderByDescending(s => s.Timestamp).First();

					var slideId = lastSubmission.SlideId;
					var slide = course.FindSlideById(slideId, false, visibleUnitsIds) as ExerciseSlide;
					if (slide == null || !slide.Scoring.RequireReview)
						continue;

					log.Info($"Создаю ручную проверку для решения {lastSubmission.Id}, слайд {slideId}");
					await slideCheckingsRepo.AddManualExerciseChecking(courseId, slideId, userId, lastSubmission.Id).ConfigureAwait(false);
					await visitsRepo.MarkVisitsAsWithManualChecking(courseId, slideId, userId).ConfigureAwait(false);
				}

			/* For quizzes */
			var passedQuizzesIds = await userQuizzesRepo.GetPassedSlideIdsAsync(courseId, userId).ConfigureAwait(false);
			foreach (var quizSlideId in passedQuizzesIds)
			{
				var slide = course.FindSlideById(quizSlideId, false, visibleUnitsIds) as QuizSlide;
				if (slide == null || !slide.ManualChecking)
					continue;
				if (!await userQuizzesRepo.IsWaitingForManualCheckAsync(courseId, quizSlideId, userId).ConfigureAwait(false))
				{
					log.Info($"Создаю ручную проверку для теста {slide.Id}");
					var submission = await userQuizzesRepo.FindLastUserSubmissionAsync(courseId, quizSlideId, userId).ConfigureAwait(false);
					if (submission == null || submission.ManualChecking != null)
						continue;
					await slideCheckingsRepo.AddManualQuizChecking(submission, courseId, quizSlideId, userId).ConfigureAwait(false);
					await visitsRepo.MarkVisitsAsWithManualChecking(courseId, quizSlideId, userId).ConfigureAwait(false);
				}
			}
		}
	}
}