using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Database;
using Database.Extensions;
using Database.Models;
using Database.Repos;
using Database.Repos.CourseRoles;
using Database.Repos.Groups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using Ulearn.Web.Api.Models.Parameters.Groups;
using Ulearn.Web.Api.Models.Responses;
using Ulearn.Web.Api.Models.Responses.Groups;

namespace Ulearn.Web.Api.Controllers.Groups
{
	[Route("/groups/{groupId:int:min(0)}")]
	[ProducesResponseType((int) HttpStatusCode.OK)]
	[ProducesResponseType((int) HttpStatusCode.NotFound)]
	[ProducesResponseType((int) HttpStatusCode.Forbidden)]
	[Authorize]
	public class GroupController : BaseGroupController
	{
		private readonly IGroupsRepo groupsRepo;
		private readonly IGroupAccessesRepo groupAccessesRepo;
		private readonly IGroupMembersRepo groupMembersRepo;
		private readonly ICourseRolesRepo courseRolesRepo;
		private readonly INotificationsRepo notificationsRepo;
		private readonly IGroupsCreatorAndCopier groupsCreatorAndCopier;

		public GroupController(ILogger logger, WebCourseManager courseManager, UlearnDb db,
			IGroupsRepo groupsRepo, IGroupAccessesRepo groupAccessesRepo, IGroupMembersRepo groupMembersRepo, IUsersRepo usersRepo, ICourseRolesRepo courseRolesRepo, INotificationsRepo notificationsRepo,
			IGroupsCreatorAndCopier groupsCreatorAndCopier)
			: base(logger, courseManager, db, usersRepo)
		{
			this.groupsRepo = groupsRepo;
			this.groupAccessesRepo = groupAccessesRepo;
			this.groupMembersRepo = groupMembersRepo;
			this.courseRolesRepo = courseRolesRepo;
			this.notificationsRepo = notificationsRepo;
			this.groupsCreatorAndCopier = groupsCreatorAndCopier;
		}

		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var groupId = (int) context.ActionArguments["groupId"];
			
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
			if (group == null)
			{
				context.Result = NotFound(new ErrorResponse($"Group with id {groupId} not found"));
				return;
			}

			var isVisible = await groupAccessesRepo.IsGroupVisibleForUserAsync(group, UserId).ConfigureAwait(false);
			if (!isVisible)
			{
				context.Result = StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You have no access to this group"));
				return;
			}

			context.ActionArguments["group"] = group;

			await base.OnActionExecutionAsync(context, next).ConfigureAwait(false);
		}

		/// <summary>
		/// Информация о группе
		/// </summary>
		[HttpGet]
		public async Task<ActionResult<GroupInfo>> Group(int groupId)
		{
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
			var members = await groupMembersRepo.GetGroupMembersAsync(groupId).ConfigureAwait(false);
			var accesses = await groupAccessesRepo.GetGroupAccessesAsync(groupId).ConfigureAwait(false);
			return BuildGroupInfo(group, members.Count, accesses);
		}

