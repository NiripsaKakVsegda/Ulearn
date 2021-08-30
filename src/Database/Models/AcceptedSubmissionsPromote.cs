using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class AcceptedSolutionsPromote
	{
		[Key]
		public int SubmissionId { get; set; }

		public virtual UserExerciseSubmission Submission { get; set; }

		[Required]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }

		[Required]
		[StringLength(100)]
		[Index("IDX_AcceptedSolutionsPromotes_Course_Slide_Submission", 1)]
		public string CourseId { get; set; }

		[Required]
		[Index("IDX_AcceptedSolutionsPromotes_Course_Slide_Submission", 2)]
		public Guid SlideId { get; set; }
	}
}