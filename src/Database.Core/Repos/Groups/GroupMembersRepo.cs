using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Vostok.Logging.Abstractions;
using Ulearn.Common.Extensions;

namespace Database.Repos.Groups
{
	public class GroupMembersRepo : IGroupMembersRepo
	{
		private readonly UlearnDb db;
		private readonly IManualCheckingsForOldSolutionsAdder manualCheckingsForOldSolutionsAdder;
		private readonly IGroupsRepo groupsRepo;
		private readonly IGroupAccessesRepo groupAccessesRepo;
		private static ILog log => LogProvider.Get().ForContext(typeof(GroupMembersRepo));

		public GroupMembersRepo(UlearnDb db, IManualCheckingsForOldSolutionsAdder manualCheckingsForOldSolutionsAdder,
			IGroupsRepo groupsRepo)
		{
			this.db = db;
			this.manualCheckingsForOldSolutionsAdder = manualCheckingsForOldSolutionsAdder;
			this.groupsRepo = groupsRepo;
		}

		public async Task<List<ApplicationUser>> GetGroupMembersAsUsersAsync(int groupId)
		{
			return await db.GroupMembers.Include(m => m.User).Where(m => m.GroupId == groupId && !m.User.IsDeleted).Select(m => m.User).ToListAsync();
		}

		public async Task<List<GroupMember>> GetGroupMembersAsync(int groupId)
		{
			return await db.GroupMembers.Include(m => m.User).Where(m => m.GroupId == groupId && !m.User.IsDeleted).ToListAsync();
		}

		public async Task<List<GroupMember>> GetGroupsMembersAsync(ICollection<int> groupsIds)
		{
			if (!groupsIds.Any())
				return new List<GroupMember>();
			return await db.GroupMembers.Include(m => m.User).Where(m => groupsIds.Contains(m.GroupId) && !m.User.IsDeleted).ToListAsync();
		}

		public async Task<List<string>> GetGroupsMembersIdsAsync(ICollection<int> groupsIds)
		{
			if (!groupsIds.Any())
				return new List<string>();
			return await db.GroupMembers.Include(m => m.User).Where(m => groupsIds.Contains(m.GroupId) && !m.User.IsDeleted).Select(u => u.UserId).ToListAsync();
		}

		public async Task<List<(int GroupId, string UserId)>> GetGroupsMembersAsGroupsIdsAndUserIds(ICollection<int> groupIds)
		{
			return (await db.GroupMembers.Include(m => m.User).Where(m => !m.User.IsDeleted && groupIds.Contains(m.GroupId))
					.Select(m => new { m.GroupId, m.UserId })
					.ToListAsync())
				.Select(m => (m.GroupId, m.UserId)).ToList();
		}

		public async Task<bool> IsUserMemberOfGroup(int groupId, string userId)
		{
			return await db.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId);
		}

		[ItemCanBeNull]
		public async Task<GroupMember> AddUserToGroupAsync(int groupId, string userId)
		{
			log.Info($"Пытаюсь добавить пользователя {userId} в группу {groupId}");
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false) ?? throw new ArgumentNullException($"Can't find group with id={groupId}");
			if (group.GroupType != GroupType.SingleGroup)
				throw new ArgumentException($"Group {groupId} isn't single group");
			
			var groupMember = new GroupMember
			{
				GroupId = groupId,
				UserId = userId,
				AddingTime = DateTime.Now,
			};
			using (var transaction = db.Database.BeginTransaction())
			{
				/* Don't add member if it's already exists */
				var existsMember = db.GroupMembers.FirstOrDefault(m => m.GroupId == groupId && m.UserId == userId);
				if (existsMember != null)
				{
					log.Info($"Пользователь {userId} уже находится в группе {groupId}, повторно добавлять не буду");
					return null;
				}

				db.GroupMembers.Add(groupMember);
				await db.SaveChangesAsync().ConfigureAwait(false);

				transaction.Commit();

				log.Info($"Пользователь {userId} добавлен в группу {groupId}");
			}

			if ((group as SingleGroup).IsManualCheckingEnabledForOldSolutions)
				await manualCheckingsForOldSolutionsAdder.AddManualCheckingsForOldSolutionsAsync(group.CourseId, userId).ConfigureAwait(false);

