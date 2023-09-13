﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos.Users;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Ulearn.Core.Courses;

namespace Database.Repos.Groups
{
	public class GroupsRepo : IGroupsRepo
	{
		private readonly UlearnDb db;
		private readonly IUsersRepo usersRepo;
		private readonly ICourseRolesRepo courseRolesRepo;
		private readonly IGroupsCreatorAndCopier groupsCreatorAndCopier;
		private readonly IManualCheckingsForOldSolutionsAdder manualCheckingsForOldSolutionsAdder;

		public GroupsRepo(
			UlearnDb db,
			IGroupsCreatorAndCopier groupsCreatorAndCopier,
			IManualCheckingsForOldSolutionsAdder manualCheckingsForOldSolutionsAdder,
			IUsersRepo usersRepo,
			ICourseRolesRepo courseRolesRepo
		)
		{
			this.db = db;
			this.groupsCreatorAndCopier = groupsCreatorAndCopier;
			this.manualCheckingsForOldSolutionsAdder = manualCheckingsForOldSolutionsAdder;
			this.usersRepo = usersRepo;
			this.courseRolesRepo = courseRolesRepo;
		}

		public async Task<SingleGroup> CreateSingleGroupAsync(string courseId,
			string name,
			string ownerId,
			int? superGroupId = null,
			bool isManualCheckingEnabled = false,
			bool isManualCheckingEnabledForOldSolutions = false,
			bool canUsersSeeGroupProgress = true,
			bool defaultProhibitFurtherReview = true,
			bool isInviteLinkEnabled = true)
		{
			return await groupsCreatorAndCopier.CreateSingleGroupAsync(
				courseId,
				name,
				ownerId,
				superGroupId,
				isManualCheckingEnabled,
				isManualCheckingEnabledForOldSolutions,
				canUsersSeeGroupProgress,
				defaultProhibitFurtherReview,
				isInviteLinkEnabled
			);
		}

		public Task<List<SingleGroup>> SearchGroups(GroupsSearchQueryModel queryModel, bool instructorIdExcluded = false)
		{
			var query = db.SingleGroups
				.Where(g => !g.IsDeleted);

			if (queryModel.CourseId is not null)
				query = query
					.Where(g => g.CourseId == queryModel.CourseId);

			if (!queryModel.IncludeArchived)
				query = query
					.Where(g => !g.IsArchived);

			if (queryModel.InstructorId is not null)
			{
				var accessesQuery = query
					.GroupJoin(
						db.GroupAccesses,
						g => g.Id,
						ga => ga.GroupId,
						(group, accesses) => new { group, accesses }
					)
					.SelectMany(
						e => e.accesses.DefaultIfEmpty(),
						(e, access) => new { e.group, access }
					);
				accessesQuery = instructorIdExcluded
					? accessesQuery
						.Where(e =>
							e.group.OwnerId != queryModel.InstructorId &&
							(e.access.UserId != queryModel.InstructorId || !e.access.IsEnabled)
						)
					: accessesQuery
						.Where(e =>
							e.group.OwnerId == queryModel.InstructorId ||
							(e.access.UserId == queryModel.InstructorId && e.access.IsEnabled)
						);
				query = accessesQuery.Select(e => e.group);
			}

			if (queryModel.MemberId is not null)
				query = query
					.GroupJoin(
						db.GroupMembers,
						g => g.Id,
						gm => gm.GroupId,
						(group, members) => new { group, members }
					)
					.SelectMany(
						e => e.members,
						(e, member) => new { e.group, member }
					)
					.Where(e => e.member.UserId == queryModel.MemberId)
					.Select(e => e.group);

			queryModel.Query = queryModel.Query?.ToLower();
			if (!string.IsNullOrEmpty(queryModel.Query))
				query = query
					.Where(g => g.Name.ToLower().Contains(queryModel.Query));

			query = query
				.Distinct()
				.OrderBy(g => g.IsArchived)
				.ThenBy(g => g.Name);

			if (queryModel.Offset > 0)
				query = query
					.Skip(queryModel.Offset);

			if (queryModel.Count > 0)
				query = query
					.Take(queryModel.Count);

			return query
				.Include(g => g.Owner)
				.Include(g => g.Members)
				.ToListAsync();
		}

