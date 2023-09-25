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
using Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models.Common.SuperGroup;
using Ulearn.Web.Api.Models.Parameters.Groups;
using Ulearn.Web.Api.Models.Parameters.Groups.SuperGroup;
using Ulearn.Web.Api.Models.Responses.Groups;
using Ulearn.Web.Api.Utils;
using Ulearn.Web.Api.Utils.SuperGroup;
using Vostok.Logging.Abstractions;
using GroupSettings = Ulearn.Web.Api.Models.Responses.Groups.GroupSettings;

namespace Ulearn.Web.Api.Controllers.Groups;

[Route("/super-group")]
public class SuperGroupController : BaseGroupController
{
	private readonly SuperGroupManager superGroupHelper;
	private readonly IGroupsRepo groupsRepo;
	private readonly IGroupAccessesRepo groupAccessesRepo;
	private readonly IGroupMembersRepo groupMembersRepo;
	private readonly IUnitsRepo unitsRepo;
	private readonly IManualCheckingsForOldSolutionsAdder manualCheckingsForOldSolutionsAdder;

	public SuperGroupController(
		ICourseStorage courseStorage,
		UlearnDb db,
		IUsersRepo usersRepo,
		IGroupsRepo groupsRepo,
		IGroupAccessesRepo groupAccessesRepo,
		IGroupMembersRepo groupMembersRepo,
		IUnitsRepo unitsRepo,
		SuperGroupManager superGroupHelper, IManualCheckingsForOldSolutionsAdder manualCheckingsForOldSolutionsAdder)
		: base(courseStorage, db, usersRepo)
	{
		this.superGroupHelper = superGroupHelper;
		this.manualCheckingsForOldSolutionsAdder = manualCheckingsForOldSolutionsAdder;
		this.groupsRepo = groupsRepo;
		this.groupAccessesRepo = groupAccessesRepo;
		this.groupMembersRepo = groupMembersRepo;
		this.unitsRepo = unitsRepo;
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
			context.Result = new ForbidResult("You have no access to this super-group");
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
		string errorMessage;
		try
		{
			var (createdGroups, spreadSheetGroups)
				= await superGroupHelper.GetSheetGroupsAndCreatedGroups(spreadsheetUrl, groupId, false);

			if (superGroup.DistributionTableLink != spreadsheetUrl)
				await groupsRepo.ModifyGroupAsync<SuperGroup>(groupId, new Database.Models.GroupSettings { DistributionTableLink = spreadsheetUrl });

			return await BuildSpreadSheetExtractionData(superGroup, createdGroups, spreadSheetGroups);
		}
		catch (GoogleApiException e)
		{
			errorMessage = $"Google Api ERROR: {e.Error.Code} {e.Error.Message}";
		}
		catch (GoogleSheetFormatException formatException)
		{
			return new SuperGroupSheetExtractionResult
			{
				Groups = new(),
				ValidatingResults = new List<ValidatingResult> { new InvalidSheetStructure { RawsIndexes = formatException.RawsIndexes } },
			};
		}
		catch (Exception e)
		{
			errorMessage = e.Message;
		}

		log.Warn($"Error while getting spread sheet from {spreadsheetUrl}, {errorMessage}");
		return BadRequest(errorMessage);
	}

