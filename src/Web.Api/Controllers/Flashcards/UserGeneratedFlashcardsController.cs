using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Flashcards;
using Database.Repos.Users;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models.Parameters.Flashcards;
using Ulearn.Web.Api.Models.Responses.Flashcards;
using Web.Api.Configuration;

namespace Ulearn.Web.Api.Controllers.Flashcards;

[Route("/user-flashcards")]
public class UserGeneratedFlashcardsController : BaseFlashcardController
{
	private readonly IUserGeneratedFlashcardsRepo userFlashcardsRepo;
	private readonly IUsersFlashcardsVisitsRepo visitsRepo;
	private readonly IUnitsRepo unitsRepo;

	public UserGeneratedFlashcardsController(
		ICourseStorage courseStorage,
		UlearnDb db,
		IUsersRepo usersRepo,
		ICourseRolesRepo courseRolesRepo,
		ICoursesRepo coursesRepo,
		IOptions<WebApiConfiguration> configuration,
		IUserGeneratedFlashcardsRepo userFlashcardsRepo,
		IUsersFlashcardsVisitsRepo visitsRepo,
		IUnitsRepo unitsRepo
	)
		: base(courseStorage, db, usersRepo, courseRolesRepo, coursesRepo, configuration)
	{
		this.userFlashcardsRepo = userFlashcardsRepo;
		this.visitsRepo = visitsRepo;
		this.unitsRepo = unitsRepo;
	}

	[HttpGet]
	[Authorize]
	[SwaggerResponse((int)HttpStatusCode.OK)]
	[SwaggerResponse((int)HttpStatusCode.NotFound)]
	[SwaggerResponse((int)HttpStatusCode.Forbidden)]
	public async Task<ActionResult<UserGeneratedFlashcardsResponse>> GetUserGeneratedFlashcards(
		[FromQuery] [Required] string courseId,
		[FromQuery] [CanBeNull] Guid? unitId,
		[FromQuery] [CanBeNull] FlashcardModerationStatus? status
	)
	{
		var course = courseStorage.FindCourse(courseId);
		if (course is null)
			return NotFound(new ErrorResponse($"Course with id {courseId} not found"));

		if (!await CanUserModerateFlashcards(course.Id))
			return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You has no access to user flashcards!"));

		if (unitId is { } unitIdValue)
		{
			var visibleUnitsIds = await unitsRepo.GetVisibleUnitIds(course, UserId);
			var unit = course.FindUnitById(unitIdValue, visibleUnitsIds);
			if (unit is null)
				return NotFound(new ErrorResponse($"Unit with id {unitId} not found in course!"));
		}

		var flashcards = await userFlashcardsRepo.GetFlashcards(course.Id, unitId, status);
		var flashcardsRates = (await visitsRepo.GetLastUserFlashcardsVisitsAsync(UserId, course.Id, unitId))
			.ToDictionary(
				g => g.FlashcardId,
				g => g.Rate
			);
		return new UserGeneratedFlashcardsResponse
		{
			Flashcards = flashcards
				.Select(flashcard => BuildUserFlashcardResponse(
					flashcard,
					flashcardsRates.TryGetValue(flashcard.Id.ToString(), out var rate) ? rate : Rate.NotRated)
				)
				.ToList()
		};
	}

	[HttpPost]
	[Authorize]
	[SwaggerResponse((int)HttpStatusCode.OK)]
	[SwaggerResponse((int)HttpStatusCode.NotFound)]
	[SwaggerResponse((int)HttpStatusCode.Forbidden)]
	[SwaggerResponse((int)HttpStatusCode.BadRequest)]
	public async Task<ActionResult<UserGeneratedFlashcardResponse>> CreateFlashcard([FromBody] CreateFlashcardParameters parameters)
	{
		var course = courseStorage.FindCourse(parameters.CourseId);
		if (course is null)
			return NotFound(new ErrorResponse($"Course with id {parameters.CourseId} not found!"));

		var visibleUnitsIds = await unitsRepo.GetVisibleUnitIds(course, UserId);
		var unit = course.FindUnitById(parameters.UnitId, visibleUnitsIds);
		if (unit is null)
			return NotFound(new ErrorResponse($"Unit with id {parameters.UnitId} not found in course!"));

		if (!unit.ContainsFlashcards)
			return BadRequest(new ErrorResponse($"Unit with id {parameters.UnitId} cannot contain flashcards!"));

		var approved = parameters.Approved ?? false;
		if (approved && !await CanUserModerateFlashcards(course.Id))
			return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You has no permissions to create approved flashcard!"));

		var flashcard = await userFlashcardsRepo.AddFlashcard(
			UserId,
			parameters.CourseId,
			parameters.UnitId,
			parameters.Question,
			parameters.Answer,
			approved
		);

		return BuildUserFlashcardResponse(flashcard, Rate.NotRated);
	}