			return groupMember;
		}

		public async Task<GroupMember> RemoveUserFromGroupAsync(int groupId, string userId)
		{
			log.Info($"Удаляю пользователя {userId} из группы {groupId}");

			var member = db.GroupMembers.FirstOrDefault(m => m.GroupId == groupId && m.UserId == userId);
			if (member != null)
				db.GroupMembers.Remove(member);
			else
				log.Info($"Пользователь {userId} не состоит в группе {groupId}");

			await db.SaveChangesAsync().ConfigureAwait(false);

			return member;
		}

		public async Task<List<GroupMember>> RemoveUsersFromGroupAsync(int groupId, List<string> userIds)
		{
			log.Info($"Удаляю пользователей {string.Join(", ", userIds)} из группы {groupId}");

			var members = db.GroupMembers.Where(m => m.GroupId == groupId && userIds.Contains(m.UserId)).ToList();
			db.GroupMembers.RemoveRange(members);

			await db.SaveChangesAsync().ConfigureAwait(false);

			return members;
		}

		public async Task<List<GroupMember>> AddUsersToGroupAsync(int toGroupId, ICollection<string> userIds)
		{
			log.Info($"Добавляю пользователей {string.Join(", ", userIds)} в группу {toGroupId}");

			var newMembers = new List<GroupMember>();
			foreach (var memberUserId in userIds)
			{
				var newMember = await AddUserToGroupAsync(toGroupId, memberUserId).ConfigureAwait(false);
				if (newMember != null)
					newMembers.Add(newMember);
			}

			return newMembers;
		}

		public Task<List<string>> GetUsersIdsForAllCourseGroupsAsync(string courseId, bool includeArchived = false)
		{
			var groupsIds = groupsRepo.GetCourseGroupsQueryable(courseId, GroupQueryType.SingleGroup, includeArchived).Select(g => g.Id);
			return db.GroupMembers.Where(m => groupsIds.Contains(m.GroupId)).Select(m => m.UserId).ToListAsync();
		}

		/* Return Dictionary<userId, List<groupId>> */
		public async Task<Dictionary<string, List<int>>> GetUsersGroupsIdsAsync(string courseId, List<string> usersIds, bool includeArchived = false)
		{
			var groupsIds = groupsRepo.GetCourseGroupsQueryable(courseId, GroupQueryType.SingleGroup, includeArchived).Select(g => g.Id);
			return (await db.GroupMembers
					.Where(m => groupsIds.Contains(m.GroupId) && usersIds.Contains(m.UserId))
					.Select(m => new { m.UserId, m.GroupId })
					.ToListAsync())
				.GroupBy(m => m.UserId)
				.ToDictionary(g => g.Key, g => g.Select(m => m.GroupId).ToList());
		}

		public async Task<List<int>> GetUserGroupsIdsAsync(string courseId, string userId, bool includeArchived = false)
		{
			return (await GetUsersGroupsIdsAsync(courseId, new List<string> { userId }, includeArchived).ConfigureAwait(false))
				.GetOrDefault(userId, new List<int>());
		}

		public async Task<List<SingleGroup>> GetUserGroupsAsync(string courseId, string userId, bool includeArchived = false)
		{
			var userGroupsIds = await GetUserGroupsIdsAsync(courseId, userId).ConfigureAwait(false);
			return (await groupsRepo
				.GetCourseGroupsQueryable(courseId, GroupQueryType.SingleGroup, includeArchived)
				.Where(g => userGroupsIds.Contains(g.Id))
				.ToListAsync())
				.AsGroups()
				.ToList();
		}

		public async Task<string> GetUserGroupsNamesAsString(string courseId, string userId, bool includeArchived = false)
		{
			var usersGroups = await GetUserGroupsAsync(courseId, userId, includeArchived);
			return string.Join(", ", usersGroups.Select(g => g.Name));
		}

		public async Task<List<SingleGroup>> GetUserGroupsAsync(string userId)
		{
			return await db.GroupMembers.Where(m => m.UserId == userId && !m.Group.IsDeleted).Select(m => m.Group).ToListAsync();
		}

		public async Task<Dictionary<string, List<SingleGroup>>> GetUsersGroupsAsync(string courseId, List<string> usersIds, bool includeArchived = false)
		{
			var userGroupsIds = await GetUsersGroupsIdsAsync(courseId, usersIds, includeArchived).ConfigureAwait(false);
			var ids = userGroupsIds.Values.SelectMany(g => g).Distinct().ToList();
			var groups = groupsRepo.GetCourseGroupsQueryable(courseId, GroupQueryType.SingleGroup, includeArchived).Where(g => ids.Contains(g.Id)).ToDictionary(g => g.Id, g => (SingleGroup)g);
			return userGroupsIds.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(id => groups[id]).ToList());
		}
	}
}