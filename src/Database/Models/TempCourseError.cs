using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace Database.Models
{
	public class TempCourseError
	{
		[Key]
		[StringLength(100)]
		public string CourseId { get; set; }

		[CanBeNull]
		public string Error { get; set; }

	}
}