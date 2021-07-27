using System;
using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
	public class TempCourse
	{
		[Key]
		[StringLength(100)]
		public string CourseId { get; set; }

		[Required]
		public DateTime LoadingTime { get; set; } // Время загрузки новой версии, используется как id версии

		[Required]
		[StringLength(64)]
		public string AuthorId { get; set; }

		public virtual ApplicationUser Author { get; set; }
	}
}