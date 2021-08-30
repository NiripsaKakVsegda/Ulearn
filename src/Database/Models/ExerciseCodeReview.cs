using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using JetBrains.Annotations;

namespace Database.Models
{
	public class ExerciseCodeReview
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Index("IDX_ExerciseCodeReview_ByManualExerciseChecking")]
		public int? ExerciseCheckingId { get; set; }

		public virtual ManualExerciseChecking ExerciseChecking { get; set; }

		[Index("IDX_ExerciseCodeReview_BySubmission")]
		/* This field is used only for reviews not attached to specific ManualExerciseChecking */
		public int? SubmissionId { get; set; }

		[CanBeNull]
		public virtual UserExerciseSubmission Submission { get; set; }

		[Required]
		public int StartLine { get; set; }

		[Required]
		public int StartPosition { get; set; }

		[Required]
		public int FinishLine { get; set; }

		[Required]
		public int FinishPosition { get; set; }

		[Required]
		public string Comment { get; set; }

		[Required]
		[StringLength(64)]
		public string AuthorId { get; set; }

		public virtual ApplicationUser Author { get; set; }

		[Required]
		public bool IsDeleted { get; set; }

		[Required]
		public bool HiddenFromTopComments { get; set; }

		public DateTime? AddingTime { get; set; }

		public virtual IList<ExerciseCodeReviewComment> Comments { get; set; }

		[Required]
		[StringLength(100)]
		[Index("IDX_ExerciseCodeReview_Course_Slide_SubmissionAuthor", 1)]
		public string CourseId { get; set; }

		[Required]
		[Index("IDX_ExerciseCodeReview_Course_Slide_SubmissionAuthor", 2)]
		public Guid SlideId { get; set; }

		//[Required]
		[StringLength(64)]
		[Index("IDX_ExerciseCodeReview_Course_Slide_SubmissionAuthor", 3)]
		public string SubmissionAuthorId { get; set; }

		[NotMapped]
		public List<ExerciseCodeReviewComment> NotDeletedComments => Comments.Where(r => !r.IsDeleted).OrderBy(r => r.AddingTime).ToList();
	}
}