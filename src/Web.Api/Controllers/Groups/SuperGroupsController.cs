using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Repos;
using Database.Repos.Groups;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models.Parameters.Groups;
using Ulearn.Web.Api.Models.Responses.Groups;

namespace Ulearn.Web.Api.Controllers.Groups;

[Route("/super-groups/")]
[Authorize(Policy = "Instructors")]
public class SuperGroupsController : BaseGroupController
{
	private readonly IGroupsRepo groupsRepo;
	private readonly IGroupAccessesRepo groupAccessesRepo;
	private readonly IGroupMembersRepo groupMembersRepo;
	private readonly INotificationsRepo notificationsRepo;

	public SuperGroupsController(ICourseStorage courseStorage, UlearnDb db,
		IUsersRepo usersRepo,
		IGroupsRepo groupsRepo, IGroupAccessesRepo groupAccessesRepo, IGroupMembersRepo groupMembersRepo, INotificationsRepo notificationsRepo)
		: base(courseStorage, db, usersRepo)
	{
		this.groupsRepo = groupsRepo;
		this.groupAccessesRepo = groupAccessesRepo;
		this.groupMembersRepo = groupMembersRepo;
		this.notificationsRepo = notificationsRepo;
	}

	/// <summary>
	/// Список супер-групп в курсе
	/// </summary>
	[HttpGet]
	public async Task<ActionResult<SuperGroupsListResponse>> GroupsList([FromQuery] GroupsListParameters parameters)
	{
		var groups = await groupAccessesRepo.GetAvailableForUserGroupsAsync(parameters.CourseId, UserId, false, true, true, GroupQueryType.SuperGroup);

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
		var groupsIds = filteredGroups
			.Select(g => g.Id)
			.ToList();

		var subGroups = await groupsRepo.FindGroupsBySuperGroupIdsAsync(groupsIds, true);
		var subGroupsIds = subGroups.Select(g => g.Id).ToList();

		var membersCountByGroupId = await db.GroupMembers
			.Where(m => groupsIds.Contains(m.GroupId) || subGroupsIds.Contains(m.GroupId))
			.Where(m => !m.User.IsDeleted)
			.Select(m => m.GroupId)
			.GroupBy(id => id)
			.Select(g => new { g.Key, Count = g.Count() })
			.ToDictionaryAsync(g => g.Key, g => g.Count);

		var superGroupAccessesByGroup = await groupAccessesRepo.GetGroupAccessesAsync(groupsIds);
		var groupAccessesByGroup = await groupAccessesRepo.GetGroupAccessesAsync(subGroupsIds);

		var groupInfos = filteredGroups
			.Select(g => BuildGroupInfo(
				g,
				membersCountByGroupId.GetOrDefault(g.Id, 0),
				superGroupAccessesByGroup[g.Id],
				addGroupApiUrl: true))
			.ToList();
		var subGroupInfos = subGroups
			.Select(g => BuildGroupInfo(
				g,
				membersCountByGroupId.GetOrDefault(g.Id, 0),
				groupAccessesByGroup[g.Id],
				addGroupApiUrl: true))
			.GroupBy(g => g.SuperGroupId)
			.ToDictionary(g => g.Key.Value, g => g.ToList());

		return new SuperGroupsListResponse
		{
			SuperGroups = groupInfos,
			SubGroupsBySuperGroupId = subGroupInfos,
			Pagination = new PaginationResponse
			{
				Offset = parameters.Offset,
				Count = filteredGroups.Count,
				TotalCount = groups.Count,
			}
		};
	}
}