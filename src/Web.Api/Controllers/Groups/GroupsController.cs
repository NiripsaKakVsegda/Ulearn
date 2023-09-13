using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Groups;
using Database.Repos.Users;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Authorization;
using Ulearn.Web.Api.Models.Parameters.Groups;
using Ulearn.Web.Api.Models.Responses.Groups;

namespace Ulearn.Web.Api.Controllers.Groups
{
	[Route("/groups/")]
	public class GroupsController : BaseGroupController
	{
		private readonly IGroupsRepo groupsRepo;
		private readonly IGroupAccessesRepo groupAccessesRepo;
		private readonly IGroupMembersRepo groupMembersRepo;
		private readonly INotificationsRepo notificationsRepo;
		private readonly ICourseRolesRepo courseRolesRepo;

		public GroupsController(ICourseStorage courseStorage, UlearnDb db,
			IUsersRepo usersRepo,
			IGroupsRepo groupsRepo, IGroupAccessesRepo groupAccessesRepo, IGroupMembersRepo groupMembersRepo, INotificationsRepo notificationsRepo, ICourseRolesRepo courseRolesRepo)
			: base(courseStorage, db, usersRepo)
		{
			this.groupsRepo = groupsRepo;
			this.groupAccessesRepo = groupAccessesRepo;
			this.groupMembersRepo = groupMembersRepo;
			this.notificationsRepo = notificationsRepo;
			this.courseRolesRepo = courseRolesRepo;
		}

		/// <summary>
		/// Список групп в курсе
		/// </summary>
		[HttpGet]
		[Authorize(Policy = "Instructors")]
		public async Task<ActionResult<GroupsListResponse>> GroupsList([FromQuery] GroupsListParameters parameters)
		{
			return await GetGroupsListResponseAsync(parameters);
		}

		private async Task<GroupsListResponse> GetGroupsListResponseAsync(GroupsListParameters parameters)
		{
			var groups = (await groupAccessesRepo.GetAvailableForUserGroupsAsync(parameters.CourseId, UserId, false, true, parameters.Archived, GroupQueryType.SingleGroup))
				.Cast<SingleGroup>()
				.ToList();
			var superGroupsIds = groups
				.Select(g => g.SuperGroupId)
				.Distinct()
				.Where(id => id != null)
				.Cast<int>()
				.ToList();
			var superGroups = (await groupsRepo.FindGroupsByIdsAsync<SuperGroup>(superGroupsIds))
				.ToDictionary(g => g.Id);
			if (parameters.UserId != null)
			{
				var groupsWithUserAsMemberIds = (await groupMembersRepo.GetUserGroupsIdsAsync(parameters.CourseId, parameters.UserId, parameters.Archived)).ToHashSet();
				groups = groups.Where(g => groupsWithUserAsMemberIds.Contains(g.Id)).ToList();
			}

			/* Order groups by (name, createTime) and get one page of data (offset...offset+count) */
			var groupIds = groups
				.OrderBy(g => g.Name, StringComparer.InvariantCultureIgnoreCase)
				.ThenBy(g => g.Id)
				.Skip(parameters.Offset)
				.Take(parameters.Count)
				.Select(g => g.Id)
				.ToImmutableHashSet();

			var filteredGroups = groups
				.Where(g => groupIds.Contains(g.Id))
				.ToList();

			var groupMembers = await groupMembersRepo.GetGroupsMembersAsync(groupIds);
			var membersCountByGroup = groupMembers.GroupBy(m => m.GroupId).ToDictionary(g => g.Key, g => g.Count()).ToDefaultDictionary();

			var groupAccessesByGroup = await groupAccessesRepo.GetGroupAccessesAsync(groupIds);

			var groupInfos = filteredGroups.Select(g => BuildGroupInfo(
				g,
				membersCountByGroup[g.Id],
				groupAccessesByGroup[g.Id],
				addGroupApiUrl: true,
				superGroupName: g.SuperGroupId.HasValue && superGroups.ContainsKey(g.SuperGroupId.Value) ? superGroups[g.SuperGroupId.Value].Name : null
			)).ToList();

			return new GroupsListResponse
			{
				Groups = groupInfos,
				Pagination = new PaginationResponse
				{
					Offset = parameters.Offset,
					Count = filteredGroups.Count,
					TotalCount = groups.Count,
				}
			};
		}

		[HttpGet("search")]
		public async Task<ActionResult<GroupsSearchResponse>> SearchGroups([FromQuery] GroupsSearchParameters parameters)
		{
			var isSysAdmin = await IsSystemAdministratorAsync();
			if (parameters.CourseId is null && !isSysAdmin)
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("Only system administrator can search groups in all courses"));

