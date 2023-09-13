using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos.Groups
{
	public interface IGroupMembersRepo
	{
		Task<List<ApplicationUser>> GetGroupMembersAsUsersAsync(int groupId);
		Task<List<GroupMember>> GetGroupMembersAsync(int groupId);
		Task<List<GroupMember>> GetGroupsMembersAsync(ICollection<int> groupsIds);
		Task<List<string>> GetGroupsMembersIdsAsync(ICollection<int> groupsIds);
		Task<GroupMember> AddUserToGroupAsync(int groupId, string userId);
		Task<List<GroupMember>> AddUsersToGroupAsync(int groupId, ICollection<string> userIds);
		Task<GroupMember> RemoveUserFromGroupAsync(int groupId, string userId);
		Task<List<GroupMember>> RemoveUsersFromGroupAsync(int groupId, List<string> userIds);
		Task<List<string>> GetUsersIdsForAllCourseGroupsAsync(string courseId, bool includeArchived = false);
		Task<Dictionary<string, List<int>>> GetUsersGroupsIdsAsync(string courseId, List<string> usersIds, bool includeArchived = false);
		Task<List<int>> GetUserGroupsIdsAsync(string courseId, string userId, bool includeArchived = false);
		Task<List<SingleGroup>> GetUserGroupsAsync(string courseId, string userId, bool includeArchived = false);
		Task<List<(int GroupId, string UserId)>> GetGroupsMembersAsGroupsIdsAndUserIds(ICollection<int> groupIds);
		Task<bool> IsUserMemberOfGroup(int groupId, string userId);
		Task<List<SingleGroup>> GetUserSubGroups(int superGroupId, string userId);
		Task<Dictionary<string, List<SingleGroup>>> GetUsersGroupsAsync(string courseId, List<string> usersIds, bool includeArchived = false);
		Task<List<GroupBase>> GetUserGroupsAsync(string userId);
		Task<List<T>> GetUserGroupsAsync<T>(string userId) where T: GroupBase;
		Task<string> GetUserGroupsNamesAsString(string courseId, string userId, bool includeArchived = false);
	}
}