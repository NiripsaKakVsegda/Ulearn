using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Common;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Flashcards;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Flashcards;
using Ulearn.Core.Courses.Units;
using Ulearn.Core.Markdown;
using Ulearn.Web.Api.Models.Responses.Flashcards;
using Web.Api.Configuration;

namespace Ulearn.Web.Api.Controllers.Flashcards
{
	[Route("/courses")]
	public class FlashcardsController : BaseFlashcardController
	{
		private readonly IUserGeneratedFlashcardsRepo userFlashcardsRepo;
		private readonly IUsersFlashcardsVisitsRepo usersFlashcardsVisitsRepo;
		private readonly IAdditionalContentPublicationsRepo additionalContentPublicationsRepo;
		private readonly IUnitsRepo unitsRepo;
		private readonly string baseUrlApi;
		private readonly string baseUrlWeb;


		public FlashcardsController(
			ICourseStorage courseStorage,
			UlearnDb db,
			IUsersRepo usersRepo,
			IUsersFlashcardsVisitsRepo usersFlashcardsVisitsRepo,
			IAdditionalContentPublicationsRepo additionalContentPublicationsRepo,
			ICourseRolesRepo courseRolesRepo,
			IUserGeneratedFlashcardsRepo userFlashcardsRepo,
			IUnitsRepo unitsRepo,
			IOptions<WebApiConfiguration> configuration,
			ICoursesRepo coursesRepo
		)
			: base(courseStorage, db, usersRepo, courseRolesRepo, coursesRepo, configuration)
		{
			this.userFlashcardsRepo = userFlashcardsRepo;
			this.usersFlashcardsVisitsRepo = usersFlashcardsVisitsRepo;
			this.additionalContentPublicationsRepo = additionalContentPublicationsRepo;
			this.unitsRepo = unitsRepo;
			baseUrlApi = configuration.Value.BaseUrlApi;
			baseUrlWeb = configuration.Value.BaseUrl;
		}

		/// <summary>
		/// Коллекция объектов флешкарт с оценками, сгруппированных по модулям по курсу
		/// </summary>
		/// <param name="course"></param>
		/// <returns></returns>
		[HttpGet("{courseId}/flashcards-by-units")]
		public async Task<ActionResult<FlashcardResponseByUnits>> Flashcards([FromRoute] Course course)
		{
			var isModerator = await CanUserModerateFlashcards(course.Id);

			var userGeneratedFlashcardsByUnits =
				(await userFlashcardsRepo.GetCourseFlashcardsRatableForUser(course.Id, UserId))
				.GroupBy(f => f.UnitId)
				.ToDictionary(
					f => f.Key,
					f => f.ToList()
				);

			var flashcardsVisits =
				(await usersFlashcardsVisitsRepo.GetLastUserFlashcardsVisitsAsync(UserId, course.Id))
				.ToDictionary(f => f.FlashcardId);

			var flashcardResponseByUnits = new FlashcardResponseByUnits();
			var visibleUnits = await unitsRepo.GetVisibleUnitIds(course, UserId);
			var isTester = await courseRolesRepo.HasUserAccessToCourse(UserId, course.Id, CourseRoleType.Tester);
			var publications = isTester
				? new List<AdditionalContentPublication>()
				: await additionalContentPublicationsRepo.GetAdditionalContentPublicationsForUser(course.Id, UserId);

			foreach (var unit in course.GetUnits(visibleUnits))
			{
				if (!unit.ContainsFlashcards)
					continue;

				if (unit.Settings.IsExtraContent && !isTester && publications.All(p => p.UnitId != unit.Id))
					continue;

				var unitFlashcardsResponse = new UnitFlashcardsResponse();
				var unitFlashcards = unit.Flashcards;

				var userFlashcards = userGeneratedFlashcardsByUnits.GetOrDefault(unit.Id, null);
				var lastRateIndexes = BuildFlashcardsLastRateIndexes(
					unitFlashcards
						.Select(f => f.Id)
						.Concat(userFlashcards.EmptyIfNull().Select(f => f.Id.ToString())),
					flashcardsVisits
				);

				var flashcardResponsesEnumerable = GetFlashcardResponses(course.Id, unit, unitFlashcards, flashcardsVisits, lastRateIndexes);
				unitFlashcardsResponse.Flashcards.AddRange(flashcardResponsesEnumerable);

				if (userFlashcards is not null)
				{
					var userFlashcardResponsesEnumerable = GetFlashcardResponses(course.Id, unit.Id, userFlashcards, flashcardsVisits, lastRateIndexes, isModerator);
					unitFlashcardsResponse.Flashcards.AddRange(userFlashcardResponsesEnumerable);
				}

				unitFlashcardsResponse.UnitId = unit.Id;
				unitFlashcardsResponse.UnitTitle = unit.Title;
				unitFlashcardsResponse.Unlocked = unitFlashcardsResponse.Flashcards
					.All(f => f.Rate != Rate.NotRated);
				flashcardResponseByUnits.Units.Add(unitFlashcardsResponse);
			}

			return flashcardResponseByUnits;
		}

