using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
	public enum LmsRoles
	{
		[Display(Name = "Сис. админ")]
		SysAdmin
	}

	public enum CourseRole
	{
		[Display(Name = "Администратор курса")]
		CourseAdmin,

		[Display(Name = "Преподаватель")]
		Instructor,

		[Display(Name = "Тестер")]
		Tester,

		[Display(Name = "Студент")]
		Student
	}
}