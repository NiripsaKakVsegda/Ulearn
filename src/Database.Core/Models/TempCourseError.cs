using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
	public class TempCourseError
	{
		[Key]
		[StringLength(100)]
		public string CourseId { get; set; }

		public string Error { get; set; }
	}
}