using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Ulearn.Common;

namespace Database.Models
{
	public class UserExerciseSubmission : ITimedSlideAction
	{
		[Required]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(40)]
		[Index("IDX_UserExerciseSubmissions_BySlideAndUser", 3)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		[StringLength(100)]
		[Index("IDX_UserExerciseSubmissions_BySlideAndUser", 1)]
		[Index("IDX_UserExerciseSubmissions_ByCourseAndSlide", 1)]
		[Index("IDX_UserExerciseSubmissions_BySlideAndTime", 1)]
		[Index("IDX_UserExerciseSubmissions_ByCourseAndIsRightAnswer", 1)]
		[Index("IDX_UserExerciseSubmissions_BySlideAndIsRightAnswer", 1)]
		public string CourseId { get; set; }

		[Required]
		[Index("IDX_UserExerciseSubmissions_BySlideAndUser", 2)]
		[Index("IDX_UserExerciseSubmissions_ByCourseAndSlide", 2)]
		[Index("IDX_UserExerciseSubmissions_BySlideAndTime", 2)]
		[Index("IDX_UserExerciseSubmissions_BySlideAndIsRightAnswer", 2)]
		public Guid SlideId { get; set; }

		[Required]
		[Index("IDX_UserExerciseSubmissions_BySlideAndTime", 3)]
		[Index("IDX_UserExerciseSubmissions_ByTime", 1)]
		public DateTime Timestamp { get; set; }

		[Required]
		[StringLength(40)]
		public string SolutionCodeHash { get; set; }

		public virtual TextBlob SolutionCode { get; set; }

		[Required]
		public int CodeHash { get; set; }

		public virtual IList<Like> Likes { get; set; }

		public int? AutomaticCheckingId { get; set; }

		public virtual AutomaticExerciseChecking AutomaticChecking { get; set; }

		[Index("IDX_UserExerciseSubmissions_ByCourseAndIsRightAnswer", 2)]
		[Index("IDX_UserExerciseSubmissions_BySlideAndIsRightAnswer", 3)]
		[Index("IDX_UserExerciseSubmissions_ByIsRightAnswer")]
		public bool AutomaticCheckingIsRightAnswer { get; set; }

		[Index("IDX_UserExerciseSubmissions_ByLanguage")]
		public Language Language { get; set; }

		[StringLength(40)]
		[Index("IDX_UserExerciseSubmissions_BySandbox")]
		public string Sandbox { get; set; } // null if csharp sandbox

		public virtual ManualExerciseChecking ManualChecking { get; set; }

		[Obsolete] // YT: ULEARN-217; Используй AntiPlagiarism.Web.Database.Models.Submission.ClientSubmissionId
		[Index("IDX_UserExerciseSubmission_ByAntiPlagiarismSubmissionId")]
		public int? AntiPlagiarismSubmissionId { get; set; }

		public virtual IList<ExerciseCodeReview> Reviews { get; set; }

		[NotMapped]
		public List<ExerciseCodeReview> NotDeletedReviews => Reviews.Where(r => !r.IsDeleted).ToList();

		public List<ExerciseCodeReview> GetAllReviews()
		{
			var manualCheckingReviews = (ManualChecking?.NotDeletedReviews).EmptyIfNull();
			return manualCheckingReviews.Concat(NotDeletedReviews).ToList();
		}
	}
}