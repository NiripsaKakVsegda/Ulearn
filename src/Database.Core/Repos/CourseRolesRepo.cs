using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos.Users;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Ulearn.Common.Extensions;

namespace Database.Repos
{
	public class CourseRolesRepo : ICourseRolesRepo
	{
		private readonly UlearnDb db;
		private readonly IUsersRepo usersRepo;
		private readonly UserManager<ApplicationUser> userManager;

		public CourseRolesRepo(UlearnDb db, IUsersRepo usersRepo, UserManager<ApplicationUser> userManager)
		{
			this.db = db;
			this.usersRepo = usersRepo;
			this.userManager = userManager;
		}

		public async Task<List<CourseRole>> Internal_GetActualUserRoles([CanBeNull] string userId, string courseId = null, bool filterByUserId = true)
		{
			if (userId == null && filterByUserId) // В этом случае userId null рассматриваем как гостя
				return new List<CourseRole>();
			var query = db.CourseRoles.AsQueryable();
			if (userId != null)
				query = query.Where(x => x.UserId == userId);
			if (courseId != null)
				query = query.Where(x => x.CourseId == courseId);
			var userCourseRoles = await query.ToListAsync().ConfigureAwait(false);
			return userCourseRoles
				.GroupBy(x => x.UserId + x.CourseId + x.Role, StringComparer.OrdinalIgnoreCase)
				.Select(gr => gr.OrderByDescending(x => x.Id))
				.Select(x => x.FirstOrDefault())
				.Where(x => x != null && (!x.IsEnabled.HasValue || x.IsEnabled.Value))
				.ToList();
		}