		public async Task<GroupBase> CreateGroupAsync(string courseId,
			string name,
			string ownerId,
			GroupType groupType,
			bool isManualCheckingEnabled = false,
			bool isManualCheckingEnabledForOldSolutions = false,
			bool canUsersSeeGroupProgress = true,
			bool defaultProhibitFurtherReview = true,
			bool isInviteLinkEnabled = true)
		{
			if (groupType == GroupType.SingleGroup)
				return await groupsCreatorAndCopier.CreateGroupAsync(
					courseId,
					name,
					ownerId,
					isManualCheckingEnabled,
					isManualCheckingEnabledForOldSolutions,
					canUsersSeeGroupProgress,
					defaultProhibitFurtherReview,
					isInviteLinkEnabled
				);

			if (groupType == GroupType.SuperGroup)
				return await groupsCreatorAndCopier.CreateSuperGroupAsync(
					courseId,
					name,
					ownerId,
					isInviteLinkEnabled
				);

			throw new ArgumentException($"Unknown group type {groupType}");
		}

		/* Copy group from one course to another. Replace owner only if newOwnerId is not empty */
		public Task<SingleGroup> CopyGroupAsync(SingleGroup group, string courseId, string newOwnerId = "")
		{
			return groupsCreatorAndCopier.CopyGroupAsync(group, courseId, newOwnerId);
		}

		public async Task<T> ModifyGroupAsync<T>(int groupId, GroupSettings newSettings) where T : GroupBase
		{
			var group = await FindGroupByIdAsync<T>(groupId).ConfigureAwait(false);
			group.Name = newSettings.NewName ?? group.Name;

			switch (group)
			{
				case SingleGroup singleGroup:
				{
					singleGroup.IsManualCheckingEnabled = newSettings.NewIsManualCheckingEnabled ?? singleGroup.IsManualCheckingEnabled;

					if (!singleGroup.IsManualCheckingEnabledForOldSolutions && newSettings.NewIsManualCheckingEnabledForOldSolutions == true)
					{
						var groupMembers = singleGroup.NotDeletedMembers.Select(m => m.UserId).ToList();
						await manualCheckingsForOldSolutionsAdder.AddManualCheckingsForOldSolutionsAsync(singleGroup.CourseId, groupMembers).ConfigureAwait(false);
					}

					singleGroup.IsManualCheckingEnabledForOldSolutions = newSettings.NewIsManualCheckingEnabledForOldSolutions ?? singleGroup.IsManualCheckingEnabledForOldSolutions;
					singleGroup.DefaultProhibitFutherReview = newSettings.NewDefaultProhibitFurtherReview ?? singleGroup.DefaultProhibitFutherReview;
					singleGroup.CanUsersSeeGroupProgress = newSettings.NewCanUsersSeeGroupProgress ?? singleGroup.CanUsersSeeGroupProgress;
					singleGroup.SuperGroupId = newSettings.SuperGroupId ?? singleGroup.SuperGroupId;

					break;
				}
				case SuperGroup superGroup:
					superGroup.DistributionTableLink = newSettings.DistributionTableLink ?? superGroup.DistributionTableLink;

					break;
			}

			db.Set<T>().Update(group);
			await db.SaveChangesAsync().ConfigureAwait(false);
			return group;
		}

