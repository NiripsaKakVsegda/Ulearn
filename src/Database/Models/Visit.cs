using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class Visit : ITimedSlideAction
	{
		[Key]
		public int Id { get; set; }

		public virtual ApplicationUser User { get; set; }

		[StringLength(64)]
		[Required]
		[Index("IDX_Visits_ByCourseSlideAndUser", 3, IsUnique = true)]
		[Index("IDX_Visits_ByCourseAndUser", 2)]
		public string UserId { get; set; }

		[Required]
		[StringLength(100)]
		[Index("IDX_Visits_ByCourseSlideAndUser", 1, IsUnique = true)]
		[Index("IDX_Visits_ByCourseSlideAndTime", 1)]
		[Index("IDX_Visits_ByCourseAndUser", 1)]
		public string CourseId { get; set; }

		[Required]
		[Index("IDX_Visits_ByCourseSlideAndUser", 2, IsUnique = true)]
		[Index("IDX_Visits_ByCourseSlideAndTime", 2)]
		public Guid SlideId { get; set; }

		///<summary>Первый заход на слайд</summary>
		[Required]
		[Index("IDX_Visits_ByCourseSlideAndTime", 3)]
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