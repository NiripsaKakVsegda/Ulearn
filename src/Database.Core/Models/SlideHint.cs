using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	/* For backward compatibility: EF Core changed table naming convention.
	   See https://github.com/aspnet/Announcements/issues/167 for details */
	[Table("SlideHints")]
	public class SlideHint : ISlideAction
	{
		[Key]
		public int Id { get; set; }

		public virtual ApplicationUser User { get; set; }

		[StringLength(64)]
		[Required]
		public string UserId { get; set; }

		[Required]
		public int HintId { get; set; }

		[Required]
		[StringLength(100)]
		public string CourseId { get; set; }

		[Required]
		public Guid SlideId { get; set; }

		public bool IsHintHelped { get; set; }
	}
}