		/// <summary>
		/// Изменить группу
		/// </summary>
		[HttpPatch]
		public async Task<ActionResult<GroupInfo>> UpdateGroup(int groupId, [FromBody] UpdateGroupParameters parameters)
		{
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
			
			var newName = parameters.Name ?? group.Name;
			var newIsManualCheckingEnabled = parameters.IsManualCheckingEnabled ?? group.IsManualCheckingEnabled;
			var newIsManualCheckingEnabledForOldSolutions = parameters.IsManualCheckingEnabledForOldSolutions ?? group.IsManualCheckingEnabledForOldSolutions;
			var newDefaultProhibitFurtherReview = parameters.DefaultProhibitFurtherReview ?? group.DefaultProhibitFutherReview;
			var newCanUsersSeeGroupProgress = parameters.CanStudentsSeeGroupProgress ?? group.CanUsersSeeGroupProgress;
			await groupsRepo.ModifyGroupAsync(
				groupId,
				newName,
				newIsManualCheckingEnabled,
				newIsManualCheckingEnabledForOldSolutions,
				newDefaultProhibitFurtherReview,
				newCanUsersSeeGroupProgress
			).ConfigureAwait(false);

			if (parameters.IsArchived.HasValue)
				await groupsRepo.ArchiveGroupAsync(groupId, parameters.IsArchived.Value).ConfigureAwait(false);

			if (parameters.IsInviteLinkEnabled.HasValue)
				await groupsRepo.EnableInviteLinkAsync(groupId, parameters.IsInviteLinkEnabled.Value).ConfigureAwait(false);

			return BuildGroupInfo(await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false));
		}

		/// <summary>
		/// Удалить группу
		/// </summary>
		[HttpDelete]
		public async Task<IActionResult> DeleteGroup(int groupId)
		{
			await groupsRepo.DeleteGroupAsync(groupId).ConfigureAwait(false);
			return Ok(new SuccessResponse($"Group {groupId} has been deleted"));
		}

		/// <summary>
		/// Сменить владельца группы
		/// </summary>
		[HttpPut("owner")]
		[SwaggerResponse((int) HttpStatusCode.NotFound, Description = "Can't find user or user is not an instructor")]
		public async Task<IActionResult> ChangeOwner(int groupId, [FromBody] ChangeOwnerParameters parameters)
		{
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);

			var isCourseAdmin = await courseRolesRepo.HasUserAccessToCourseAsync(UserId, group.CourseId, CourseRoleType.CourseAdmin).ConfigureAwait(false);
			var canChangeOwner = group.OwnerId == UserId || isCourseAdmin;
			if (!canChangeOwner)
				return StatusCode((int) HttpStatusCode.Forbidden, new ErrorResponse("You can't change the owner of this group. Only current owner and course admin can change the owner."));

			/* New owner should exist and be a course instructor */
			var user = await usersRepo.FindUserByIdAsync(parameters.OwnerId).ConfigureAwait(false);
			if (user == null)
				return NotFound(new ErrorResponse($"Can't find user with id {parameters.OwnerId}"));
			var isInstructor = await courseRolesRepo.HasUserAccessToCourseAsync(parameters.OwnerId, group.CourseId, CourseRoleType.Instructor).ConfigureAwait(false);
			if (!isInstructor)
				return NotFound(new ErrorResponse($"User {parameters.OwnerId} is not an instructor of course {group.CourseId}"));
			
			/* Grant full access to previous owner */
			await groupAccessesRepo.GrantAccessAsync(groupId, group.OwnerId, GroupAccessType.FullAccess, UserId).ConfigureAwait(false);
			/* Change owner */
			await groupsRepo.ChangeGroupOwnerAsync(groupId, parameters.OwnerId).ConfigureAwait(false);
			/* Revoke access from new owner */
			await groupAccessesRepo.RevokeAccessAsync(groupId, parameters.OwnerId).ConfigureAwait(false);

			return Ok(new SuccessResponse($"New group's owner is {parameters.OwnerId}"));
		}

		/// <summary>
		/// Копирует группу в тот же или другой курс
		/// </summary>
		[HttpPost("copy")]
		[SwaggerResponse((int) HttpStatusCode.NotFound, Description = "Course not found")]
		[SwaggerResponse((int) HttpStatusCode.Forbidden, Description = "You have no access to destination course. You should be instructor or course admin.")]
		public async Task<IActionResult> Copy(int groupId, [FromQuery] CopyGroupParameters parameters)
		{
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
			if (! courseManager.HasCourse(parameters.DestinationCourseId))
				return NotFound($"Course {parameters.DestinationCourseId} not found");
			if (! await CanCreateGroupInCourseAsync(UserId, parameters.DestinationCourseId).ConfigureAwait(false))
				return Forbid();
			
			var newOwnerId = parameters.ChangeOwner ? UserId : null;
			var newGroup = await groupsCreatorAndCopier.CopyGroupAsync(group, parameters.DestinationCourseId, newOwnerId).ConfigureAwait(false);

			return Ok(new SuccessResponse($"Group {group.Id} has been copied to course {parameters.DestinationCourseId}. Id of new group is {newGroup.Id}"));
		}
		
		private async Task<bool> CanCreateGroupInCourseAsync(string userId, string courseId)
		{
			return await courseRolesRepo.HasUserAccessToCourseAsync(userId, courseId, CourseRoleType.Instructor).ConfigureAwait(false) || 
				await IsSystemAdministratorAsync().ConfigureAwait(false);
		}

		/// <summary>
		/// Список студентов группы
		/// </summary>
		[HttpGet("students")]
		public async Task<ActionResult<GroupStudentsResponse>> GroupStudents(int groupId)
		{
			var members = await groupMembersRepo.GetGroupMembersAsync(groupId).ConfigureAwait(false);
			return new GroupStudentsResponse
			{
				Students = members.Select(m => new GroupStudentInfo
				{
					User = BuildShortUserInfo(m.User, discloseLogin: true),
					AddingTime = m.AddingTime
				}).ToList()
			};
		}

		/// <summary>
		/// Добавить студента в группу
		/// </summary>
		[HttpPost("students/{studentId:guid}")]
		[SwaggerResponse((int) HttpStatusCode.NotFound, Description = "Can't find user")]
		[SwaggerResponse((int) HttpStatusCode.Conflict, Description = "User is already a student of this group")]
		public async Task<IActionResult> AddStudent(int groupId, string studentId)
		{
			if (!User.IsSystemAdministrator())
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("Only system administrator can add students to group directly"));
			
			var user = await usersRepo.FindUserByIdAsync(studentId).ConfigureAwait(false);
			if (user == null)
				return NotFound(new ErrorResponse($"Can't find user with id {studentId}"));
			
			var groupMember = await groupMembersRepo.AddUserToGroupAsync(groupId, studentId).ConfigureAwait(false);
			if (groupMember == null)
				return StatusCode((int)HttpStatusCode.Conflict, new ErrorResponse($"User {studentId} is already a student of group {groupId}"));

			return Ok(new SuccessResponse($"Student {studentId} is added to group {groupId}"));
		}
		
		/// <summary>
		/// Удалить студента из группы 
		/// </summary>
		[HttpDelete("students/{studentId:guid}")]
		[SwaggerResponse((int) HttpStatusCode.NotFound, Description = "Can't find user or user is not a student of this group")]
		public async Task<IActionResult> RemoveStudent(int groupId, string studentId)
		{
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
			
			var user = await usersRepo.FindUserByIdAsync(studentId).ConfigureAwait(false);
			if (user == null)
				return NotFound(new ErrorResponse($"Can't find user with id {studentId}"));

			var groupMember = await groupMembersRepo.RemoveUserFromGroupAsync(groupId, studentId).ConfigureAwait(false);
			if (groupMember == null)
				return NotFound(new ErrorResponse($"User {studentId} is not a student of group {groupId}"));
			
			await notificationsRepo.AddNotificationAsync(
				group.CourseId,
				new GroupMembersHaveBeenRemovedNotification(groupId, new List<string> { studentId }, usersRepo),
				UserId
			).ConfigureAwait(false);

			return Ok(new SuccessResponse($"Student {studentId} is removed from group {groupId}"));
		}

		/// <summary>
		/// Удалить студентов из группы
		/// </summary>
		[HttpDelete("students")]
		public async Task<IActionResult> RemoveStudents(int groupId, RemoveStudentsParameters parameters)
		{
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);

			var members = await groupMembersRepo.RemoveUsersFromGroupAsync(groupId, parameters.UserIds).ConfigureAwait(false);
			
			await notificationsRepo.AddNotificationAsync(
				group.CourseId,
				new GroupMembersHaveBeenRemovedNotification(group.Id, parameters.UserIds, usersRepo),
				UserId
			).ConfigureAwait(false);

			return Ok(new SuccessResponse($"{members.Count} students have been removed from group {group.Id}"));
		}

		/// <summary>
		/// Скопировать студентов из одной группы в другую
		/// </summary>
		[HttpPost("students/copy/to/{destinationGroupId:int:min(0)}")]
		public async Task<IActionResult> CopyStudents(int groupId, int destinationGroupId, CopyStudentsParameters parameters)
		{
			var destinationGroup = await groupsRepo.FindGroupByIdAsync(destinationGroupId).ConfigureAwait(false);
			if (destinationGroup == null)
				return NotFound(new ErrorResponse($"Group {destinationGroupId} not found"));
			
			var isDestinationGroupVisible = await groupAccessesRepo.IsGroupVisibleForUserAsync(destinationGroup, UserId).ConfigureAwait(false);
			if (!isDestinationGroupVisible)
				return StatusCode((int) HttpStatusCode.Forbidden, new ErrorResponse($"You have no access to group {destinationGroupId}"));

			var newMembers = await groupMembersRepo.CopyUsersFromOneGroupToAnotherAsync(groupId, destinationGroupId, parameters.UserIds).ConfigureAwait(false);
			
			await notificationsRepo.AddNotificationAsync(
				destinationGroup.CourseId,
				new GroupMembersHaveBeenAddedNotification(destinationGroupId, parameters.UserIds, usersRepo),
				UserId
			).ConfigureAwait(false);
			
			return Ok(new SuccessResponse($"{newMembers.Count} students have been copied from group {groupId} to group {destinationGroupId}"));
		}

		/// <summary>
		/// Список доступов к группе
		/// </summary>
		[HttpGet("accesses")]
		public async Task<ActionResult<GroupAccessesResponse>> GroupAccesses(int groupId)
		{
			var accesses = await groupAccessesRepo.GetGroupAccessesAsync(groupId).ConfigureAwait(false);
			return new GroupAccessesResponse
			{
				Accesses = accesses.Select(BuildGroupAccessesInfo).ToList()
			};
		}

		/// <summary>
		/// Выдать доступ к группе
		/// </summary>
		[HttpPost("accesses/{userId:guid}")]
		[SwaggerResponse((int) HttpStatusCode.Conflict, "User already has access to group")]
		/* TODO (andgein): We don't check that userId is Instructor of course. Should we check it? Or no? */
		public async Task<IActionResult> GrantAccess(int groupId, string userId)
		{
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
			
			var user = await usersRepo.FindUserByIdAsync(userId).ConfigureAwait(false);
			if (user == null)
				return NotFound(new ErrorResponse($"User {userId} not found"));

			var alreadyHasAccess = await groupAccessesRepo.HasUserAccessToGroupAsync(groupId, userId).ConfigureAwait(false);
			if (alreadyHasAccess)
				return Conflict(new ErrorResponse($"User {userId} already has access to group {groupId}"));

			var access = await groupAccessesRepo.GrantAccessAsync(groupId, userId, GroupAccessType.FullAccess, UserId).ConfigureAwait(false);
			await notificationsRepo.AddNotificationAsync(group.CourseId, new GrantedAccessToGroupNotification { AccessId = access.Id }, UserId).ConfigureAwait(false);
			
			return Ok(new SuccessResponse($"User {userId} has full access to {groupId}"));
		}

		/// <summary>
		/// Отозвать доступ к группе
		/// </summary>
		[HttpDelete("accesses/{userId:guid}")]
		public async Task<IActionResult> RevokeAccess(int groupId, string userId)
		{
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
			
			var canRevokeAccess = await groupAccessesRepo.CanRevokeAccessAsync(groupId, userId, UserId).ConfigureAwait(false) ||
				await IsSystemAdministratorAsync().ConfigureAwait(false);
			if (!canRevokeAccess)
				return Forbid();

			var hasAccess = await groupAccessesRepo.HasUserAccessToGroupAsync(groupId, userId).ConfigureAwait(false);
			if (!hasAccess)
				return NotFound(new ErrorResponse($"User {userId} has no access to group {groupId}"));

			var accesses = await groupAccessesRepo.RevokeAccessAsync(groupId, userId).ConfigureAwait(false);
			foreach (var access in accesses)
				await notificationsRepo.AddNotificationAsync(
					group.CourseId,
					new RevokedAccessToGroupNotification { AccessId = access.Id },
					UserId
				).ConfigureAwait(false);

			return Ok(new SuccessResponse($"User {userId} has no access to {groupId}"));
		}
	}
}