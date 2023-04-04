using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Ulearn.Core.Courses;

namespace Database.Repos.Groups
{
	public interface IGroupsRepo
	{
		Task<GroupBase> CreateGroupAsync(
			string courseId,
			string name,
			string ownerId,
			GroupType groupType,
			bool isManualCheckingEnabled = false,
			bool isManualCheckingEnabledForOldSolutions = false,
			bool canUsersSeeGroupProgress = true,
			bool defaultProhibitFurtherReview = true,
			bool isInviteLinkEnabled = true);

		Task<SingleGroup> CreateSingleGroupAsync(string courseId,
			string name,
			string ownerId,
			int? superGroupId = null,
			bool isManualCheckingEnabled = false,
			bool isManualCheckingEnabledForOldSolutions = false,
			bool canUsersSeeGroupProgress = true,
			bool defaultProhibitFurtherReview = true,
			bool isInviteLinkEnabled = true);

		Task<SingleGroup> CopyGroupAsync(SingleGroup group, string courseId, string newOwnerId = "");
		Task<GroupBase> ModifyGroupAsync(int groupId, GroupSettings settings);
		Task<T> ModifyGroupAsync<T>(int groupId, GroupSettings newSettings) where T : GroupBase;
		Task ChangeGroupOwnerAsync(int groupId, string newOwnerId);
		Task<GroupBase> ArchiveGroupAsync(int groupId, bool isArchived);
		Task DeleteGroupAsync(int groupId);
		Task<T> FindGroupByIdAsync<T>(int groupId) where T : GroupBase;
		Task<GroupBase> FindGroupByIdAsync(int groupId);
		Task<List<SingleGroup>> FindGroupsBySuperGroupIdAsync(int superGroupId, bool includeArchived = false);
		Task<GroupBase> FindGroupByInviteHashAsync(Guid hash);
		Task<GroupBase> FindGroupByInviteHashAsync_WithDisabledLink(Guid hash);
		Task<List<GroupBase>> GetCourseGroupsAsync(string courseId, GroupQueryType groupType, bool includeArchived = false);
		Task<List<GroupBase>> GetMyGroupsFilterAccessibleToUserAsync(string courseId, string userId, bool includeArchived = false);
		Task EnableInviteLinkAsync(int groupId, bool isEnabled);
		Task<bool> IsManualCheckingEnabledForUserAsync(Course course, string userId);
		Task<bool> GetDefaultProhibitFurtherReviewForUserAsync(string courseId, string userId, string instructorId);
		Task EnableAdditionalScoringGroupsForGroupAsync(int groupId, IEnumerable<string> scoringGroupsIds);
		Task<List<EnabledAdditionalScoringGroup>> GetEnabledAdditionalScoringGroupsAsync(string courseId, bool includeArchived = false);
		Task<List<EnabledAdditionalScoringGroup>> GetEnabledAdditionalScoringGroupsForGroupAsync(int groupId);
		IQueryable<GroupBase> GetCourseGroupsQueryable(string courseId, GroupQueryType groupType, bool includeArchived = false);
		Task<HashSet<string>> GetUsersIdsWithEnabledManualChecking(Course course, List<string> userIds);
		Task<List<string>> GetUsersIdsForAllGroups(string courseId);
		Task<List<GroupBase>> GetMyGroupsFilterAccessibleToUser(string courseId, string userId, bool includeArchived = false);
		Task<List<string>> GetGroupsMembersAsUserIds(IEnumerable<int> groupIds);
		Task<IEnumerable<(int GroupId, string UserId)>> GetGroupsMembersAsGroupsIdsAndUserIds(IEnumerable<int> groupIds);
		List<ApplicationUser> GetGroupsMembersAsUsers(IEnumerable<int> groupIds);
		Task<bool> GetDefaultProhibitFutherReviewForUser(string courseId, string userId, string instructorId);
	}
}