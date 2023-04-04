using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos.Groups;
using Database.Repos.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models.Common.SuperGroup;
using Ulearn.Web.Api.Models.Parameters.Groups.SuperGroup;
using Ulearn.Web.Api.Models.Responses.Groups;
using Ulearn.Web.Api.Utils;

namespace Ulearn.Web.Api.Controllers.Groups;

[Route("/super-group")]
public class SuperGroupController : BaseGroupController
{
	private readonly SuperGroupManager superGroupHelper;
	private readonly IGroupsRepo groupsRepo;
	private readonly IGroupAccessesRepo groupAccessesRepo;
	private readonly IGroupMembersRepo groupMembersRepo;


	public SuperGroupController(
		ICourseStorage courseStorage,
		UlearnDb db,
		IUsersRepo usersRepo,
		IGroupsRepo groupsRepo,
		IGroupAccessesRepo groupAccessesRepo,
		IGroupMembersRepo groupMembersRepo,
		SuperGroupManager superGroupHelper)
		: base(courseStorage, db, usersRepo)
	{
		this.superGroupHelper = superGroupHelper;
		this.groupsRepo = groupsRepo;
		this.groupAccessesRepo = groupAccessesRepo;
		this.groupMembersRepo = groupMembersRepo;
	}

	public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var groupId = (int)context.ActionArguments["groupId"];

		var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
		if (group == null)
		{
			context.Result = NotFound(new ErrorResponse($"Auto-group with id {groupId} not found"));
			return;
		}

		var hasEditAccess = await groupAccessesRepo.HasUserEditAccessToGroupAsync(groupId, UserId).ConfigureAwait(false);
		if (!hasEditAccess)
		{
			context.Result = new ForbidResult("You have no access to this auto-group");
			return;
		}

