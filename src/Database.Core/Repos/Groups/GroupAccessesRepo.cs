using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos.SystemAccessesRepo;
using Database.Repos.Users;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Extensions;

namespace Database.Repos.Groups
{
	public class GroupAccessesRepo : IGroupAccessesRepo
	{
		private readonly UlearnDb db;
		private readonly IGroupsRepo groupsRepo;
		private readonly ISystemAccessesRepo systemAccessesRepo;
		private readonly ICoursesRepo coursesRepo;
		private readonly IGroupMembersRepo groupMembersRepo;
		private readonly ICourseRolesRepo courseRolesRepo;
		private readonly IUsersRepo usersRepo;
		private readonly ICourseStorage courseStorage;

		public GroupAccessesRepo(
			UlearnDb db,
			IGroupsRepo groupsRepo, ISystemAccessesRepo systemAccessesRepo, ICoursesRepo coursesRepo, IGroupMembersRepo groupMembersRepo,
			IUsersRepo usersRepo,
			ICourseRolesRepo courseRolesRepo,
			ICourseStorage courseStorage
		)
		{
			this.db = db;
			this.groupsRepo = groupsRepo;
			this.systemAccessesRepo = systemAccessesRepo;
			this.coursesRepo = coursesRepo;
			this.groupMembersRepo = groupMembersRepo;
			this.courseRolesRepo = courseRolesRepo;
			this.usersRepo = usersRepo;
			this.courseStorage = courseStorage;
		}

		public async Task<GroupAccess> GrantAccessAsync(int groupId, string userId, GroupAccessType accessType, string grantedById)
		{
			var currentAccess = await db.GroupAccesses.FirstOrDefaultAsync(a => a.GroupId == groupId && a.UserId == userId).ConfigureAwait(false);
			if (currentAccess == null)
			{
				currentAccess = new GroupAccess
				{
					GroupId = groupId,
					UserId = userId,
				};
				db.GroupAccesses.Add(currentAccess);
			}

			currentAccess.AccessType = accessType;
			currentAccess.GrantedById = grantedById;
			currentAccess.GrantTime = DateTime.Now;
			currentAccess.IsEnabled = true;

			await db.SaveChangesAsync().ConfigureAwait(false);
			return db.GroupAccesses.Include(a => a.GrantedBy).Single(a => a.Id == currentAccess.Id);
		}

		/* TODO (andgein): move it to GroupController */
		public async Task<bool> CanRevokeAccessAsync(int groupId, string userId, string revokedById)
		{
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false) ?? throw new ArgumentNullException($"Can't find group with id={groupId}");
			var isCourseAdmin = await courseRolesRepo.HasUserAccessToCourse(revokedById, @group.CourseId, CourseRoleType.CourseAdmin).ConfigureAwait(false);
			if (group.OwnerId == revokedById || isCourseAdmin)
				return true;
			return db.GroupAccesses.Any(a => a.GroupId == groupId && a.UserId == userId && a.GrantedById == revokedById && a.IsEnabled);
		}

		public async Task<List<GroupAccess>> RevokeAccessAsync(int groupId, string userId)
		{
			var accesses = await db.GroupAccesses.Where(a => a.GroupId == groupId && a.UserId == userId).ToListAsync().ConfigureAwait(false);
			foreach (var access in accesses)
				access.IsEnabled = false;

			await db.SaveChangesAsync().ConfigureAwait(false);
			return accesses;
		}

		public Task<List<GroupAccess>> GetGroupAccessesAsync(int groupId)
		{
			return db.GroupAccesses.Include(a => a.User).Include(a => a.GrantedBy).Where(a => a.GroupId == groupId && a.IsEnabled && !a.User.IsDeleted).ToListAsync();
		}