			var isCourseAdmin = isSysAdmin || await courseRolesRepo.HasUserAccessToCourse(UserId, parameters.CourseId, CourseRoleType.CourseAdmin);
			if (parameters.InstructorId is not null && !isCourseAdmin)
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("Only course admins can search groups by instructor id"));

			var isInstructor = isCourseAdmin || await courseRolesRepo.HasUserAccessToCourse(UserId, parameters.CourseId, CourseRoleType.Instructor);
			if (!isInstructor)
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You should be at least instructor to search groups"));

			if (!isCourseAdmin)
				parameters.InstructorId = UserId;

			var searchModel = new GroupsSearchQueryModel
			{
				CourseId = parameters.CourseId,
				IncludeArchived = parameters.IncludeArchived,
				InstructorId = parameters.InstructorId,
				MemberId = parameters.MemberId,
				Query = parameters.Query,
				Offset = parameters.Offset,
				Count = parameters.Count
			};

			var groups = new List<GroupBase>();
			if (isCourseAdmin && searchModel.InstructorId is null)
			{
				searchModel.InstructorId = UserId;
				groups.AddRange(await SearchGroups(parameters.GroupType, searchModel));
				searchModel.Count -= groups.Count;
				if (searchModel.Count > 0)
					groups.AddRange(await SearchGroups(parameters.GroupType, searchModel, true));
			}
			else
			{
				groups.AddRange(await SearchGroups(parameters.GroupType, searchModel));
			}

			return new GroupsSearchResponse
			{
				Groups = groups
					.Select(BuildShortGroupInfo)
					.ToList()
			};
		}

		[HttpGet("by-ids")]
		[Authorize]
		public async Task<ActionResult<GroupsByIdsResponse>> FindGroupsByIds(
			[FromQuery] List<int> groupIds,
			[FromQuery] [CanBeNull] string courseId
		)
		{
			const int maxRequestGroupsCount = 100;

			if (groupIds.Count > maxRequestGroupsCount)
				return StatusCode((int)HttpStatusCode.RequestEntityTooLarge, new ErrorResponse($"You cannot request more than {maxRequestGroupsCount} groups"));

			var isSysAdmin = await IsSystemAdministratorAsync();

			if (courseId is null && !isSysAdmin)
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("Only system administrator can request groups in all courses"));

			if (courseId is not null)
			{
				var isInstructor = isSysAdmin || await courseRolesRepo.HasUserAccessToCourse(UserId, courseId, CourseRoleType.Instructor);
				if (!isInstructor)
					return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You should be at least instructor of course"));
			}

			var groups = await groupsRepo.FindGroupsFilterAvailableForUser<SingleGroup>(groupIds, UserId, courseId);

			return new GroupsByIdsResponse
			{
				FoundGroups = groups
					.Select(BuildShortGroupInfo)
					.ToList(),
				NotFoundGroupIds = groupIds
					.Except(groups.Select(u => u.Id))
					.ToList()
			};
		}

		/// <summary>
		/// Создать новую группу в курсе
		/// </summary>
		/// <param name="parameters">Название новой группы</param>
		[HttpPost]
		[Authorize(Policy = "Instructors")]
		[ProducesResponseType((int)HttpStatusCode.Created)]
		public async Task<ActionResult<CreateGroupResponse>> CreateGroup([FromQuery] CourseAuthorizationParameters courseAuthorizationParameters, CreateGroupParameters parameters)
		{
			var ownerId = User.GetUserId();
			var group = await groupsRepo.CreateGroupAsync(courseAuthorizationParameters.CourseId, parameters.Name, ownerId, parameters.GroupType);

			await notificationsRepo.AddNotification(
				group.CourseId,
				new CreatedGroupNotification(group.Id),
				UserId
			);

			var url = Url.Action(new UrlActionContext { Action = nameof(GroupController.Group), Controller = "Group", Values = new { groupId = group.Id } });
			return Created(url, new CreateGroupResponse
			{
				Id = group.Id,
				ApiUrl = url,
			});
		}

		public async Task<IEnumerable<GroupBase>> SearchGroups(GroupType? groupType, GroupsSearchQueryModel queryModel, bool instructorIdExcluded = false) =>
			groupType switch
			{
				GroupType.SingleGroup => await groupsRepo.SearchGroups<SingleGroup>(queryModel, instructorIdExcluded),
				GroupType.SuperGroup => await groupsRepo.SearchGroups<SuperGroup>(queryModel, instructorIdExcluded),
				_ => await groupsRepo.SearchGroups<GroupBase>(queryModel, instructorIdExcluded)
			};
	}
}