using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database.Models
{
	/* For backward compatibility: EF Core changed table naming convention.
	See https://github.com/aspnet/Announcements/issues/167 for details */
	[Table("Likes")]
	[Index(nameof(SubmissionId))]
	[Index(nameof(UserId), nameof(SubmissionId))]
	[Index(nameof(CourseId), nameof(SlideId), nameof(SubmissionId))]
	public class Like
	{
		[Key]
		public int Id { get; set; }

		public virtual UserExerciseSubmission Submission { get; set; }

		[Required]
		public int SubmissionId { get; set; }

		[Required]
		[StringLength(64)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }

		[Required]
		[StringLength(100)]
		public string CourseId { get; set; }

		[Required]
		public Guid SlideId { get; set; }
	}
}