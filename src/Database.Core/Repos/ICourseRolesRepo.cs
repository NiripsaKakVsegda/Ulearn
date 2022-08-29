using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;

namespace Database.Repos
{
	public interface ICourseRolesRepo
	{
		Task<Dictionary<string, CourseRoleType>> GetRoles(string userId);
		Task<CourseRoleType> GetRole(string userId, string courseId);
		Task<bool> ToggleRole(string courseId, string userId, CourseRoleType roleType, string grantedById, string comment);
		Task<bool> HasUserAccessToCourse(string userId, string courseId, CourseRoleType minCourseRoleType);
		Task<bool> HasUserAccessTo_Any_Course(string userId, CourseRoleType minCourseRoleType);
		Task<List<string>> GetCoursesWhereUserIsInRole(string userId, CourseRoleType minCourseRoleType);
		Task<List<string>> GetCoursesWhereUserIsInStrictRole(string userId, CourseRoleType courseRoleType);
		Task<List<string>> GetListOfUsersWithCourseRole(CourseRoleType? courseRoleType, [CanBeNull] string courseId, bool includeHighRoles);
		Task<List<string>> GetListOfUsersByPrivilege(bool onlyPrivileged, string courseId);
		Task<List<UserRolesInfo>> FilterUsersByEmail(UserSearchQueryModel query, int limit = 100);
		Task<List<UserRolesInfo>> FilterUsers(UserSearchQueryModel query, int limit = 100);
		Task<Dictionary<string, Dictionary<CourseRoleType, List<string>>>> GetCoursesForUsers();
		Task<List<CourseRole>> GetUserRolesHistoryByCourseId(string userId, string courseId);
		Task<List<CourseRole>> GetUserRolesHistory(string userId);
		Task<Dictionary<string, List<CourseRoleType>>> GetRolesByUsers(string courseId);
		Task<List<string>> FilterUsersByNamePrefix(string namePrefix);
	}
}