		public async Task ModifySubGroupsAsync(int superGroupId, GroupSettings newSettings)
		{
			var groups = await FindGroupsBySuperGroupIdAsync(superGroupId);

			foreach (var group in groups)
			{
				group.IsManualCheckingEnabled = newSettings.NewIsManualCheckingEnabled ?? group.IsManualCheckingEnabled;

				if (!group.IsManualCheckingEnabledForOldSolutions && newSettings.NewIsManualCheckingEnabledForOldSolutions == true)
				{
					var groupMembers = group.NotDeletedMembers.Select(m => m.UserId).ToList();
					await manualCheckingsForOldSolutionsAdder.AddManualCheckingsForOldSolutionsAsync(group.CourseId, groupMembers).ConfigureAwait(false);
				}

				group.IsManualCheckingEnabledForOldSolutions = newSettings.NewIsManualCheckingEnabledForOldSolutions ?? group.IsManualCheckingEnabledForOldSolutions;
				group.DefaultProhibitFutherReview = newSettings.NewDefaultProhibitFurtherReview ?? group.DefaultProhibitFutherReview;
				group.CanUsersSeeGroupProgress = newSettings.NewCanUsersSeeGroupProgress ?? group.CanUsersSeeGroupProgress;
			}

			db.SingleGroups.UpdateRange(groups);
			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task<GroupBase> ModifyGroupAsync(int groupId, GroupSettings newSettings)
		{
			return await ModifyGroupAsync<GroupBase>(groupId, newSettings);
		}

		public async Task ChangeGroupOwnerAsync(int groupId, string newOwnerId)
		{
			var group = await FindGroupByIdAsync(groupId).ConfigureAwait(false) ?? throw new ArgumentNullException($"Can't find group with id={groupId}");
			group.OwnerId = newOwnerId;

			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task<GroupBase> ArchiveGroupAsync(int groupId, bool isArchived)
		{
			var group = await FindGroupByIdAsync(groupId).ConfigureAwait(false) ?? throw new ArgumentNullException($"Can't find group with id={groupId}");
			group.IsArchived = isArchived;
			if (!isArchived)
				group.CreateTime = DateTime.Now; // Обновляем при восстановлении, чтобы автоархивация эту группу снова не съела.

			await db.SaveChangesAsync().ConfigureAwait(false);
			return group;
		}

		public async Task EnableInviteLinkAsync(int groupId, bool isEnabled)
		{
			var group = await FindGroupByIdAsync(groupId).ConfigureAwait(false) ?? throw new ArgumentNullException($"Can't find group with id={groupId}");
			group.IsInviteLinkEnabled = isEnabled;

			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task DeleteGroupAsync(int groupId)
		{
			var group = await FindGroupByIdAsync(groupId).ConfigureAwait(false);

			/* Maybe group is already deleted */
			if (group == null)
				return;

			group.IsDeleted = true;

			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		[ItemCanBeNull]
		public Task<T> FindGroupByIdAsync<T>(int groupId) where T : GroupBase
		{
			return db.Set<T>().FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted);
		}

		[ItemCanBeNull]
		public Task<List<T>> FindGroupsByIdsAsync<T>(List<int> groupIds) where T : GroupBase
		{
			return db.Set<T>().Where(g => groupIds.Contains(g.Id) && !g.IsDeleted).ToListAsync();
		}

		[ItemCanBeNull]
		public Task<GroupBase> FindGroupByIdAsync(int groupId)
		{
			return FindGroupByIdAsync<GroupBase>(groupId);
		}


		public Task<bool> IsGroupExist<T>(int groupId) where T : GroupBase
		{
			return db.Set<T>().AnyAsync(g => g.Id == groupId && !g.IsDeleted);
		}

		public Task<bool> IsGroupExist(int groupId)
		{
			return IsGroupExist<GroupBase>(groupId);
		}


		[ItemCanBeNull]
		public async Task<List<SingleGroup>> FindGroupsBySuperGroupIdAsync(int superGroupId, bool includeArchived = false)
		{
			return await FindGroupsBySuperGroupIdsAsync(new List<int> { superGroupId }, includeArchived);
		}

		[ItemCanBeNull]
		public async Task<List<SingleGroup>> FindGroupsBySuperGroupIdsAsync(List<int> superGroupIds, bool includeArchived = false)
		{
			var query = db
				.Set<SingleGroup>()
				.Where(g => superGroupIds.Contains(g.SuperGroupId.Value) && !g.IsDeleted);

			if (!includeArchived)
				query = query.Where(g => g.IsArchived == false);

			return await query
				.Include(g => g.Members)
				.ThenInclude(m => m.User)
				.ToListAsync();
		}

		[ItemCanBeNull]
		public Task<GroupBase> FindGroupByInviteHashAsync(Guid hash, bool onlyEnabledLink = true)
		{
			return onlyEnabledLink
				? db.Groups
					.FirstOrDefaultAsync(g => g.InviteHash == hash && !g.IsDeleted && g.IsInviteLinkEnabled)
				: db.Groups
					.FirstOrDefaultAsync(g => g.InviteHash == hash && !g.IsDeleted);
		}

		public IQueryable<GroupBase> GetCourseGroupsQueryable(string courseId, GroupQueryType groupType, bool includeArchived = false)
		{
			var queryGroup = groupType.HasFlag(GroupQueryType.SingleGroup);
			var querySuperGroup = groupType.HasFlag(GroupQueryType.SuperGroup);
			var groups = db.Groups.Where(g => g.CourseId == courseId && !g.IsDeleted &&
											(g.GroupType == GroupType.SingleGroup && queryGroup || g.GroupType == GroupType.SuperGroup && querySuperGroup));
			if (!includeArchived)
				groups = groups.Where(g => !g.IsArchived);
			return groups;
		}

		public Task<List<GroupBase>> GetCourseGroupsAsync(string courseId, GroupQueryType groupType, bool includeArchived = false)
		{
			return GetCourseGroupsQueryable(courseId, groupType, includeArchived).ToListAsync();
		}

		public Task<List<GroupBase>> GetMyGroupsFilterAccessibleToUserAsync(string courseId, string userId, bool includeArchived = false)
		{
			var accessibleGroupsIds = db.GroupAccesses.Where(a => a.Group.CourseId == courseId && a.UserId == userId && a.IsEnabled).Select(a => a.GroupId);

			var groups = db.Groups.Where(g => g.CourseId == courseId && !g.IsDeleted && (g.OwnerId == userId || accessibleGroupsIds.Contains(g.Id)));
			if (!includeArchived)
				groups = groups.Where(g => !g.IsArchived);
			return groups.ToListAsync();
		}

		/// <summary>
		/// Получение пользователей из групп в которых пользователь является владельцем или преподавателем
		/// </summary>
		public Task<List<string>> GetMyGroupsMembers(string courseId, string userId, bool includeArchived = false)
		{
			var groups = db.SingleGroups
				.GroupJoin(
					db.GroupAccesses,
					g => g.Id,
					ga => ga.GroupId,
					(group, accesses) => new { group, accesses }
				)
				.SelectMany(
					e => e.accesses.DefaultIfEmpty(),
					(e, access) => new { e.group, access }
				)
				.Where(e =>
					e.group.OwnerId == userId ||
					(e.access != null && e.access.UserId == userId && e.access.IsEnabled)
				)
				.Select(e => e.group);

			if (!includeArchived)
				groups = groups.Where(g => !g.IsArchived);

			var userIds = groups
				.SelectMany(g => g.Members)
				.Where(member => !member.User.IsDeleted)
				.Select(member => member.UserId)
				.Distinct();

			return userIds.ToListAsync();
		}

		public async Task<bool> GetDefaultProhibitFurtherReviewForUser(string courseId, string userId, string instructorId)
		{
			var accessibleGroupsIds = new HashSet<int>((await GetMyGroupsFilterAccessibleToUser(courseId, instructorId)).Select(g => g.Id));
			var userGroupsIdsWithDefaultProhibitFutherReview = db.GroupMembers
				.Include(m => m.Group)
				.Where(m => m.Group.CourseId == courseId && m.UserId == userId && !m.Group.IsDeleted && !m.Group.IsArchived && m.Group.DefaultProhibitFutherReview)
				.Select(m => m.GroupId)
				.Distinct()
				.ToList();
			return userGroupsIdsWithDefaultProhibitFutherReview.Any(g => accessibleGroupsIds.Contains(g));
		}

		public Task<List<int>> GetGroupIdsByMembers(string[] userIds)
		{
			return db.GroupMembers
				.Where(m => userIds.Contains(m.UserId))
				.Select(m => m.Group)
				.Where(g => !g.IsDeleted)
				.Select(g => g.Id)
				.Distinct()
				.ToListAsync();
		}

		public async Task<List<T>> FindGroupsFilterAvailableForUser<T>(IEnumerable<int> groupIds, string userId, string courseId = null) where T : GroupBase
		{
			var query = await FilterGroupsAvailableForUser_Internal<T>(groupIds, userId, courseId);
			return query is null
				? new List<T>()
				: await query.ToListAsync();
		}

		public async Task<List<int>> FilterGroupIdsAvailableForUser<T>(IEnumerable<int> groupIds, string userId, string courseId = null) where T : GroupBase
		{
			var query = await FilterGroupsAvailableForUser_Internal<T>(groupIds, userId, courseId);
			return query is null
				? new List<int>()
				: await query
					.Select(g => g.Id)
					.ToListAsync();
		}

		[ItemCanBeNull]
		private async Task<IQueryable<T>> FilterGroupsAvailableForUser_Internal<T>(IEnumerable<int> groupIds, string userId, [CanBeNull] string courseId = null) where T : GroupBase
		{
			groupIds = groupIds
				.Distinct()
				.ToArray();

			var query = db.Set<T>()
				.Where(g => groupIds.Contains(g.Id) && !g.IsDeleted);

			var accessesQuery = query
				.GroupJoin(
					db.GroupAccesses,
					g => g.Id,
					ga => ga.GroupId,
					(group, accesses) => new { group, accesses }
				)
				.SelectMany(
					e => e.accesses.DefaultIfEmpty(),
					(e, access) => new { e.group, access }
				);

			var isSysAdmin = await usersRepo.IsSystemAdministrator(userId);

			if (courseId is not null)
			{
				var courseRole = await courseRolesRepo.GetRole(userId, courseId);
				if (isSysAdmin || courseRole is CourseRoleType.CourseAdmin)
					return query.Where(g => g.CourseId == courseId);

				if (courseRole > CourseRoleType.Instructor)
					return null;

				return accessesQuery
					.Where(e => e.group.CourseId == courseId)
					.Where(e =>
						e.group.OwnerId == userId ||
						(e.access.UserId == userId && e.access.IsEnabled)
					)
					.Select(e => e.group)
					.Distinct();
			}

			if (isSysAdmin)
				return query;

			var userRoles = await courseRolesRepo.GetRoles(userId);
			var adminCourseIds = userRoles
				.Where(kvp => kvp.Value == CourseRoleType.CourseAdmin)
				.Select(kvp => kvp.Key)
				.ToList();
			var instructorCourseIds = userRoles
				.Where(kvp => kvp.Value == CourseRoleType.Instructor)
				.Select(kvp => kvp.Key)
				.ToList();

			if (adminCourseIds.Count + instructorCourseIds.Count == 0)
				return null;

			return accessesQuery
				.Where(e =>
					adminCourseIds.Contains(e.group.CourseId) ||
					instructorCourseIds.Contains(e.group.CourseId) &&
					(e.group.OwnerId == userId || (e.access.UserId == userId && e.access.IsEnabled))
				)
				.Select(e => e.group)
				.Distinct();
		}

		public async Task<bool> IsManualCheckingEnabledForUserAsync(Course course, string userId)
		{
			if (course.Settings.IsManualCheckingEnabled)
				return true;

			return await db.GroupMembers
				.AnyAsync(m => m.Group.CourseId == course.Id && m.UserId == userId && !m.Group.IsDeleted && !m.Group.IsArchived && m.Group.IsManualCheckingEnabled);
		}

		public async Task<HashSet<string>> GetUsersIdsWithEnabledManualChecking(Course course, List<string> userIds)
		{
			if (course.Settings.IsManualCheckingEnabled)
				return userIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

			return (await db.GroupMembers
					.Where(m => m.Group.CourseId == course.Id && !m.Group.IsDeleted && !m.Group.IsArchived && m.Group.IsManualCheckingEnabled)
					.Where(m => userIds.Contains(m.UserId))
					.Select(m => m.UserId)
					.ToListAsync())
				.ToHashSet(StringComparer.OrdinalIgnoreCase);
		}

		public async Task<bool> GetDefaultProhibitFurtherReviewForUserAsync(string courseId, string userId, string instructorId)
		{
			var accessibleGroups = await GetMyGroupsFilterAccessibleToUserAsync(courseId, instructorId).ConfigureAwait(false);
			var accessibleGroupsIds = accessibleGroups.Select(g => g.Id).ToHashSet();
			var userGroupsIdsWithDefaultProhibitFutherReview = await db.GroupMembers
				.Where(m => m.Group.CourseId == courseId && m.UserId == userId && !m.Group.IsDeleted && !m.Group.IsArchived && m.Group.DefaultProhibitFutherReview)
				.Select(m => m.GroupId)
				.Distinct()
				.ToListAsync()
				.ConfigureAwait(false);
			return userGroupsIdsWithDefaultProhibitFutherReview.Any(g => accessibleGroupsIds.Contains(g));
		}

		public async Task EnableAdditionalScoringGroupsForGroupAsync(int groupId, IEnumerable<string> scoringGroupsIds)
		{
			await EnableAdditionalScoringGroupsForGroupAsync(new HashSet<int> { groupId }, scoringGroupsIds.ToList());
		}

		public async Task EnableAdditionalScoringGroupsForGroupAsync(HashSet<int> groupIds, List<string> scoringGroupsIds)
		{
			await using var transaction = await db.Database.BeginTransactionAsync();

			db.EnabledAdditionalScoringGroups.RemoveRange(
				db.EnabledAdditionalScoringGroups.Where(e => groupIds.Contains(e.GroupId))
			);

			foreach (var groupId in groupIds)
			foreach (var scoringGroupId in scoringGroupsIds)
				db.EnabledAdditionalScoringGroups.Add(new EnabledAdditionalScoringGroup
				{
					GroupId = groupId,
					ScoringGroupId = scoringGroupId
				});

			await db.SaveChangesAsync().ConfigureAwait(false);

			await transaction.CommitAsync();
		}

		public Task<List<EnabledAdditionalScoringGroup>> GetEnabledAdditionalScoringGroupsAsync(string courseId, bool includeArchived = false)
		{
			var groupsIds = GetCourseGroupsQueryable(courseId, GroupQueryType.SingleGroup, includeArchived).Select(g => g.Id);
			return db.EnabledAdditionalScoringGroups.Where(e => groupsIds.Contains(e.GroupId)).ToListAsync();
		}

		public Task<List<EnabledAdditionalScoringGroup>> GetEnabledAdditionalScoringGroupsForGroupAsync(int groupId)
		{
			return GetEnabledAdditionalScoringGroupsForGroupsAsync(new HashSet<int> { groupId });
		}

		public Task<List<EnabledAdditionalScoringGroup>> GetEnabledAdditionalScoringGroupsForGroupsAsync(HashSet<int> groupsIds)
		{
			return db.EnabledAdditionalScoringGroups.Where(e => groupsIds.Contains(e.GroupId)).ToListAsync();
		}

		public async Task<List<string>> GetUsersIdsForAllGroups(string courseId)
		{
			var groupsIdsQueryable = GetCourseGroupsQueryable(courseId, GroupQueryType.SingleGroup)
				.Select(g => g.Id);

			return await db.GroupMembers
				.Where(m => groupsIdsQueryable.Contains(m.GroupId))
				.Select(m => m.UserId)
				.ToListAsync();
		}

		// khapov todo: duplicate GetMyGroupsFilterAccessibleToUserAsync?
		public async Task<List<GroupBase>> GetMyGroupsFilterAccessibleToUser(string courseId, string userId, bool includeArchived = false)
		{
			//var userId = user.Identity.GetUserId();
			var accessableGroupsIds = db.GroupAccesses
				.Where(a => a.Group.CourseId == courseId && a.UserId == userId && a.IsEnabled)
				.Select(a => a.GroupId);

			var groups = db.Groups
				.Where(g => g.CourseId == courseId && !g.IsDeleted && (g.OwnerId == userId || accessableGroupsIds.Contains(g.Id)));

			if (!includeArchived)
				groups = groups.Where(g => !g.IsArchived);

			return await groups.ToListAsync();
		}

		public async Task<List<string>> GetGroupsMembersAsUserIds(IEnumerable<int> groupIds)
		{
			return await db.GroupMembers
				.Include(m => m.User)
				.Where(m => !m.User.IsDeleted && groupIds.Contains(m.GroupId))
				.Select(m => m.UserId)
				.Distinct()
				.ToListAsync();
		}

		public async Task<IEnumerable<(int GroupId, string UserId)>> GetGroupsMembersAsGroupsIdsAndUserIds(IEnumerable<int> groupIds)
		{
			return (await db.GroupMembers
					.Include(m => m.User)
					.Where(m => !m.User.IsDeleted && groupIds.Contains(m.GroupId))
					.Select(m => new { m.GroupId, m.UserId })
					.ToListAsync())
				.Select(m => (m.GroupId, m.UserId));
		}

		public List<ApplicationUser> GetGroupsMembersAsUsers(IEnumerable<int> groupIds)
		{
			return db.GroupMembers
				.Include(m => m.User)
				.Where(m => !m.User.IsDeleted && groupIds.Contains(m.GroupId)).Select(m => m.User)
				.AsEnumerable()
				.Deprecated_DistinctBy(g => g.Id)
				.ToList();
		}
	}
}