	[HttpPatch("{flashcardId:guid}")]
	[Authorize]
	[SwaggerResponse((int)HttpStatusCode.OK)]
	[SwaggerResponse((int)HttpStatusCode.NotFound)]
	[SwaggerResponse((int)HttpStatusCode.Forbidden)]
	public async Task<ActionResult<UserGeneratedFlashcardResponse>> EditFlashcard(Guid flashcardId, [FromBody] EditFlashcardParameters parameters)
	{
		if (parameters.Question is null && parameters.Answer is null)
			return BadRequest(new ErrorResponse("Question and answer cannot both be null!"));

		var flashcard = await userFlashcardsRepo.FindFlashcardById(flashcardId);
		if (flashcard is null)
			return NotFound(new ErrorResponse($"Flashcard with id {flashcardId} not found!"));

		var isOwner = flashcard.OwnerId == UserId;
		var isModerator = await CanUserModerateFlashcards(flashcard.CourseId);

		if (!isOwner && !isModerator)
			return NotFound(new ErrorResponse($"Flashcard with id {flashcardId} not found!"));

		if (!isModerator && flashcard.ModerationStatus is FlashcardModerationStatus.Approved)
			return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You cannot edit flashcard after it was published by moderator!"));

		if (!isOwner && flashcard.ModerationStatus is not FlashcardModerationStatus.Approved)
			return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You cannot edit user's flashcards without approving it!"));

		flashcard = flashcard.ModerationStatus is FlashcardModerationStatus.Approved
			? await userFlashcardsRepo.ApproveFlashcard(flashcardId, UserId, parameters.Question, parameters.Answer)
			: await userFlashcardsRepo.EditFlashcard(flashcardId, parameters.Question, parameters.Answer);

		return BuildUserFlashcardResponse(flashcard, await GetFlashcardLastRate(flashcard), !isModerator);
	}

	[HttpDelete("{flashcardId:guid}")]
	[Authorize]
	[SwaggerResponse((int)HttpStatusCode.NoContent)]
	[SwaggerResponse((int)HttpStatusCode.NotFound)]
	[SwaggerResponse((int)HttpStatusCode.Forbidden)]
	public async Task<ActionResult> RemoveFlashcard(Guid flashcardId)
	{
		var flashcard = await userFlashcardsRepo.FindFlashcardById(flashcardId);
		if (flashcard is null)
			return NotFound(new ErrorResponse($"Flashcard with id {flashcardId} not found!"));

		var isOwner = flashcard.OwnerId == UserId;
		var isModerator = await CanUserModerateFlashcards(flashcard.CourseId);

		if (!isOwner && !isModerator)
			return NotFound(new ErrorResponse($"Flashcard with id {flashcardId} not found!"));

		if (!isOwner)
			return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You cannot remove user's flashcards!"));

		if (!isModerator && flashcard.ModerationStatus is FlashcardModerationStatus.Approved)
			return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You cannot remove flashcard after it was published by moderator!"));

		await userFlashcardsRepo.DeleteFlashcard(flashcardId);

		return NoContent();
	}

	[HttpPut("{flashcardId:guid}/approve")]
	[Authorize]
	[SwaggerResponse((int)HttpStatusCode.OK)]
	[SwaggerResponse((int)HttpStatusCode.NotFound)]
	[SwaggerResponse((int)HttpStatusCode.Forbidden)]
	public async Task<ActionResult<UserGeneratedFlashcardResponse>> ApproveFlashcard(Guid flashcardId, [FromBody] EditFlashcardParameters parameters)
	{
		var flashcard = await userFlashcardsRepo.FindFlashcardById(flashcardId);
		if (flashcard is null)
			return NotFound(new ErrorResponse($"Flashcard with id {flashcardId} not found!"));

		var isOwner = flashcard.OwnerId == UserId;
		var isModerator = await CanUserModerateFlashcards(flashcard.CourseId);

		if (!isModerator)
			return isOwner
				? StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You have no permissions to approve flashcards!"))
				: NotFound(new ErrorResponse($"Flashcard with id {flashcardId} not found!"));

		flashcard = await userFlashcardsRepo.ApproveFlashcard(flashcardId, UserId, parameters.Question, parameters.Answer);

		return BuildUserFlashcardResponse(flashcard, await GetFlashcardLastRate(flashcard));
	}