		await base.OnActionExecutionAsync(context, next).ConfigureAwait(false);
	}

	[HttpGet]
	[Route("extract-spreadsheet")]
	[ProducesResponseType((int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.Forbidden)]
	[ProducesResponseType((int)HttpStatusCode.NotFound)]
	public async Task<ActionResult<SuperGroupSheetExtractionResult>> ExtractFromGoogleSheet([FromQuery] string spreadsheetUrl, [FromQuery] int groupId)
	{
		var superGroup = await groupsRepo.FindGroupByIdAsync<SuperGroup>(groupId);
		var (createdGroups, spreadSheetGroups)
			= await superGroupHelper.GetSheetGroupsAndCreatedGroups(spreadsheetUrl, groupId, false);

		if (superGroup.DistributionTableLink != spreadsheetUrl)
			await groupsRepo.ModifyGroupAsync<SuperGroup>(groupId, new GroupSettings { DistributionTableLink = spreadsheetUrl });

		return await BuildSpreadSheetExtractionData(createdGroups, spreadSheetGroups);
	}

	[HttpPost]
	[Route("update-groups")]
	[ProducesResponseType((int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.Forbidden)]
	[ProducesResponseType((int)HttpStatusCode.NotFound)]
	public async Task<ActionResult<Dictionary<string, SupperGroupUpdateItem>>> UpdateGroups([FromQuery] int groupId, [FromBody] UpdateGroupsRequestParameters parameters)
	{
		if (parameters.GroupsToUpdate.Count == 0)
			return BadRequest($"There is no groups to update");

		var updateInfoByGroupName = new Dictionary<string, SupperGroupUpdateItem>();

		var superGroup = await groupsRepo.FindGroupByIdAsync<SuperGroup>(groupId);

		var createdGroups = await groupsRepo.FindGroupsBySuperGroupIdAsync(groupId);

		var createdGroupsByName = createdGroups
			.ToDictionary(g => g.Name);

		foreach (var (groupName, status) in parameters.GroupsToUpdate.Where(g => g.Value == SuperGroupItemActions.ShouldBeCreated))
		{
			if (!createdGroupsByName.TryGetValue(groupName, out var group))
				group = await groupsRepo.CreateSingleGroupAsync(
					superGroup.CourseId,
					groupName,
					superGroup.OwnerId,
					groupId);

			updateInfoByGroupName.Add(groupName, new SupperGroupUpdateItem { GroupId = group.Id, GroupName = groupName });
		}

		foreach (var (groupName, status) in parameters.GroupsToUpdate.Where(g => g.Value == SuperGroupItemActions.ShouldBeDeleted))
		{
			if (!createdGroupsByName.TryGetValue(groupName, out var groupToDelete) || groupToDelete.NotDeletedMembers.Count != 0)
				continue;
			
			await groupsRepo.DeleteGroupAsync(groupToDelete.Id);
			updateInfoByGroupName.Add(groupName, new SupperGroupUpdateItem { GroupName = groupName });
		}

		return Ok(updateInfoByGroupName);
	}

	[HttpPost]
	[Route("resort-students")]
	[ProducesResponseType((int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.Forbidden)]
	[ProducesResponseType((int)HttpStatusCode.BadRequest)]
	public async Task<ActionResult<SuperGroupMoveUserResponse>> ResortStudents([FromQuery] int groupId, [FromBody] Dictionary<string, MoveStudentInfo> moves) 
	{
		var superGroup = await groupsRepo.FindGroupByIdAsync<SuperGroup>(groupId);
		if (superGroup.DistributionTableLink == null)
			return BadRequest("Auto-group doesn't have a correct link to google sheet table");

		var createdGroups = await groupsRepo.FindGroupsBySuperGroupIdAsync(superGroup.Id);

		var createdGroupsByNames = createdGroups.ToDictionary(g => g.Name, g => g);

		var notCreatedGroups = moves
			.SelectMany(p => new[] { p.Value.FromGroupName, p.Value.ToGroupName })
			.Distinct()
			.Where(groupName => !createdGroupsByNames.ContainsKey(groupName))
			.ToList();

		if (notCreatedGroups.Count > 0)
			return BadRequest($"Can't sort users in not created groups ({string.Join(", ", notCreatedGroups)}), please create these groups first");

		var movedUsers = await MoveUsers(createdGroupsByNames, moves);

		return Ok(new SuperGroupMoveUserResponse { MovedUsers = movedUsers });
	}

	private async Task<List<MoveUserInfo>> MoveUsers(
		Dictionary<string, SingleGroup> createdGroupsByNames,
		Dictionary<string, MoveStudentInfo> usersToMoveByGroupName
	)
	{
		var createdGroupsIds = createdGroupsByNames
			.Values
			.Select(g => g.Id)
			.ToList();

		var joinedStudents = await groupMembersRepo.GetGroupsMembersAsync(createdGroupsIds);

		var joinedStudentsByName_withoutNamesake = joinedStudents
			.GroupBy(m => m.User.VisibleName)
			.Where(m => m.Count() == 1)
			.ToDictionary(
				m => m.Key,
				m => m.FirstOrDefault()
			);

		var moveResults = new List<MoveUserInfo>();
		var usersToRemoveByGroupsIds = new Dictionary<int, List<string>>();
		var usersToAddByGroupsIds = new Dictionary<int, List<string>>();

		foreach (var (studentName, moveStudentInfo) in usersToMoveByGroupName)
		{
			var group = createdGroupsByNames[moveStudentInfo.ToGroupName];

			if (!joinedStudentsByName_withoutNamesake.TryGetValue(studentName, out var studentAsMember) || studentAsMember.GroupId == group.Id)
				continue;

			if (!usersToRemoveByGroupsIds.ContainsKey(studentAsMember.GroupId))
				usersToRemoveByGroupsIds.Add(studentAsMember.GroupId, new List<string>());

			if (!usersToAddByGroupsIds.ContainsKey(group.Id))
				usersToAddByGroupsIds.Add(group.Id, new List<string>());

			usersToRemoveByGroupsIds[studentAsMember.GroupId].Add(studentAsMember.UserId);
			usersToAddByGroupsIds[group.Id].Add(studentAsMember.UserId);

			moveResults.Add(new MoveUserInfo { UserId = studentAsMember.UserId, UserName = studentName, CurrentGroupId = group.Id, OldGroupId = studentAsMember.GroupId });
		}

		await Task.WhenAll(
			AddOrDeleteUsersFromGroups(usersToRemoveByGroupsIds, true),
			AddOrDeleteUsersFromGroups(usersToAddByGroupsIds)
		);

		return moveResults;
	}

	private async Task AddOrDeleteUsersFromGroups(Dictionary<int, List<string>> usersToUpdateByGroupsIds, bool toRemove = false)
	{
		var resortTasks = new List<Task>();

		foreach (var (groupId, userIds) in usersToUpdateByGroupsIds)
			resortTasks.Add(toRemove
				? groupMembersRepo.RemoveUsersFromGroupAsync(groupId, userIds)
				: groupMembersRepo.AddUsersToGroupAsync(groupId, userIds)
			);

		await Task.WhenAll(resortTasks);
	}

	private async Task<SuperGroupSheetExtractionResult> BuildSpreadSheetExtractionData(List<SingleGroup> createdGroups, (string groupName, string studentName)[] spreadSheetGroups)
	{
		var createdGroupsDictionary = createdGroups
			.ToDictionary(g => g.Name, g => g);

		var sheetGroupsNames = spreadSheetGroups
			.Select(g => g.groupName)
			.Distinct()
			.ToHashSet();

		var shouldBeDeletedGroups = createdGroups
			.Where(g => !sheetGroupsNames.Contains(g.Name))
			.ToList();
		var createdGroupsIds = createdGroups
			.Select(g => g.Id)
			.ToList();

		var allJoinedStudents = createdGroups.Count > 0
			? await groupMembersRepo.GetGroupsMembersAsync(createdGroupsIds)
			: new List<GroupMember>();

		List<GroupMemberInfo> GetJoinedStudents(int groupId)
		{
			return allJoinedStudents
				.Where(m => m.GroupId == groupId)
				.Select(m => GroupMemberInfo.BuildGroupMemberInfo(m, BuildShortUserInfo(m.User)))
				.ToList();
		}

		var shouldBeDeletedGroupsDictionary = shouldBeDeletedGroups
			.GroupBy(g => g.Name)
			.ToDictionary(
				g => g.Key,
				g =>
				{
					var groupId = g.FirstOrDefault().Id;
					return new SuperGroupItemInfo
					{
						NeededAction = SuperGroupItemActions.ShouldBeDeleted,
						JoinedStudents = GetJoinedStudents(groupId),
						GroupId = groupId,
					};
				}
			);

		var studentsByGroupName = spreadSheetGroups
			.GroupBy(g => g.groupName)
			.ToDictionary(
				g => g.Key,
				g =>
				{
					var isGroupCreated = createdGroupsDictionary.ContainsKey(g.Key);
					int? groupId = isGroupCreated ? createdGroupsDictionary[g.Key].Id : null;

					return new SuperGroupItemInfo
					{
						NeededAction = isGroupCreated
							? null
							: SuperGroupItemActions.ShouldBeCreated,
						StudentNames = g.Select(pair => pair.studentName).ToList(),
						JoinedStudents = isGroupCreated ? GetJoinedStudents(createdGroupsDictionary[g.Key].Id) : null,
						GroupId = groupId,
					};
				});

		var groupNamesByStudent = superGroupHelper.GetGroupsByUserName(spreadSheetGroups);
		var neededMoves = await GetMovesInSuperGroup(createdGroups, spreadSheetGroups);
		var validatingResults = new List<ValidatingResult>();

		if (groupNamesByStudent.Count > 0)
			validatingResults.Add(new GroupsHasSameStudents { SameNamesInGroups = groupNamesByStudent });

		if (neededMoves.Count > 0)
			validatingResults.Add(new StudentBelongsToOtherGroup { NeededMoves = neededMoves });

		return new SuperGroupSheetExtractionResult
		{
			Groups = studentsByGroupName
				.Concat(shouldBeDeletedGroupsDictionary)
				.ToDictionary(x => x.Key, x => x.Value),
			ValidatingResults = validatingResults,
		};
	}

	private async Task<Dictionary<string, MoveStudentInfo>> GetMovesInSuperGroup(List<SingleGroup> createdGroups, (string groupName, string studentName)[] spreadSheetGroups)
	{
		var studentNamesByGroupsName = new Dictionary<string, MoveStudentInfo>();
		var createdGroupsIds = createdGroups
			.Select(g => g.Id)
			.ToList();

		var createdGroupsById = createdGroups
			.ToDictionary(g => g.Id);
		var joinedStudents = await groupMembersRepo.GetGroupsMembersAsync(createdGroupsIds);

		var joinedStudentsByName_withoutNamesake = joinedStudents
			.GroupBy(m => m.User.VisibleName)
			.Where(m => m.Count() == 1)
			.Select(m => m.FirstOrDefault())
			.ToList();

		var sheetStudentsByName_withoutNamesake = spreadSheetGroups
			.GroupBy(p => p.studentName)
			.Where(g => g.Count() == 1)
			.Select(g => g.FirstOrDefault())
			.ToDictionary(p => p.studentName);

		foreach (var joinedStudent in joinedStudentsByName_withoutNamesake)
		{
			if (!sheetStudentsByName_withoutNamesake.TryGetValue(joinedStudent.User.VisibleName, out var pair)
				|| !createdGroupsById.TryGetValue(joinedStudent.GroupId, out var group)
				|| group.Name == pair.groupName)
				continue;

			studentNamesByGroupsName[pair.studentName] = new() { FromGroupName = group.Name, ToGroupName = pair.groupName };
		}

		return studentNamesByGroupsName;
	}
}