	[HttpGet]
	[Route("{groupId}/scores")]
	[ProducesResponseType((int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.Forbidden)]
	[ProducesResponseType((int)HttpStatusCode.NotFound)]
	public async Task<ActionResult<GroupScoringGroupsResponse>> GetScores([FromRoute] int groupId)
	{
		var superGroup = await groupsRepo.FindGroupByIdAsync<SuperGroup>(groupId);
		var createdGroups = await groupsRepo.FindGroupsBySuperGroupIdAsync(groupId);
		var createdGroupsIds = createdGroups.Select(g => g.Id).ToHashSet();
		var course = courseStorage.FindCourse(superGroup.CourseId);
		if (course == null)
			return NotFound($"Course '{superGroup.CourseId}' not found");

		var scoringGroups = course.Settings.Scoring.Groups.Values.ToList();
		var visibleUnitIds = await unitsRepo.GetVisibleUnitIds(course, UserId);
		var scoringGroupsCanBeSetInSomeUnit = GetScoringGroupsCanBeSetInSomeUnit(course.GetUnits(visibleUnitIds));
		var enabledScoringGroups = await groupsRepo.GetEnabledAdditionalScoringGroupsForGroupsAsync(createdGroupsIds)
			.ConfigureAwait(false);

		return new GroupScoringGroupsResponse
		{
			Scores = scoringGroups
				.Select(scoringGroup => BuildGroupScoringGroupInfo(scoringGroup, scoringGroupsCanBeSetInSomeUnit, enabledScoringGroups, createdGroups.Count))
				.ToList(),
		};
	}

	[HttpPost]
	[Route("{groupId}/scores")]
	[ProducesResponseType((int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.Forbidden)]
	[ProducesResponseType((int)HttpStatusCode.NotFound)]
	public async Task<ActionResult<GroupScoringGroupsResponse>> SetScores([FromRoute] int groupId, [FromBody] SetScoringGroupsParameters parameters)
	{
		var hasEditAccess = await groupAccessesRepo.HasUserEditAccessToGroupAsync(groupId, UserId).ConfigureAwait(false);
		if (!hasEditAccess)
			return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You have no edit access to this group"));

		var superGroup = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
		var createdGroups = await groupsRepo.FindGroupsBySuperGroupIdAsync(groupId);
		var createdGroupsIds = createdGroups.Select(g => g.Id).ToHashSet();
		var course = courseStorage.FindCourse(superGroup.CourseId);
		if (course == null)
			return NotFound(new ErrorResponse("Group or course not found"));

		var courseScoringGroupIds = course.Settings.Scoring.Groups.Values.Select(g => g.Id).ToList();
		var visibleUnitIds = await unitsRepo.GetVisibleUnitIds(course, UserId);
		var scoringGroupsCanBeSetInSomeUnit = GetScoringGroupsCanBeSetInSomeUnit(course.GetUnits(visibleUnitIds)).Select(g => g.Id).ToList();
		foreach (var scoringGroupId in parameters.Scores)
		{
			if (!courseScoringGroupIds.Contains(scoringGroupId))
				return NotFound(new ErrorResponse($"Score {scoringGroupId} not found in course {course.Id}"));

			var scoringGroup = course.Settings.Scoring.Groups[scoringGroupId];
			if (scoringGroup.EnabledForEveryone)
				return BadRequest(new ErrorResponse($"You can not enable or disable additional scoring for {scoringGroupId}, because it is enabled for all groups by course creator."));

			if (!scoringGroupsCanBeSetInSomeUnit.Contains(scoringGroupId))
				return BadRequest(new ErrorResponse($"You can not enable or disable additional scoring for {scoringGroupId}, because there is no additional scores for this score in any unit. Contact with course creator."));
		}

		await groupsRepo.EnableAdditionalScoringGroupsForGroupAsync(createdGroupsIds, parameters.Scores).ConfigureAwait(false);

		return Ok(new SuccessResponseWithMessage($"Scores for sub-groups of {superGroup.Name} are updated"));
	}

	[HttpGet("{groupId}/settings")]
	[ProducesResponseType((int)HttpStatusCode.OK)]
	public async Task<ActionResult<ChangeableGroupSettings>> Settings([FromRoute] int groupId)
	{
		var createdGroups = await groupsRepo.FindGroupsBySuperGroupIdAsync(groupId);
		return BuildSuperGroupSettings(createdGroups);
	}

	[HttpPost("{groupId}/settings")]
	[ProducesResponseType((int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.Forbidden)]
	[ProducesResponseType((int)HttpStatusCode.NotFound)]
	public async Task<ActionResult<GroupSettings>> UpdateGroup([FromRoute] int groupId, [FromBody] ChangeableGroupSettings parameters)
	{
		var hasEditAccess = await groupAccessesRepo.HasUserEditAccessToGroupAsync(groupId, UserId).ConfigureAwait(false);
		if (!hasEditAccess)
			return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You have no edit access to this group"));

		var settings = new Database.Models.GroupSettings
		{
			NewIsManualCheckingEnabled = parameters.IsManualCheckingEnabled,
			NewIsManualCheckingEnabledForOldSolutions = parameters.IsManualCheckingEnabledForOldSolutions,
			NewDefaultProhibitFurtherReview = parameters.DefaultProhibitFurtherReview,
			NewCanUsersSeeGroupProgress = parameters.CanStudentsSeeGroupProgress
		};

		await groupsRepo.ModifySubGroupsAsync(groupId, settings).ConfigureAwait(false);

		return Ok(new SuccessResponseWithMessage($"Settings for sub-groups of super-group with id {groupId} are updated"));
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
	public async Task<ActionResult<SuperGroupMoveUserResponse>> ResortStudents([FromQuery] int groupId, [FromBody] Dictionary<string, MoveStudentInfo> moves)
	{
		var superGroup = await groupsRepo.FindGroupByIdAsync<SuperGroup>(groupId);
		if (superGroup.DistributionTableLink == null)
			return BadRequest("Auto-group doesn't have a correct link to google sheet table");

		var createdGroups = await groupsRepo.FindGroupsBySuperGroupIdAsync(superGroup.Id, true);

		var createdGroupsByNames = createdGroups.ToDictionary(g => g.Name, g => g);

		var notCreatedGroups = moves
			.SelectMany(p => new[] { p.Value.FromGroupName, p.Value.ToGroupName })
			.Distinct()
			.Where(groupName => groupName is not null && !createdGroupsByNames.ContainsKey(groupName))
			.ToList();

		if (notCreatedGroups.Count > 0)
			return BadRequest($"Can't sort users in not created groups ({string.Join(", ", notCreatedGroups)}), please create these groups first");

		var movedUsers = await MoveUsers(superGroup, createdGroupsByNames, moves);

		return Ok(new SuperGroupMoveUserResponse { MovedUsers = movedUsers });
	}

	private async Task<List<MoveUserInfo>> MoveUsers(
		SuperGroup superGroup,
		Dictionary<string, SingleGroup> createdGroupsByNames,
		Dictionary<string, MoveStudentInfo> usersToMoveByGroupName
	)
	{
		var createdGroupsIds = createdGroupsByNames
			.Values
			.Select(g => g.Id)
			.Append(superGroup.Id)
			.ToList();

		var joinedStudents = await groupMembersRepo.GetGroupsMembersAsync(createdGroupsIds);

		var joinedStudentsByName_withoutNamesake = joinedStudents
			.Where(m => m.User.FirstName is not null && m.User.LastName is not null)
			.GroupBy(m => $"{m.User.FirstName.Trim()} {m.User.LastName.Trim()}".Replace('ё', 'е'), StringComparer.OrdinalIgnoreCase)
			.Where(m => m.Count() == 1)
			.Select(m => m.Single())
			.SelectMany(
				member => new[]
				{
					(member, name: $"{member.User.FirstName.Trim()} {member.User.LastName.Trim()}".Replace('ё', 'е')),
					(member, name: $"{member.User.LastName.Trim()} {member.User.FirstName.Trim()}".Replace('ё', 'е'))
				}
			)
			.Distinct()
			.ToDictionary(
				e => e.name,
				e => e.member,
				StringComparer.OrdinalIgnoreCase
			);

		var moveResults = new List<MoveUserInfo>();
		var usersToRemoveByGroupsIds = new Dictionary<int, List<string>>();
		var usersToAddByGroupsIds = new Dictionary<int, List<string>>();

		foreach (var (studentName, moveStudentInfo) in usersToMoveByGroupName)
		{
			var group = createdGroupsByNames[moveStudentInfo.ToGroupName];

			if (!joinedStudentsByName_withoutNamesake.TryGetValue(studentName.Trim().Replace('ё', 'е'), out var member) || member.GroupId == group.Id)
				continue;

			if (!usersToRemoveByGroupsIds.ContainsKey(member.GroupId))
				usersToRemoveByGroupsIds.Add(member.GroupId, new List<string>());

			if (!usersToAddByGroupsIds.ContainsKey(group.Id))
				usersToAddByGroupsIds.Add(group.Id, new List<string>());

			usersToRemoveByGroupsIds[member.GroupId].Add(member.UserId);
			usersToAddByGroupsIds[group.Id].Add(member.UserId);

			moveResults.Add(new MoveUserInfo { UserId = member.UserId, UserName = studentName, CurrentGroupId = group.Id, OldGroupId = member.GroupId });
		}

		var usersToDelete = usersToRemoveByGroupsIds
			.SelectMany(
				kvp => kvp.Value,
				(kvp, userId) => $"{kvp.Key}_{userId}"
			)
			.ToHashSet();

		var membersToDelete = joinedStudents
			.Where(m => usersToDelete.Contains($"{m.GroupId}_{m.UserId}"))
			.ToArray();

		var membersToAdd = usersToAddByGroupsIds
			.SelectMany(
				kvp => kvp.Value,
				(kvp, userId) => new GroupMember
				{
					GroupId = kvp.Key,
					UserId = userId,
					AddingTime = DateTime.Now
				}
			)
			.ToArray();

		var usersWithManualCheckingForOldSolutions = createdGroupsByNames.Values
			.Where(g => g.IsManualCheckingEnabledForOldSolutions)
			.GroupJoin(
				membersToAdd,
				g => g.Id,
				m => m.GroupId,
				(_, members) => members
			)
			.SelectMany(m => m)
			.Select(m => m.UserId);

		await ApplyMembersMoves(superGroup.CourseId, membersToDelete, membersToAdd, usersWithManualCheckingForOldSolutions);

		return moveResults;
	}

	private async Task ApplyMembersMoves(
		string courseId,
		GroupMember[] membersToDelete,
		GroupMember[] membersToAdd,
		IEnumerable<string> usersWithManualCheckingForOldSolutions
	)
	{
		await using var transaction = await db.Database.BeginTransactionAsync();

		db.GroupMembers.RemoveRange(membersToDelete);
		await db.GroupMembers.AddRangeAsync(membersToAdd);
		await manualCheckingsForOldSolutionsAdder.AddManualCheckingsForOldSolutionsAsync(courseId, usersWithManualCheckingForOldSolutions);
		await db.SaveChangesAsync();
		await transaction.CommitAsync();
	}

	private async Task<SuperGroupSheetExtractionResult> BuildSpreadSheetExtractionData(SuperGroup superGroup, List<SingleGroup> createdGroups, (string groupName, string studentName)[] spreadSheetGroups)
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

		var shouldBeDeletedGroupsDictionary = shouldBeDeletedGroups
			.GroupBy(g => g.Name)
			.ToDictionary(
				g => g.Key,
				g =>
				{
					var groupId = g.First().Id;
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
				}
			);

		var groupNamesByStudent = superGroupHelper.GetGroupsByUserName(spreadSheetGroups);
		var neededMoves = await GetMovesInSuperGroup(superGroup, createdGroups, spreadSheetGroups);
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

		List<GroupMemberInfo> GetJoinedStudents(int groupId)
		{
			return allJoinedStudents
				.Where(m => m.GroupId == groupId)
				.Select(m => GroupMemberInfo.BuildGroupMemberInfo(m, BuildShortUserInfo(m.User)))
				.ToList();
		}
	}

	private async Task<Dictionary<string, MoveStudentInfo>> GetMovesInSuperGroup(SuperGroup superGroup, List<SingleGroup> createdGroups, (string groupName, string studentName)[] spreadSheetGroups)
	{
		var studentNamesByGroupsName = new Dictionary<string, MoveStudentInfo>();

		var createdGroupsById = createdGroups
			.Cast<GroupBase>()
			.Append(superGroup)
			.ToDictionary(g => g.Id);

		var createdGroupsIds = createdGroupsById.Keys;

		var joinedStudents = await groupMembersRepo.GetGroupsMembersAsync(createdGroupsIds);

		var sheetGroupByStudentName_withoutNamesake = spreadSheetGroups
			.GroupBy(p => p.studentName.Trim().Replace('ё', 'е'), StringComparer.OrdinalIgnoreCase)
			.Where(g => g.Count() == 1)
			.Select(g => g.Single())
			.ToDictionary(
				p => p.studentName.Trim().Replace('ё', 'е'),
				p => p.groupName,
				StringComparer.OrdinalIgnoreCase
			);

		var joinedStudents_withoutNamesake = joinedStudents
			.Where(m => m.User.FirstName is not null && m.User.LastName is not null)
			.GroupBy(m => $"{m.User.FirstName.Trim()} {m.User.LastName.Trim()}".Replace('ё', 'е'), StringComparer.OrdinalIgnoreCase)
			.Where(m => m.Count() == 1)
			.Select(m => m.Single());

		foreach (var member in joinedStudents_withoutNamesake)
		{
			if (
				(
					!sheetGroupByStudentName_withoutNamesake
						.TryGetValue($"{member.User.FirstName.Trim()} {member.User.LastName.Trim()}".Replace('ё', 'е'), out var groupName) &&
					!sheetGroupByStudentName_withoutNamesake
						.TryGetValue($"{member.User.LastName.Trim()} {member.User.FirstName.Trim()}".Replace('ё', 'е'), out groupName)
				)
				|| !createdGroupsById.TryGetValue(member.GroupId, out var group)
				|| group.Name == groupName)
				continue;

			studentNamesByGroupsName[member.User.VisibleName] = new MoveStudentInfo
			{
				FromGroupName = group.GroupType == GroupType.SingleGroup ? group.Name : null,
				ToGroupName = groupName
			};
		}

		return studentNamesByGroupsName;
	}

	private ChangeableGroupSettings BuildSuperGroupSettings(List<SingleGroup> groupsSettings)
	{
		var isManualCheckingNull = groupsSettings.Any(gs => gs.IsManualCheckingEnabled != groupsSettings[0].IsManualCheckingEnabled);
		var isManualCheckingNullForOldSolutions = groupsSettings.Any(gs => gs.IsManualCheckingEnabledForOldSolutions != groupsSettings[0].IsManualCheckingEnabledForOldSolutions);
		var nullProhibitFurtherReview = groupsSettings.Any(gs => gs.DefaultProhibitFutherReview != groupsSettings[0].DefaultProhibitFutherReview);
		var canStudentsSeeGroupProgressIsNull = groupsSettings.Any(gs => gs.CanUsersSeeGroupProgress != groupsSettings[0].CanUsersSeeGroupProgress);

		return new ChangeableGroupSettings
		{
			IsManualCheckingEnabled = isManualCheckingNull ? null : groupsSettings[0].IsManualCheckingEnabled,
			IsManualCheckingEnabledForOldSolutions = isManualCheckingNullForOldSolutions ? null : groupsSettings[0].IsManualCheckingEnabledForOldSolutions,
			DefaultProhibitFurtherReview = nullProhibitFurtherReview ? null : groupsSettings[0].DefaultProhibitFutherReview,
			CanStudentsSeeGroupProgress = canStudentsSeeGroupProgressIsNull ? null : groupsSettings[0].CanUsersSeeGroupProgress,
		};
	}
}