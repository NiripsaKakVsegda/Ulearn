using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Ulearn.Core.Courses;

namespace Database.Repos.Groups
{
	public class GroupsRepo : IGroupsRepo
	{
		private readonly UlearnDb db;
		private readonly IGroupsCreatorAndCopier groupsCreatorAndCopier;
		private readonly IManualCheckingsForOldSolutionsAdder manualCheckingsForOldSolutionsAdder;

		public GroupsRepo(
			UlearnDb db,
			IGroupsCreatorAndCopier groupsCreatorAndCopier, IManualCheckingsForOldSolutionsAdder manualCheckingsForOldSolutionsAdder)
		{
			this.db = db;
			this.groupsCreatorAndCopier = groupsCreatorAndCopier;
			this.manualCheckingsForOldSolutionsAdder = manualCheckingsForOldSolutionsAdder;
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

		public async Task<GroupBase> ModifyGroupAsync(int groupId, GroupSettings newSettings)
		{
			var groupBase = await FindGroupByIdAsync(groupId).ConfigureAwait(false) ?? throw new ArgumentNullException($"Can't find group with id={groupId}");
			groupBase.Name = newSettings.NewName ?? groupBase.Name;

			if (groupBase is SingleGroup singleGroup)
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
			}

			if (groupBase is SuperGroup superGroup)
			{
				superGroup.DistributionTableLink = newSettings.DistributionTableLink;
			}

			await db.SaveChangesAsync().ConfigureAwait(false);

			return groupBase;
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
		public Task<GroupBase> FindGroupByIdAsync(int groupId)
		{
			return db.Groups.FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted);
		}

		[ItemCanBeNull]
		public Task<GroupBase> FindGroupByInviteHashAsync(Guid hash)
		{
			return db.Groups.FirstOrDefaultAsync(g => g.InviteHash == hash && !g.IsDeleted && g.IsInviteLinkEnabled);
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

		public async Task<bool> GetDefaultProhibitFutherReviewForUser(string courseId, string userId, string instructorId)
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
			using (var transaction = db.Database.BeginTransaction())
			{
				db.EnabledAdditionalScoringGroups.RemoveRange(
					db.EnabledAdditionalScoringGroups.Where(e => e.GroupId == groupId)
				);

				foreach (var scoringGroupId in scoringGroupsIds)
					db.EnabledAdditionalScoringGroups.Add(new EnabledAdditionalScoringGroup
					{
						GroupId = groupId,
						ScoringGroupId = scoringGroupId
					});

				await db.SaveChangesAsync().ConfigureAwait(false);

				transaction.Commit();
			}
		}

		public Task<List<EnabledAdditionalScoringGroup>> GetEnabledAdditionalScoringGroupsAsync(string courseId, bool includeArchived = false)
		{
			var groupsIds = GetCourseGroupsQueryable(courseId, GroupQueryType.SingleGroup, includeArchived).Select(g => g.Id);
			return db.EnabledAdditionalScoringGroups.Where(e => groupsIds.Contains(e.GroupId)).ToListAsync();
		}

		public Task<List<EnabledAdditionalScoringGroup>> GetEnabledAdditionalScoringGroupsForGroupAsync(int groupId)
		{
			return db.EnabledAdditionalScoringGroups.Where(e => e.GroupId == groupId).ToListAsync();
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