		private IEnumerable<CourseFlashcardResponse> GetFlashcardResponses(
			string courseId,
			Unit unit,
			IEnumerable<Flashcard> flashcards,
			IReadOnlyDictionary<string, UserFlashcardsVisit> visitsByFlashcardId,
			IReadOnlyDictionary<string, int> lastRateIndexes
		)
		{
			var markdownContext = new MarkdownRenderContext(baseUrlApi, baseUrlWeb, courseId, unit.UnitDirectoryRelativeToCourse);

			foreach (var flashcard in flashcards)
			{
				var question = GetRenderedContent(flashcard.Question.Blocks, markdownContext);
				var answer = GetRenderedContent(flashcard.Answer.Blocks, markdownContext);

				var rateResponse = visitsByFlashcardId.TryGetValue(flashcard.Id, out var visit)
					? visit.Rate
					: Rate.NotRated;

				var lastRateIndex = lastRateIndexes[flashcard.Id];

				yield return new CourseFlashcardResponse
				{
					Answer = answer,
					Question = question,
					Rate = rateResponse,
					Id = flashcard.Id,
					CourseId = courseId,
					UnitId = unit.Id,
					TheorySlidesIds = flashcard.TheorySlidesIds,
					LastRateIndex = lastRateIndex
				};
			}
		}

		private IEnumerable<UserGeneratedFlashcardResponse> GetFlashcardResponses(
			string courseId,
			Guid unitId,
			IEnumerable<UserGeneratedFlashcard> flashcards,
			IReadOnlyDictionary<string, UserFlashcardsVisit> visitsByFlashcardId,
			IReadOnlyDictionary<string, int> lastRateIndexes,
			bool isModerator
		)
		{
			foreach (var flashcard in flashcards)
			{
				var rateResponse = visitsByFlashcardId.TryGetValue(flashcard.Id.ToString(), out var visit)
					? visit.Rate
					: Rate.NotRated;

				var lastRateIndex = lastRateIndexes[flashcard.Id.ToString()];

				var flashcardResponse = new UserGeneratedFlashcardResponse
				{
					Answer = flashcard.Answer,
					Question = flashcard.Question,
					Rate = rateResponse,
					Id = flashcard.Id.ToString(),
					CourseId = courseId,
					UnitId = unitId,
					LastRateIndex = lastRateIndex,
					IsPublished = flashcard.ModerationStatus is FlashcardModerationStatus.Approved
				};

				if (flashcard.OwnerId == UserId || isModerator)
				{
					flashcardResponse.Owner = BuildShortUserInfo(flashcard.Owner);
					flashcardResponse.LastUpdateTimestamp = flashcard.LastUpdateTimestamp;
				}

				if (isModerator)
				{
					flashcardResponse.ModerationStatus = flashcard.ModerationStatus;
					flashcardResponse.Moderator = flashcard.Moderator is null ? null : BuildShortUserInfo(flashcard.Moderator);
					flashcardResponse.ModerationTimestamp = flashcard.ModerationTimestamp;
				}

				yield return flashcardResponse;
			}
		}

		private static Dictionary<string, int> BuildFlashcardsLastRateIndexes(
			IEnumerable<string> flashcardsIds,
			IReadOnlyDictionary<string, UserFlashcardsVisit> visitsByFlashcardId
		)
		{
			return flashcardsIds
				.Select(fId => visitsByFlashcardId.TryGetValue(fId, out var visit)
					? visit
					: new UserFlashcardsVisit
					{
						FlashcardId = fId,
						Timestamp = DateTime.MinValue
					}
				)
				.OrderBy(visit => visit.Timestamp)
				.Select((visit, i) => (fId: visit.FlashcardId, i))
				.ToDictionary(fWithIndex => fWithIndex.fId, fWithIndex => fWithIndex.i);
		}

		private static string GetRenderedContent(IEnumerable<SlideBlock> blocks, MarkdownRenderContext markdownContext)
		{
			var content = new StringBuilder();
			foreach (var block in blocks)
				content.Append(Flashcard.RenderBlock(block, markdownContext));
			return content.ToString();
		}

		/// <summary>
		/// Изменить оценку для флеш-карты
		/// </summary>
		///
		[Authorize]
		[HttpPut("{courseId}/flashcards/{flashcardId}/status")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		public async Task<IActionResult> Status([FromRoute] Course course, [FromRoute] string flashcardId, [FromBody] Rate rate)
		{
			var visibleUnitsIds = await unitsRepo.GetVisibleUnitIds(course, UserId);
			var unit = course.GetUnits(visibleUnitsIds).FirstOrDefault(x => x.GetFlashcardById(flashcardId) != null);

			var userFlashcard = unit is null && Guid.TryParse(flashcardId, out var flashcardIdGuid)
				? await userFlashcardsRepo.FindFlashcardById(flashcardIdGuid)
				: null;
			var canPutUserFlashcardStatus = userFlashcard is not null && (
				userFlashcard.ModerationStatus == FlashcardModerationStatus.Approved ||
				userFlashcard.OwnerId == UserId
			);

			if (unit is null && !canPutUserFlashcardStatus)
				return BadRequest($"flashcard with id {flashcardId} does not exist");
			var unitId = unit?.Id ?? userFlashcard.Id;
			await usersFlashcardsVisitsRepo.AddFlashcardVisitAsync(UserId, course.Id, unitId, flashcardId, rate);
			return NoContent();
		}
	}
}