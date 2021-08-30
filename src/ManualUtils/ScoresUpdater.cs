using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Models.Quizzes;
using Database.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Core.Courses.Slides.Quizzes.Blocks;

namespace ManualUtils
{
	public static class ScoresUpdater
	{
		private const int MaxFillInBlockSize = 8 * 1024;

		public static async Task UpdateVisitsForExercises(IServiceProvider serviceProvider, DateTime from)
		{
			var db = serviceProvider.GetService<UlearnDb>();
			var courseStorage = serviceProvider.GetService<ICourseStorage>();
			var visitsRepo = serviceProvider.GetService<IVisitsRepo>();

			var keys = (await db.UserExerciseSubmissions
				.Where(s => s.AutomaticCheckingIsRightAnswer && (s.Timestamp > from || (s.ManualChecking != null && s.ManualChecking.Timestamp > from)))
				.Select(s => new { s.CourseId, s.SlideId, s.UserId })
				.Distinct()
				.OrderBy(s => s.CourseId)
				.ThenBy(s => s.SlideId)
				.ThenBy(s => s.UserId)
				.ToListAsync())
				.Select(s => (s.CourseId, s.SlideId, s.UserId))
				.ToList();

			Console.WriteLine($"records to update count: {keys.Count}");

			var i = 0;
			foreach (var (courseId, slideId, userId) in keys)
			{
				var course = courseStorage.GetCourse(courseId);
				var slide = course.FindSlideByIdNotSafe(slideId);
				if (slide == null)
					continue;
				await visitsRepo.UpdateScoreForVisit(courseId, slide, userId);
				i++;
				if (i % 10 == 0)
					Console.WriteLine($"{i}/{keys.Count}"); 
			}
		}

		// Обновляет пройденные тесты с нулевым баллом
		public static async Task UpdateTests(IServiceProvider serviceProvider, string courseId)
		{
			var db = serviceProvider.GetService<UlearnDb>();
			var courseStorage = serviceProvider.GetService<ICourseStorage>();
			var visitsRepo = serviceProvider.GetService<IVisitsRepo>();

			var course = courseStorage.GetCourse(courseId);
			var tests = course.GetSlidesNotSafe().OfType<QuizSlide>().ToList();
			foreach (var test in tests)
			{
				var testVisits = db.Visits.Where(v => v.CourseId == courseId && v.SlideId == test.Id && v.IsPassed && v.Score == 0).ToList();
				foreach (var visit in testVisits)
				{
					var answers = db.UserQuizAnswers.Where(s => s.Submission.CourseId == courseId && s.Submission.UserId == visit.UserId && s.Submission.SlideId == visit.SlideId).ToList();
					var groupsBySubmission = answers.GroupBy(a => a.Submission.Id).ToList();

					foreach (var group in groupsBySubmission)
					{
						var submissionAnswers = group.ToList();
						var blockAnswers = submissionAnswers.GroupBy(s => s.BlockId);

						var allQuizInfos = new List<QuizInfoForDb>();
						foreach (var ans in blockAnswers)
						{
							var quizInfos = CreateQuizInfo(test, ans);
							if (quizInfos != null)
								allQuizInfos.AddRange(quizInfos);
						}

						foreach (var answer in submissionAnswers)
						{
							var valid = allQuizInfos.First(i => i.BlockId == answer.BlockId && i.ItemId == answer.ItemId);
							answer.QuizBlockScore = valid.QuizBlockScore;
							answer.QuizBlockMaxScore = valid.QuizBlockMaxScore;
						}

						var score = allQuizInfos
							.DistinctBy(forDb => forDb.BlockId)
							.Sum(forDb => forDb.QuizBlockScore);

						var checking = db.AutomaticQuizCheckings.FirstOrDefault(c => c.Submission.Id == group.Key);
						if (checking != null) // В случае попытки сверх лимита AutomaticQuizCheckings не создается, но ответы сохраняются в UserQuizAnswers
							checking.Score = score;

						db.SaveChanges();
					}

					await visitsRepo.UpdateScoreForVisit(courseId, test, visit.UserId);
				}

				Console.WriteLine(test.Id);
			}
		}

		private static IEnumerable<QuizInfoForDb> CreateQuizInfo(QuizSlide slide, IGrouping<string, UserQuizAnswer> answer)
		{
			var block = slide.FindBlockById(answer.Key);
			if (block is FillInBlock)
				return CreateQuizInfoForDb(block as FillInBlock, answer.First().Text);
			if (block is ChoiceBlock)
				return CreateQuizInfoForDb(block as ChoiceBlock, answer);
			if (block is OrderingBlock)
				return CreateQuizInfoForDb(block as OrderingBlock, answer);
			if (block is MatchingBlock)
				return CreateQuizInfoForDb(block as MatchingBlock, answer);
			if (block is IsTrueBlock)
				return CreateQuizInfoForDb(block as IsTrueBlock, answer);
			return null;
		}

		private static IEnumerable<QuizInfoForDb> CreateQuizInfoForDb(IsTrueBlock isTrueBlock, IGrouping<string, UserQuizAnswer> data)
		{
			var isTrue = isTrueBlock.IsRight(data.First().Text);
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

		private static IEnumerable<QuizInfoForDb> CreateQuizInfoForDb(ChoiceBlock choiceBlock, IGrouping<string, UserQuizAnswer> answers)
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

		private static MistakesCount GetChoiceBlockMistakesCount(ChoiceBlock choiceBlock, List<QuizInfoForDb> ans)
		{
			var checkedUnnecessary = ans.Count(x => !x.IsRightAnswer);

			var totallyTrueItemIds = choiceBlock.Items.Where(x => x.IsCorrect == ChoiceItemCorrectness.True).Select(x => x.Id);
			var userItemIds = ans.Select(y => y.ItemId).ToHashSet();
			var notCheckedNecessary = totallyTrueItemIds.Count(x => !userItemIds.Contains(x));

			return new MistakesCount(checkedUnnecessary, notCheckedNecessary);
		}

		private static IEnumerable<QuizInfoForDb> CreateQuizInfoForDb(OrderingBlock orderingBlock, IGrouping<string, UserQuizAnswer> answers)
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

		private static IEnumerable<QuizInfoForDb> CreateQuizInfoForDb(MatchingBlock matchingBlock, IGrouping<string, UserQuizAnswer> answers)
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

		private static IEnumerable<QuizInfoForDb> CreateQuizInfoForDb(FillInBlock fillInBlock, string data)
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
	}
}