	[HttpPut("{flashcardId:guid}/decline")]
	[Authorize]
	[SwaggerResponse((int)HttpStatusCode.OK)]
	[SwaggerResponse((int)HttpStatusCode.NotFound)]
	[SwaggerResponse((int)HttpStatusCode.Forbidden)]
	public async Task<ActionResult<UserGeneratedFlashcardResponse>> DeclineFlashcard(Guid flashcardId)
	{
		var flashcard = await userFlashcardsRepo.FindFlashcardById(flashcardId);
		if (flashcard is null)
			return NotFound(new ErrorResponse($"Flashcard with id {flashcardId} not found!"));

		var isOwner = flashcard.OwnerId == UserId;
		var isModerator = await CanUserModerateFlashcards(flashcard.CourseId);

		if (!isModerator)
			return isOwner
				? StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You have no permissions to decline flashcards!"))
				: NotFound(new ErrorResponse($"Flashcard with id {flashcardId} not found!"));

		flashcard = await userFlashcardsRepo.DeclineFlashcard(flashcardId, UserId);

		return BuildUserFlashcardResponse(flashcard, await GetFlashcardLastRate(flashcard));
	}

	[HttpPut("{flashcardId:guid}/restore")]
	[Authorize]
	[SwaggerResponse((int)HttpStatusCode.OK)]
	[SwaggerResponse((int)HttpStatusCode.NotFound)]
	[SwaggerResponse((int)HttpStatusCode.Forbidden)]
	public async Task<ActionResult<UserGeneratedFlashcardResponse>> RestoreFlashcard(Guid flashcardId)
	{
		var flashcard = await userFlashcardsRepo.FindFlashcardById(flashcardId);
		if (flashcard is null)
			return NotFound(new ErrorResponse($"Flashcard with id {flashcardId} not found!"));

		var isOwner = flashcard.OwnerId == UserId;
		var isModerator = await CanUserModerateFlashcards(flashcard.CourseId);

		if (!isModerator)
			return isOwner
				? StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You have no permissions to restore flashcards!"))
				: NotFound(new ErrorResponse($"Flashcard with id {flashcardId} not found!"));

		flashcard = await userFlashcardsRepo.RestoreFlashcard(flashcardId);

		return BuildUserFlashcardResponse(flashcard, await GetFlashcardLastRate(flashcard));
	}

	private UserGeneratedFlashcardResponse BuildUserFlashcardResponse(
		UserGeneratedFlashcard flashcard,
		Rate rate,
		bool hideModerationInfo = false
	)
	{
		return new UserGeneratedFlashcardResponse
		{
			Id = flashcard.Id.ToString(),
			CourseId = flashcard.CourseId,
			UnitId = flashcard.UnitId,
			Question = flashcard.Question,
			Answer = flashcard.Answer,
			Rate = rate,
			LastRateIndex = 0,
			IsPublished = flashcard.ModerationStatus is FlashcardModerationStatus.Approved,
			Owner = BuildShortUserInfo(flashcard.Owner),
			LastUpdateTimestamp = flashcard.LastUpdateTimestamp,
			ModerationStatus = hideModerationInfo ? null : flashcard.ModerationStatus,
			Moderator = hideModerationInfo || flashcard.Moderator is null
				? null
				: BuildShortUserInfo(flashcard.Moderator),
			ModerationTimestamp = hideModerationInfo ? null : flashcard.ModerationTimestamp
		};
	}

	private async Task<Rate> GetFlashcardLastRate(UserGeneratedFlashcard flashcard)
	{
		var visit = await visitsRepo.FindLastUserFlashcardVisitAsync(UserId, flashcard.CourseId, flashcard.Id.ToString());
		return visit?.Rate ?? Rate.NotRated;
	}
}