using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Groups;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models.Responses.Groups;
using Ulearn.Web.Api.Utils;

namespace Ulearn.Web.Api.Controllers.Groups
{
	[Route("/groups/{inviteHash:guid}")]
	[ProducesResponseType((int)HttpStatusCode.NotFound)]
	[Authorize]
	public class JoinGroupController : BaseGroupController
	{
		private readonly IGroupsRepo groupsRepo;
		private readonly IGroupMembersRepo groupMembersRepo;
		private readonly ISlideCheckingsRepo slideCheckingsRepo;
		private readonly SuperGroupManager superGroupManager;

		public JoinGroupController(ICourseStorage courseStorage, UlearnDb db,
			IUsersRepo usersRepo,
			IGroupsRepo groupsRepo,
			SuperGroupManager superGroupManager,
			IGroupMembersRepo groupMembersRepo,
			ISlideCheckingsRepo slideCheckingsRepo)
			: base(courseStorage, db, usersRepo)
		{
			this.groupsRepo = groupsRepo;
			this.groupMembersRepo = groupMembersRepo;
			this.slideCheckingsRepo = slideCheckingsRepo;
			this.superGroupManager = superGroupManager;
		}

		/// <summary>
		/// Найти группу по инвайт-хешу.
		/// Группа должна быть не удалена, а инвайт-ссылка в ней — включена.
		/// </summary>
		/// <param name="inviteHash">Инвайт-хеш группы</param>
		[HttpGet]
		public async Task<ActionResult<JoinGroupInfo>> Group(Guid inviteHash)
		{
			var group = await groupsRepo.FindGroupByInviteHashAsync(inviteHash, false).ConfigureAwait(false);

			if (group is null)
				return NotFound(new ErrorResponse($"Group with invite hash {inviteHash} not found"));

			var isInviteLinkEnabled = group.IsInviteLinkEnabled;

			SuperGroupError? error = null;
			if (group is SuperGroup superGroup)
			{
				if (superGroup.DistributionTableLink is null)
				{
					error = SuperGroupError.NoDistributionLink;
				}
				else
				{
					var groups = await GetGroupsToJoinViaSuperGroup(superGroup);
					if (groups.Count == 1)
						group = groups[0];
					else
						error = SuperGroupError.NoGroupFoundForStudent;
				}
			}

			var singleGroup = group as SingleGroup;
			return new JoinGroupInfo
			{
				Id = group.Id,
				Name = group.Name,
				CourseId = group.CourseId,
				Owner = BuildShortUserInfo(group.Owner),
				IsInviteLinkEnabled = isInviteLinkEnabled,
				CanStudentsSeeProgress = singleGroup is not null &&
										singleGroup.CanUsersSeeGroupProgress,
				SuperGroupError = error,
				IsMember = singleGroup is not null &&
							await groupMembersRepo.IsUserMemberOfGroup(singleGroup.Id, UserId)
				// Can't use singleGroup.Members.Any(u => u.UserId == UserId) because EF Core doesn't correctly update related entities after removing student from group
			};
		}

		/// <summary>
		/// Вступить в группу по инвайт-хешу.
		/// Группа должна быть не удалена, а инвайт-ссылка в ней — включена.
		/// </summary>
		/// <param name="inviteHash">Инвайт-хеш группы</param>
		[HttpPut("join")]
		[SwaggerResponse((int)HttpStatusCode.Conflict, Description = "User is already a student of this group")]
		public async Task<IActionResult> Join(Guid inviteHash)
		{
			var group = await groupsRepo.FindGroupByInviteHashAsync(inviteHash).ConfigureAwait(false);

			if (group is null)
				return NotFound(new ErrorResponse($"Group with invite hash {inviteHash} not found"));

			if (group is SuperGroup superGroup)
				return await JoinSuperGroup(superGroup);

			var groupMember = await groupMembersRepo.AddUserToGroupAsync(group.Id, UserId).ConfigureAwait(false);
			if (groupMember == null)
				return StatusCode((int)HttpStatusCode.Conflict, new ErrorResponse($"User {UserId} is already a student of group {group.Id}"));

			await slideCheckingsRepo.ResetManualCheckingLimitsForUser(group.CourseId, UserId);

			return Ok(new SuccessResponseWithMessage($"Student {UserId} is added to group {group.Id}"));
		}

		private async Task<ActionResult> JoinSuperGroup(SuperGroup superGroup)
		{
			var groups = await GetGroupsToJoinViaSuperGroup(superGroup);

			if (groups.Count == 0)
				return NotFound(new ErrorResponse("No group found for user"));
			if (groups.Count > 1)
				return NotFound(new ErrorResponse("Found multiple groups for user"));

			var groupToJoin = groups[0];

			var groupMember = await groupMembersRepo.AddUserToGroupAsync(groupToJoin.Id, UserId).ConfigureAwait(false);
			if (groupMember is null)
				return Conflict(new ErrorResponse($"User {UserId} is already a student of group {groupToJoin.Id}"));

			await slideCheckingsRepo.ResetManualCheckingLimitsForUser(groupToJoin.CourseId, UserId);

			return Ok(new SuccessResponseWithMessage($"Student {UserId} is added to group {superGroup.Id}"));
		}

		private async Task<List<SingleGroup>> GetGroupsToJoinViaSuperGroup(SuperGroup superGroup)
		{
			var (createdGroups, spreadSheetGroups) = await superGroupManager.GetSheetGroupsAndCreatedGroups(
				superGroup.DistributionTableLink,
				superGroup.Id
			);
			var user = (await usersRepo.FindUserById(UserId))!;

			var userNames = new HashSet<string>
			{
				$"{user.FirstName.Trim()} {user.LastName.Trim()}",
				$"{user.LastName.Trim()} {user.FirstName.Trim()}"
			};
			var userGroupsNames = spreadSheetGroups
				.Where(g => userNames.Contains(g.studentName))
				.Select(g => g.groupName)
				.ToHashSet();

			return createdGroups
				.Where(g => userGroupsNames.Contains(g.Name))
				.ToList();
		}
	}
}