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

			if (group is SuperGroup superGroup)
			{
				var getSubGroupsUserMember = await groupMembersRepo.GetUserSubGroups(superGroup.Id, UserId);
				if (getSubGroupsUserMember.Count > 0)
				{
					group = getSubGroupsUserMember[0];
				}
				else
				{
					var groups = superGroup.DistributionTableLink is null
						? new List<SingleGroup>()
						: await GetGroupsToJoinViaSuperGroup(superGroup);
					if (groups.Count == 1)
						group = groups[0];
				}
			}

			var isMember = await groupMembersRepo.IsUserMemberOfGroup(group.Id, UserId);
			var isInDefaultGroup = group switch
			{
				SuperGroup => isMember,
				SingleGroup { SuperGroupId: not null } singleGroup =>
					await groupMembersRepo.IsUserMemberOfGroup(singleGroup.SuperGroupId.Value, UserId),
				_ => false
			};

			return new JoinGroupInfo
			{
				Id = group.Id,
				GroupType = group.GroupType,
				Name = group.Name,
				CourseId = group.CourseId,
				CourseTitle = courseStorage.FindCourse(group.CourseId)?.Title,
				Owner = BuildShortUserInfo(group.Owner),
				IsInviteLinkEnabled = isInviteLinkEnabled,
				CanStudentsSeeProgress = group is SingleGroup { CanUsersSeeGroupProgress: true },
				IsMember = isMember,
				IsInDefaultGroup = isInDefaultGroup
				// Can't use group.Members.Any(u => u.UserId == UserId) because EF Core doesn't correctly update related entities after removing student from group
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
			if (group is SingleGroup { SuperGroupId: not null } singleGroup)
				await groupMembersRepo.RemoveUserFromGroupAsync(singleGroup.SuperGroupId.Value, UserId);

			if (groupMember == null)
				return StatusCode((int)HttpStatusCode.Conflict, new ErrorResponse($"User {UserId} is already a student of group {group.Id}"));

			await slideCheckingsRepo.ResetManualCheckingLimitsForUser(group.CourseId, UserId);

			return Ok(new SuccessResponseWithMessage($"Student {UserId} is added to group {group.Id}"));
		}

		private async Task<ActionResult> JoinSuperGroup(SuperGroup superGroup)
		{
			var groups = superGroup.DistributionTableLink is null
				? new List<SingleGroup>()
				: await GetGroupsToJoinViaSuperGroup(superGroup);

			GroupBase groupToJoin = groups.Count == 1
				? groups[0]
				: superGroup;

			var groupMember = await groupMembersRepo.AddUserToGroupAsync(groupToJoin.Id, UserId).ConfigureAwait(false);
			if (groupToJoin.GroupType is GroupType.SingleGroup)
				await groupMembersRepo.RemoveUserFromGroupAsync(superGroup.Id, UserId);

			if (groupMember is null)
				return Conflict(new ErrorResponse($"User {UserId} is already a student of group {groupToJoin.Id}"));

			await slideCheckingsRepo.ResetManualCheckingLimitsForUser(groupToJoin.CourseId, UserId);

			return Ok(new SuccessResponseWithMessage($"Student {UserId} is added to group {groupToJoin.Id}"));
		}

		private async Task<List<SingleGroup>> GetGroupsToJoinViaSuperGroup(SuperGroup superGroup)
		{
			var (createdGroups, spreadSheetGroups) = await superGroupManager.GetSheetGroupsAndCreatedGroups(
				superGroup.DistributionTableLink,
				superGroup.Id
			);
			var user = (await usersRepo.FindUserById(UserId))!;

			var userNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				$"{user.FirstName.Trim()} {user.LastName.Trim()}".Replace('ё', 'е'),
				$"{user.LastName.Trim()} {user.FirstName.Trim()}".Replace('ё', 'е')
			};
			var userGroupsNames = spreadSheetGroups
				.Where(g => userNames.Contains(g.studentName.Trim().Replace('ё', 'е')))
				.Select(g => g.groupName)
				.ToHashSet();

			return createdGroups
				.Where(g => userGroupsNames.Contains(g.Name))
				.ToList();
		}
	}
}