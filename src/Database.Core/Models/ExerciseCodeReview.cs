using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Database.Models
{
	[Index(nameof(ExerciseCheckingId))]
	[Index(nameof(CourseId), nameof(SlideId), nameof(SubmissionAuthorId))]
	public class ExerciseCodeReview
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public int? ExerciseCheckingId { get; set; }

		// Заполняется, если не ревью бота, иначе Submission
		public virtual ManualExerciseChecking ExerciseChecking { get; set; }

		public int? SubmissionId { get; set; }

		// Заполняется вместо ExerciseChecking для ревью бота
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
		public string CourseId { get; set; }

		[Required]
		public Guid SlideId { get; set; }

		//[Required]
		[StringLength(64)]
		public string SubmissionAuthorId { get; set; }

		public virtual ApplicationUser SubmissionAuthor { get; set; }

		[NotMapped]
		public List<ExerciseCodeReviewComment> NotDeletedComments => Comments.Where(r => !r.IsDeleted).ToList();
	}
}