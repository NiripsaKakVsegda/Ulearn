using System;
using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
	// Это курсы, которые в базе остались, но на ulearn не загружаются и не видны
	public class ArchivedCourse
	{
		[Key]
		[Required]
		[StringLength(100)]
		public string CourseId { get; set; }

		[Required]
		[StringLength(64)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }
	}
}