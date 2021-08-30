using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public enum SlideRates
	{
		Good,
		NotUnderstand,
		Trivial,
		NotWatched
	}

	public class SlideRate : ISlideAction
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public SlideRates Rate { get; set; }

		[StringLength(64)]
		[Required]
		[Index("SlideAndUser", 2)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		[StringLength(100)]
		public string CourseId { get; set; }

		[Required]
		[Index("SlideAndUser", 1)]
		public Guid SlideId { get; set; }
	}
}