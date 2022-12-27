namespace Database.Models;

public class UserSearchQueryModel
{
	public string NamePrefix { get; set; }
	public string Role { get; set; }
	public CourseRoleType? CourseRole { get; set; }
	public bool IncludeHighCourseRoles { get; set; }
	public string CourseId { get; set; }
	public bool OnlyPrivileged { get; set; }
}