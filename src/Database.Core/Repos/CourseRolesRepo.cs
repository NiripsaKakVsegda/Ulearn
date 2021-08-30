using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos.Users;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common.Extensions;

namespace Database.Repos
{
	public class CourseRolesRepo : ICourseRolesRepo
	{
		private readonly UlearnDb db;
		private readonly IUsersRepo usersRepo;

		public CourseRolesRepo(UlearnDb db, IUsersRepo usersRepo)
		{
			this.db = db;
			this.usersRepo = usersRepo;
		}

		public async Task<List<CourseRole>> Internal_GetActualUserRoles([CanBeNull]string userId, string courseId = null, bool filterByUserId = true)
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

		public async Task<List<string>> GetListOfUsersWithCourseRole(CourseRoleType courseRoleType, string courseId, bool includeHighRoles)
		{
			IEnumerable<CourseRole> usersRoles = await Internal_GetActualUserRoles(null, courseId.NullIfEmptyOrWhitespace(), false);
			usersRoles = includeHighRoles
				? usersRoles.Where(userRole => userRole.Role <= courseRoleType)
				: usersRoles.Where(userRole => userRole.Role == courseRoleType);
			return usersRoles.Select(user => user.UserId).Distinct().ToList();
		}
	}
}