		public async Task<Dictionary<string, CourseRoleType>> GetRoles(string userId)
		{
			return (await Internal_GetActualUserRoles(userId))
				.GroupBy(role => role.CourseId, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(g => g.Key, g => g.Select(role => role.Role).Min());
		}
		
		public async Task<CourseRoleType> GetRole(string userId, string courseId)
		{
			var roles = (await Internal_GetActualUserRoles(userId, courseId)).Select(role => role.Role).ToList();
			return roles.Any() ? roles.Min() : CourseRoleType.Student;
		}

		public async Task<bool> ToggleRole(string courseId, string userId, CourseRoleType roleType, string grantedById, string comment)
		{
			var userRoles = await db.CourseRoles.ToListAsync();

			var userRole = userRoles.LastOrDefault(u => u.UserId == userId && u.Role == roleType && string.Equals(u.CourseId, courseId, StringComparison.OrdinalIgnoreCase));
			bool isEnabled;
			if (userRole != null && (!userRole.IsEnabled.HasValue || userRole.IsEnabled.Value))
				isEnabled = false;
			else
				isEnabled = true;
			db.CourseRoles.Add(new CourseRole
			{
				UserId = userId,
				CourseId = courseId,
				Role = roleType,
				IsEnabled = isEnabled,
				GrantedById = grantedById,
				GrantTime = DateTime.Now.ToUniversalTime(),
				Comment = comment
			});

			await db.SaveChangesAsync();

			return isEnabled;
		}

		public async Task<bool> HasUserAccessToCourse(string userId, string courseId, CourseRoleType minCourseRoleType)
		{
			var user = await usersRepo.FindUserById(userId).ConfigureAwait(false);
			if (usersRepo.IsSystemAdministrator(user))
				return true;

			return (await Internal_GetActualUserRoles(userId)).Any(r => string.Equals(r.CourseId, courseId, StringComparison.OrdinalIgnoreCase) && r.Role <= minCourseRoleType);
		}

		public async Task<bool> HasUserAccessTo_Any_Course(string userId, CourseRoleType minCourseRoleType)
		{
			var user = await usersRepo.FindUserById(userId).ConfigureAwait(false);
			if (usersRepo.IsSystemAdministrator(user))
				return true;

			return (await Internal_GetActualUserRoles(userId)).Any(r => r.Role <= minCourseRoleType);
		}

		public async Task<List<string>> GetCoursesWhereUserIsInRole(string userId, CourseRoleType minCourseRoleType)
		{
			var roles = (await Internal_GetActualUserRoles(userId)).Where(r => r.Role <= minCourseRoleType).ToList();
			return roles.Select(r => r.CourseId).ToList();
		}

		public async Task<List<string>> GetCoursesWhereUserIsInStrictRole(string userId, CourseRoleType courseRoleType)
		{
			var roles = (await Internal_GetActualUserRoles(userId)).Where(r => r.Role == courseRoleType).ToList();
			return roles.Select(r => r.CourseId).ToList();
		}

		public async Task<List<string>> GetListOfUsersWithCourseRole(CourseRoleType? courseRoleType, string courseId, bool includeHighRoles)
		{
			if (!courseRoleType.HasValue)
				return null;
			
			IEnumerable<CourseRole> usersRoles = await Internal_GetActualUserRoles(null, courseId.NullIfEmptyOrWhitespace(), false);
			usersRoles = includeHighRoles
				? usersRoles.Where(userRole => userRole.Role <= courseRoleType)
				: usersRoles.Where(userRole => userRole.Role == courseRoleType);
			return usersRoles.Select(user => user.UserId).Distinct().ToList();
		}

		public async Task<List<string>> GetListOfUsersByPrivilege(bool onlyPrivileged, string courseId)
		{
			if (!onlyPrivileged)
				return null;

			var usersRoles = await Internal_GetActualUserRoles(userId: null, courseId: courseId, filterByUserId: false);
			return usersRoles.Select(userRole => userRole.UserId).Distinct().ToList();
		}

		[ItemCanBeNull]
		public async Task<List<UserRolesInfo>> FilterUsersByEmail(UserSearchQueryModel query, int limit = 100)
		{
			if (string.IsNullOrEmpty(query.NamePrefix) || !query.NamePrefix.Contains('@'))
				return null;
			var email = query.NamePrefix;
			var usersIdsByEmail = db.Users.Where(u => u.Email == email).Select(u => u.Id);
			return await FilterUsers(query, usersIdsByEmail, null, limit);
		}

		public async Task<List<UserRolesInfo>> FilterUsers(UserSearchQueryModel query, int limit = 100)
		{
			var usersIdsByNamePrefix = string.IsNullOrEmpty(query.NamePrefix)
				? null
				: await GetUsersByNamePrefix(query.NamePrefix);
			return await FilterUsers(query, null, usersIdsByNamePrefix, limit);
		}

		private static Regex nonWordChars = new(@"[^\w\s\-\.@_]*", RegexOptions.Compiled);

		private async Task<List<string>> GetUsersByNamePrefix(string name, int limit = 100)
		{
			name = name.ToLower();
			var escapedName = nonWordChars.Replace(name, "").Replace(".", "\\.").Trim();
			var sql =
				$@"SELECT ""Id""
FROM ""AspNetUsers""
WHERE ""IsDeleted"" = False and ""Names"" ~ @query
LIMIT {limit}"; // ~ - регвыр c учетом размера букв. Есть ~* без учета. Но мы все равно поле ловеркейзим, чтобы по нему можно было искать в том числе like. 
			// var userIds = db.Database.SqlQuery<string>(
			// 	sql,
			// 	new NpgsqlParameter<string>("@query", $@"(^|\s){escapedName}")
			// ).ToList();
			var userIds = await db.Users
				.FromSqlRaw(sql, new NpgsqlParameter<string>("@query", $@"(^|\s){escapedName}"))
				.Select(u => u.Id)
				.ToListAsync();
			return userIds;
		}
		
		public async Task<List<string>> FilterUsersByNamePrefix(string namePrefix)
		{
			if (string.IsNullOrEmpty(namePrefix))
				return await db.Users.Where(u => !u.IsDeleted).Select(u => u.Id).ToListAsync();
			return (await GetUsersByNamePrefix(namePrefix)).ToList();
		}

		private async Task<List<UserRolesInfo>> FilterUsers(UserSearchQueryModel query, [CanBeNull] IQueryable<string> userIdsQuery, [CanBeNull] List<string> userIds, int limit)
		{
			var roles = await db.Roles.ToListAsync();
			var role = string.IsNullOrEmpty(query.Role) ? null : roles.FirstOrDefault(r => r.Name == query.Role);
			var users = db.Users.Include(u => u.Roles).Where(u => !u.IsDeleted);
			if (userIdsQuery != null)
				users = users.Where(u => userIdsQuery.Contains(u.Id));

			return await users
				.FilterByRole(role)
				.FilterByUserIds(
					await GetListOfUsersWithCourseRole(query.CourseRole, query.CourseId, query.IncludeHighCourseRoles),
					await GetListOfUsersByPrivilege(query.OnlyPrivileged, query.CourseId),
					userIds
				)
				.GetUserRolesInfo(limit, userManager);
		}

		public async Task<Dictionary<string, Dictionary<CourseRoleType, List<string>>>> GetCoursesForUsers()
		{
			return (await Internal_GetActualUserRoles(userId: null, courseId: null, filterByUserId: false))
				.GroupBy(userRole => userRole.UserId)
				.ToDictionary(
					g => g.Key,
					g => g
						.GroupBy(userRole => userRole.Role)
						.ToDictionary(
							gg => gg.Key,
							gg => gg
								.Select(userRole => userRole.CourseId.ToLower())
								.ToList()
						)
				);
		}

		public async Task<List<CourseRole>> GetUserRolesHistoryByCourseId(string userId, string courseId)
		{
			courseId = courseId.ToLower();
			return await db.CourseRoles
				.Where(x => x.UserId == userId && x.CourseId == courseId)
				.ToListAsync();
		}

		public async Task<List<CourseRole>> GetUserRolesHistory(string userId)
		{
			return await db.CourseRoles.Where(x => x.UserId == userId).ToListAsync();
		}

		public async Task<Dictionary<string, List<CourseRoleType>>> GetRolesByUsers(string courseId)
		{
			var userRoles = await Internal_GetActualUserRoles(userId: null, courseId: courseId, filterByUserId: false);
			return userRoles
				.GroupBy(role => role.UserId)
				.ToDictionary(
					g => g.Key,
					g => g.Select(role => role.Role).Distinct().ToList()
				);
		}
	}
}