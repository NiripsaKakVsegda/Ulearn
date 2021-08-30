using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class UserFlashcardsUnlocking
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[Index("IDX_UserFlashcardsUnlocking_ByUserIdAndCourseIdAndUnitId", 1, IsUnique = false)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		[StringLength(100)]
		[Index("IDX_UserFlashcardsUnlocking_ByUserIdAndCourseIdAndUnitId", 2, IsUnique = false)]
		public string CourseId { get; set; }

		[Required]
		[Index("IDX_UserFlashcardsUnlocking_ByUserIdAndCourseIdAndUnitId", 3, IsUnique = false)]
		public Guid UnitId { get; set; }
	}
}