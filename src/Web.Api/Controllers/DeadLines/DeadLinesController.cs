using System;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Users;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models.Responses;

namespace Ulearn.Web.Api.Controllers.DeadLines
{
	[Authorize]
	[Route("/dead-lines")]
	public class DeadLinesController : BaseController
	{
		private IDeadLinesRepo deadLinesRepo;
		private ICourseRolesRepo courseRolesRepo;

		public DeadLinesController(
			ICourseStorage courseStorage,
			UlearnDb db,
			IDeadLinesRepo deadLinesRepo,
			ICourseRolesRepo courseRolesRepo,
			IUsersRepo usersRepo)
			: base(courseStorage, db, usersRepo)
		{
			this.deadLinesRepo = deadLinesRepo;
			this.courseRolesRepo = courseRolesRepo;
		}

		[HttpGet("for-user")]
		public async Task<ActionResult<DeadLinesResponse>> GetDeadLinesForUser([FromQuery] string courseId)
		{
			var deadLines = await deadLinesRepo.GetDeadLinesForUser(courseId, Guid.Parse(UserId));

			return new DeadLinesResponse
			{
				DeadLines = deadLines,
			};
		}

		[HttpGet("for-user/{userId}")]
		public async Task<ActionResult<DeadLinesResponse>> GetDeadLinesForUser([FromQuery] string courseId, [FromQuery] string userId)
		{
			var isInstructor = await courseRolesRepo.HasUserAccessToCourse(UserId, courseId, CourseRoleType.Tester);
			if (!isInstructor)
				return Forbid($"You do not have an access to view dead lines in course {courseId}");

			var deadLines = await deadLinesRepo.GetDeadLinesForUser(courseId, Guid.Parse(UserId));

			return new DeadLinesResponse
			{
				DeadLines = deadLines,
			};
		}

		[HttpGet]
		public async Task<ActionResult<DeadLinesResponse>> GetDeadLines([FromQuery] string courseId, [FromQuery] int groupId)
		{
			var isInstructor = await courseRolesRepo.HasUserAccessToCourse(UserId, courseId, CourseRoleType.Tester);
			if (!isInstructor)
				return Forbid($"You do not have an access to view dead lines in course {courseId}");
			
			var deadLines = await deadLinesRepo.GetDeadLines(courseId, groupId);

			return new DeadLinesResponse
			{
				DeadLines = deadLines,
			};
		}

		[HttpPost]
		public async Task<ActionResult<DeadLine>> CreateDeadLine(
			[FromQuery] string courseId,
			[FromQuery] int groupId,
			[FromQuery] Guid unitId,
			[FromQuery] [CanBeNull] Guid? slideId,
			[FromQuery] [CanBeNull] Guid? userId,
			[FromQuery] DateTime date,
			[FromQuery] int scorePercent)
		{
			var isInstructor = await courseRolesRepo.HasUserAccessToCourse(UserId, courseId, CourseRoleType.Tester);
			if (!isInstructor)
				return Forbid($"You do not have an access to add dead lines in course {courseId}");
			
			var deadLine = await deadLinesRepo.AddDeadLine(courseId, groupId, unitId, slideId, userId, date, scorePercent);

			return deadLine;
		}


		[Route("{deadLineId}")]
		[HttpPatch]
		public async Task<ActionResult> UpdateDeadLine(
			[FromRoute] Guid deadLineId,
			[FromQuery] Guid unitId,
			[FromQuery] [CanBeNull] Guid? slideId,
			[FromQuery] [CanBeNull] Guid? userId,
			[FromQuery] DateTime date,
			[FromQuery] int scorePercent)
		{
			var deadLine = await deadLinesRepo.GetDeadLineById(deadLineId);
			
			if (deadLine == null)
			{
				return NotFound($"Dead line with id {deadLineId} was not found");
			}

			var isInstructor = await courseRolesRepo.HasUserAccessToCourse(UserId, deadLine.CourseId, CourseRoleType.Tester);
			if (!isInstructor)
				return Forbid($"You do not have an access to update dead lines in course {deadLine.CourseId}");

			deadLine.UnitId = unitId;
			deadLine.SlideId = slideId;
			deadLine.UserId = userId;
			deadLine.Date = date;
			deadLine.ScorePercent = scorePercent;

			await deadLinesRepo.UpdateDeadLine(deadLine);

			return NoContent();
		}

		[Route("{deadLineId}")]
		[HttpDelete]
		public async Task<ActionResult> DeleteDeadLine([FromRoute] Guid deadLineId)
		{
			var deadLine = await deadLinesRepo.GetDeadLineById(deadLineId);
			if (deadLine == null)
			{
				return NotFound($"Dead line with id {deadLineId} was not found");
			}
			
			var isInstructor = await courseRolesRepo.HasUserAccessToCourse(UserId, deadLine.CourseId, CourseRoleType.Tester);
			if (!isInstructor)
				return Forbid($"You do not have an access to delete dead lines in course {deadLine.CourseId}");

			await deadLinesRepo.DeleteDeadLine(deadLine);

			return NoContent();
		}
	}
}