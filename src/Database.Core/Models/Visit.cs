using System;
using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
	public class Visit : ITimedSlideAction
	{
		[Key]
		public int Id { get; set; }

		public virtual ApplicationUser User { get; set; }

		[StringLength(64)]
		[Required]
		public string UserId { get; set; }

		[Required]
		[StringLength(100)]
		public string CourseId { get; set; }

		[Required]
		public Guid SlideId { get; set; }

		///<summary>Первый заход на слайд</summary>
		[Required]
		public DateTime Timestamp { get; set; }

		public int Score { get; set; }
		public bool HasManualChecking { get; set; }
		[Obsolete("Фактически не используется")]
		public int AttemptsCount { get; set; }
		public bool IsSkipped { get; set; }
		// Условие выставления в SlideCheckingsRepo.IsSlidePassed
		public bool IsPassed { get; set; }

		public string IpAddress { get; set; }
	}
}