using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class Like
	{
		[Key]
		public int Id { get; set; }

		public virtual UserExerciseSubmission Submission { get; set; }

		[Required]
		[Index("IDX_Like_ByUserAndSubmission", 2)]
		[Index("IDX_Like_BySubmission")]
		[Index("IDX_Like_Course_Slide_Submission", 3)]
		public int SubmissionId { get; set; }

		[Required]
		[StringLength(64)]
		[Index("IDX_Like_ByUserAndSubmission", 1)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }

		[Required]
		[StringLength(100)]
		[Index("IDX_Likes_Course_Slide_Submission", 1)]
		public string CourseId { get; set; }

		[Required]
		[Index("IDX_Likes_Course_Slide_Submission", 2)]
		public Guid SlideId { get; set; }
	}
}