		public async Task<DefaultDictionary<int, List<GroupAccess>>> GetGroupAccessesAsync(IEnumerable<int> groupsIds)
		{
			var groupIdsList = groupsIds.ToList();
			var groupAccesses = await db.GroupAccesses
				.Include(a => a.User)
				.Include(a => a.GrantedBy)
				.Where(a => groupIdsList.Contains(a.GroupId) && a.IsEnabled && !a.User.IsDeleted)
				.ToListAsync()
				.ConfigureAwait(false);

			return groupAccesses
				.GroupBy(a => a.GroupId)
				.ToDictionary(g => g.Key, g => g.ToList())
				.ToDefaultDictionary();
		}

		public async Task<bool> HasUserEditAccessToGroupAsync(int groupId, string userId)
		{
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false) ?? throw new ArgumentNullException($"Can't find group with id={groupId}");
			if ((await GetCoursesWhereUserCanEditAllGroupsAsync(userId).ConfigureAwait(false)).Contains(group.CourseId, StringComparer.OrdinalIgnoreCase))
				return true;
			return await HasUserGrantedAccessToGroupOrIsOwnerAsync(groupId, userId).ConfigureAwait(false);
		}

		public async Task<bool> HasUserGrantedAccessToGroupOrIsOwnerAsync(int groupId, string userId)
		{
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false) ?? throw new ArgumentNullException($"Can't find group with id={groupId}");

			if (group.OwnerId == userId)
				return true;

			return await db.GroupAccesses.Where(a => a.GroupId == groupId && a.UserId == userId && a.IsEnabled).AnyAsync().ConfigureAwait(false);
		}

		public async Task<bool> IsGroupVisibleForUserAsync(int groupId, string userId)
		{
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false) ?? throw new ArgumentNullException($"Can't find group with id={groupId}");
			return await IsGroupVisibleForUserAsync(group, userId).ConfigureAwait(false);
		}

		public async Task<bool> IsGroupVisibleForUserAsync(GroupBase group, string userId)
		{
			/* Course admins and other privileged users can see all groups in the course */
			if (await CanUserSeeAllCourseGroupsAsync(userId, group.CourseId).ConfigureAwait(false))
				return true;

			if (!await courseRolesRepo.HasUserAccessToCourse(userId, group.CourseId, CourseRoleType.Instructor).ConfigureAwait(false))
				return false;

			return await HasUserGrantedAccessToGroupOrIsOwnerAsync(group.Id, userId).ConfigureAwait(false);
		}

		private async Task<List<GroupBase>> InternalGetAvailableForUserGroupsAsync(
			[CanBeNull] List<string> coursesIds,
			string userId,
			bool needEditAccess,
			bool actual,
			bool archived,
			GroupQueryType groupType = GroupQueryType.SingleGroup)
		{
			var queryGroup = groupType.HasFlag(GroupQueryType.SingleGroup);
			var querySuperGroup = groupType.HasFlag(GroupQueryType.SuperGroup);
			List<string> coursesWhereUserCanSeeAllGroups = null;
			List<string> coursesWhereUserCanEditAllGroups = null;
			if (!needEditAccess)
			{
				if (coursesIds == null)
					coursesWhereUserCanSeeAllGroups = await GetCoursesWhereUserCanSeeAllGroupsAsync(userId).ConfigureAwait(false);
				else
					coursesWhereUserCanSeeAllGroups = await GetCoursesWhereUserCanSeeAllGroupsAsync(userId, coursesIds).ConfigureAwait(false);
			}
			else
			{
				coursesWhereUserCanEditAllGroups = (await GetCoursesWhereUserCanEditAllGroupsAsync(userId).ConfigureAwait(false))
					.Where(с => coursesIds == null || coursesIds.Contains(с, StringComparer.OrdinalIgnoreCase)).ToList();
			}

			var groupsWithAccess = new HashSet<int>(db.GroupAccesses.Where(a => a.UserId == userId && a.IsEnabled).Select(a => a.GroupId));
			var groups = (await db.Groups
					.Include(g => g.Owner)
					.Where(g => !g.IsDeleted &&
								(g.GroupType == GroupType.SingleGroup && queryGroup || g.GroupType == GroupType.SuperGroup && querySuperGroup) &&
								(actual && !g.IsArchived || archived && g.IsArchived) &&
								(coursesIds == null || coursesIds.Contains(g.CourseId)))
					.ToListAsync().ConfigureAwait(false))
				.Where(g =>
					/* Course admins can see all groups */
					!needEditAccess && coursesWhereUserCanSeeAllGroups.Contains(g.CourseId, StringComparer.OrdinalIgnoreCase) ||
					needEditAccess && coursesWhereUserCanEditAllGroups.Contains(g.CourseId, StringComparer.OrdinalIgnoreCase) ||
					/* Other instructor can see only own groups */
					g.OwnerId == userId || groupsWithAccess.Contains(g.Id)
				);

			return groups
				.OrderBy(g => g.IsArchived)
				.ThenBy(g => g.OwnerId != userId)
				.ThenBy(g => g.Name)
				.ToList();
		}

		public async Task<List<GroupBase>> GetAvailableForUserGroupsAsync(string courseId, string userId, bool needEditAccess, bool actual, bool archived, GroupQueryType groupType = GroupQueryType.SingleGroup)
		{
			if (!await courseRolesRepo.HasUserAccessToCourse(userId, courseId, CourseRoleType.Instructor).ConfigureAwait(false))
				return new List<GroupBase>();

			return await InternalGetAvailableForUserGroupsAsync(new List<string> { courseId }, userId, needEditAccess, actual, archived, groupType).ConfigureAwait(false);
		}

		public Task<List<GroupBase>> GetAvailableForUserGroupsAsync(List<string> coursesIds, string userId, bool needEditAccess, bool actual, bool archived, GroupQueryType groupType = GroupQueryType.SingleGroup)
		{
			return InternalGetAvailableForUserGroupsAsync(coursesIds, userId, needEditAccess, actual, archived);
		}

		public Task<List<GroupBase>> GetAvailableForUserGroupsAsync(
			string userId,
			bool needEditAccess,
			bool actual,
			bool archived,
			GroupQueryType groupType = GroupQueryType.SingleGroup)
		{
			return InternalGetAvailableForUserGroupsAsync(null, userId, needEditAccess, actual, archived, groupType);
		}

		/* Instructor can view student if he is a course admin or if student is member of one of accessible for instructor group */
		public async Task<bool> HasInstructorViewAccessToStudentGroup(string instructorId, string studentId)
		{
			if (await courseRolesRepo.HasUserAccessTo_Any_Course(instructorId, CourseRoleType.CourseAdmin).ConfigureAwait(false))
				return true;

			var coursesIds = courseStorage.GetCourses().Select(c => c.Id).ToList();
			var groups = await GetAvailableForUserGroupsAsync(coursesIds, instructorId, false, actual: true, archived: false).ConfigureAwait(false);
			var members = await groupMembersRepo.GetGroupsMembersAsync(groups.Select(g => g.Id).ToList()).ConfigureAwait(false);
			return members.Select(m => m.UserId).Contains(studentId);
		}

		public async Task<bool> HasInstructorEditAccessToStudentGroup(string instructorId, string studentId)
		{
			if (await courseRolesRepo.HasUserAccessTo_Any_Course(instructorId, CourseRoleType.CourseAdmin).ConfigureAwait(false))
				return true;

			var coursesIds = courseStorage.GetCourses().Select(c => c.Id).ToList();
			var groups = await GetAvailableForUserGroupsAsync(coursesIds, instructorId, true, actual: true, archived: false).ConfigureAwait(false);
			var members = await groupMembersRepo.GetGroupsMembersAsync(groups.Select(g => g.Id).ToList()).ConfigureAwait(false);
			return members.Select(m => m.UserId).Contains(studentId);
		}

		public async Task<List<string>> GetCoursesWhereUserCanSeeAllGroupsAsync(string userId, IEnumerable<string> coursesIds)
		{
			if (await usersRepo.IsSystemAdministrator(userId))
				return coursesIds.ToList();
			var result = new List<string>();
			foreach (var courseId in coursesIds)
				if (await CanUserSeeAllCourseGroupsAsync(userId, courseId, false).ConfigureAwait(false))
					result.Add(courseId);
			return result;
		}

		public async Task<List<GroupMember>> GetMembersOfAllGroupsVisibleForUserAsync(string userId)
		{
			var groups = await GetAvailableForUserGroupsAsync(userId, false, actual: true, archived: true, GroupQueryType.All).ConfigureAwait(false);
			return await groupMembersRepo.GetGroupsMembersAsync(groups.Select(g => g.Id).ToList()).ConfigureAwait(false);
		}

		public async Task<List<ApplicationUser>> GetInstructorsOfAllGroupsVisibleForUserAsync(string userId)
		{
			var groups = await GetAvailableForUserGroupsAsync(userId, false, actual: true, archived: true).ConfigureAwait(false);
			var accesses = await GetGroupAccessesAsync(groups.Select(g => g.Id)).ConfigureAwait(false);
			return accesses.SelectMany(a => a.Value).Select(a => a.User).ToList();
		}

		public async Task<List<string>> GetInstructorsOfAllGroupsWhereUserIsMemberAsync(string courseId, string userId)
		{
			var groupsWhereUserIsStudent = await groupMembersRepo.GetUserGroupsAsync(courseId, userId).ConfigureAwait(false);
			var accesses = await GetGroupAccessesAsync(groupsWhereUserIsStudent.Select(g => g.Id)).ConfigureAwait(false);
			var ownersIds = groupsWhereUserIsStudent.Select(g => g.OwnerId).ToList();
			return accesses
				.SelectMany(kvp => kvp.Value.Select(a => a.User.Id))
				.Concat(ownersIds)
				.Distinct()
				.ToList();
		}

		private async Task<List<string>> GetCoursesWhereUserCanSeeAllGroupsAsync(string userId)
		{
			if (await usersRepo.IsSystemAdministrator(userId) || await systemAccessesRepo.HasSystemAccessAsync(userId, SystemAccessType.ViewAllGroupMembers).ConfigureAwait(false))
				return await coursesRepo.GetPublishedCourseIds().ConfigureAwait(false);

			var coursesAsCourseAdmin = await courseRolesRepo.GetCoursesWhereUserIsInRole(userId, CourseRoleType.CourseAdmin).ConfigureAwait(false);
			var coursesWithCourseAccess = await coursesRepo.GetCoursesUserHasAccessTo(userId, CourseAccessType.ViewAllGroupMembers).ConfigureAwait(false);

			return new HashSet<string>(coursesAsCourseAdmin).Concat(coursesWithCourseAccess).ToList();
		}

		private async Task<List<string>> GetCoursesWhereUserCanEditAllGroupsAsync(string userId)
		{
			if (await usersRepo.IsSystemAdministrator(userId))
				return await coursesRepo.GetPublishedCourseIds().ConfigureAwait(false);

			var coursesAsCourseAdmin = await courseRolesRepo.GetCoursesWhereUserIsInRole(userId, CourseRoleType.CourseAdmin).ConfigureAwait(false);

			return new HashSet<string>(coursesAsCourseAdmin).ToList();
		}

		public async Task<bool> CanUserSeeAllCourseGroupsAsync(string userId, string courseId, bool? isSystemAdministrator = null)
		{
			if (isSystemAdministrator == true || isSystemAdministrator == null && await usersRepo.IsSystemAdministrator(userId))
				return true;
			var canViewAllGroupMembersGlobal = await systemAccessesRepo.HasSystemAccessAsync(userId, SystemAccessType.ViewAllGroupMembers).ConfigureAwait(false);
			var canViewAllGroupMembersInCourse = await coursesRepo.HasCourseAccess(userId, courseId, CourseAccessType.ViewAllGroupMembers).ConfigureAwait(false);
			var isCourseAdmin = await courseRolesRepo.HasUserAccessToCourse(userId, courseId, CourseRoleType.CourseAdmin).ConfigureAwait(false);
			return isCourseAdmin || canViewAllGroupMembersGlobal || canViewAllGroupMembersInCourse;
		}

		public async Task<Dictionary<string, string>> GetUsersGroupsNamesAsStrings(List<string> courseIds, IEnumerable<string> userIds, string userId, bool actual, bool archived, int maxCount = 3)
		{
			var usersGroups = (await GetUsersGroupsAsync(courseIds, userIds, userId, actual, archived, maxCount))
				.ToDictSafe(
					kv => kv.Key,
					kv => kv.Value.Select((g, idx) => idx >= maxCount ? "..." : g.Name.TruncateWithEllipsis(40)).ToList());
			return usersGroups.ToDictSafe(kv => kv.Key, kv => string.Join(", ", kv.Value));
		}

		public async Task<Dictionary<string, string>> GetUsersGroupsNamesAsStrings(string courseId, IEnumerable<string> userIds, string userId, bool actual, bool archived, int maxCount = 3)
		{
			return await GetUsersGroupsNamesAsStrings(new List<string> { courseId }, userIds, userId, actual, archived, maxCount);
		}

		public async Task<Dictionary<string, List<SingleGroup>>> GetUsersGroupsAsync(List<string> courseIds, IEnumerable<string> userIds, string userId, bool actual, bool archived, int maxCount = 3)
		{
			var canSeeAllGroups = new Dictionary<string, bool>();
			foreach (var courseId in courseIds)
				canSeeAllGroups[courseId.ToLower()] = await CanUserSeeAllCourseGroupsAsync(userId, courseId);

			var groupsWithAccess = new HashSet<int>(db.GroupAccesses.Where(a => a.UserId == userId && a.IsEnabled).Select(a => a.GroupId));
			var usersGroups = (await db.SingleGroups
					.SelectMany(
						g => g.Members,
						(group, member) => new { group, member }
					)
					.Where(e => userIds.Contains(e.member.UserId) && courseIds.Contains(e.group.CourseId))
					.ToListAsync()
				)
				.GroupBy(m => m.member.UserId)
				.ToDictionary(group => group.Key, group => group.ToList())
				.ToDictionary(
					kv => kv.Key,
					kv => kv.Value
						.Select(m => m.group)
						.Distinct()
						.Where(g => (g.OwnerId == userId || groupsWithAccess.Contains(g.Id) || canSeeAllGroups[g.CourseId.ToLower()]) && !g.IsDeleted)
						.Where(g => actual != g.IsArchived || archived == g.IsArchived)
						.OrderBy(g => g.OwnerId != userId)
						.Take(maxCount)
						.ToList()
				);

			return usersGroups;
		}

		public DefaultDictionary<int, List<GroupAccess>> GetGroupsAccesses(IEnumerable<int> groupsIds)
		{
			return db.GroupAccesses
				.Include(a => a.User)
				.Where(a => groupsIds.Contains(a.GroupId) && a.IsEnabled && !a.User.IsDeleted)
				.AsEnumerable()
				.GroupBy(a => a.GroupId)
				.ToDictionary(g => g.Key, g => g.ToList())
				.ToDefaultDictionary();
		}

		public async Task<Dictionary<string, List<int>>> GetUsersActualGroupsIds(List<string> courseIds, IEnumerable<string> userIds, string userId, int maxCount = 3)
		{
			var usersGroups = await GetUsersGroupsAsync(courseIds, userIds, userId, actual: true, archived: false, maxCount);
			return usersGroups.ToDictSafe(
				kv => kv.Key,
				kv => kv.Value.Select(g => g.Id).ToList());
		}

		public async Task<bool> CanInstructorViewStudent(string instructorId, string studentId, string courseId)
		{
			var isCourseAdmin = await courseRolesRepo.HasUserAccessToCourse(instructorId, courseId, CourseRoleType.CourseAdmin).ConfigureAwait(false);
			if (isCourseAdmin)
				return true;

			var coursesIds = courseStorage.GetCourses().Select(c => c.Id).ToList();
			var groups = await GetAvailableForUserGroupsAsync(coursesIds, instructorId, false, true, true);
			var members = await groupMembersRepo.GetGroupsMembersAsync(groups.Select(g => g.Id).ToList());
			return members.Select(m => m.UserId).Contains(studentId);
		}
	}
}