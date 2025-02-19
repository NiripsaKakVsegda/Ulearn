using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database.Models.Quizzes
{
	[Index(nameof(CourseId), nameof(SlideId))]
	[Index(nameof(CourseId), nameof(SlideId), nameof(UserId))]
	[Index(nameof(CourseId), nameof(SlideId), nameof(Timestamp))]
	public class UserQuizSubmission : ITimedSlideAction
	{
		[Required]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(40)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		[StringLength(100)]
		public string CourseId { get; set; }

		[Required]
		public Guid SlideId { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }

		public virtual AutomaticQuizChecking AutomaticChecking { get; set; }

		public virtual ManualQuizChecking ManualChecking { get; set; }
	}
}