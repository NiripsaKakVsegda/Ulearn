using System;
using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
	public class UserFlashcardsUnlocking
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		[StringLength(100)]
		public string CourseId { get; set; }

		[Required]
		public Guid UnitId { get; set; }
	}
}