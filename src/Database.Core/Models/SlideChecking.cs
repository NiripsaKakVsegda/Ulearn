using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using Database.Models.Quizzes;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common.Extensions;

namespace Database.Models
{
	public class AbstractSlideChecking : ITimedSlideAction
	{
		public virtual int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string CourseId { get; set; }

		[Required]
		public Guid SlideId { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }

		// Пользователь, чья работа проверяется. А кто проверил, написано в LockedById
		[Required]
		[StringLength(64)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		public virtual void PreRemove(UlearnDb db)
		{
		}
	}

	public class AbstractManualSlideChecking : AbstractSlideChecking
	{
		[CanBeNull]
		public DateTime? LockedUntil { get; set; }

		[StringLength(64)]
		[CanBeNull]
		public string LockedById { get; set; }

		[CanBeNull]
		public virtual ApplicationUser LockedBy { get; set; }

		public bool IsChecked { get; set; }

		[CanBeNull]
		public string CheckedById { get; set; }
		
		[CanBeNull]
		public virtual ApplicationUser CheckedBy { get; set; }
		
		[CanBeNull]
		public DateTime? CheckedTimestamp { get; set; }

		public bool IsLocked => LockedUntil.HasValue && LockedUntil.Value > DateTime.Now;

		public bool IsLockedBy(ClaimsPrincipal identity)
		{
			return IsLocked && LockedById == identity.GetUserId();
		}

		public bool HasLastLockedBy(ClaimsPrincipal identity)
		{
			return LockedById == identity.GetUserId();
		}
	}

	public class AbstractAutomaticSlideChecking : AbstractSlideChecking
	{
	}

	public enum AutomaticExerciseCheckingStatus
	{
		Done = 0,
		Waiting = 1,
		NotFound = 2,
		AccessDeny = 3,
		Error = 4, // Не получен ответ от чеккера или результат имеет статус SandboxError
		Running = 5,
		RequestTimeLimit = 6 // Не взято из очереди за разумное время
	}

	[Index(nameof(CourseId), nameof(SlideId))]
	[Index(nameof(CourseId), nameof(SlideId), nameof(UserId))]
	[Index(nameof(CourseId), nameof(SlideId), nameof(Timestamp))]
	[Index(nameof(CourseId), nameof(UserId))]
	[Index(nameof(IsRightAnswer))]
	[Index(nameof(CourseId), nameof(SlideId), nameof(IsRightAnswer))]
	public class AutomaticExerciseChecking : AbstractAutomaticSlideChecking
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public override int Id { get; set; }

		public AutomaticExerciseCheckingStatus Status { get; set; }

		public TimeSpan? Elapsed { get; set; }

		public string DisplayName { get; set; }

		[Required]
		public bool IsRightAnswer { get; set; }

		[Required]
		public bool IsCompilationError { get; set; }

		[CanBeNull]
		public virtual TextBlob CompilationError { get; set; }

		[StringLength(40)]
		public string CompilationErrorHash { get; set; }

		[CanBeNull]
		public virtual TextBlob Output { get; set; }

		[CanBeNull]
		public virtual TextBlob DebugLogs { get; set; }

		[StringLength(40)]
		public string OutputHash { get; set; }

		[StringLength(40)]
		public string DebugLogsHash { get; set; }

		[StringLength(40)]
		public string ExecutionServiceName { get; set; }

		[StringLength(256)]
		public string CheckingAgentName { get; set; }

		[Obsolete] // Данные этого столбца вычисляются из других. Оставелно, чтобы не удалять столбец
		public int? Score { get; set; }

		public float? Points { get; set; }

		public string GetVerdict()
		{
			if (IsCompilationError)
				return "CompilationError";
			if (!IsRightAnswer)
				return "Wrong Answer";

			return "Accepted";
		}
	}

	/* Manual Exercise Checking is Code Review */

	[Index(nameof(CourseId), nameof(SlideId), nameof(UserId), nameof(Timestamp))]
	[Index(nameof(CourseId), nameof(SlideId), nameof(UserId), nameof(ProhibitFurtherManualCheckings))]
	[Index(nameof(CourseId), nameof(SlideId), nameof(Timestamp))]
	[Index(nameof(CourseId), nameof(IsChecked), nameof(UserId), nameof(SlideId), nameof(Timestamp))]
	[Index(nameof(CourseId), nameof(IsChecked), nameof(SlideId), nameof(Timestamp))]
	public class ManualExerciseChecking : AbstractManualSlideChecking
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public override int Id { get; set; }

		public virtual UserExerciseSubmission Submission { get; set; }

		[Required]
		// Действует, если стоит хотя бы у одной проверки. Если снимается у одной проверки, снимается у всех.
		public bool ProhibitFurtherManualCheckings { get; set; }

		// Здесь ревью преподавателя. Ревью бота лежат в UserExerciseSubmission
		public virtual IList<ExerciseCodeReview> Reviews { get; set; }

		[Obsolete("Хранит старые данные, теперь используется Percent")]
		public int? Score { get; set; }

		// Процент, поставленный преподавателем за ревью. Если поставить меньше баллов бота, то баллы бота уменьшется.
		// Obsolete: Если процент не указан, используется Score. Это старый сценарий. Баллы Score суммируются с баллами бота.
		public int? Percent { get; set; } // Теперь не null, если IsChecked

		[NotMapped]
		public List<ExerciseCodeReview> NotDeletedReviews => Reviews.Where(r => !r.IsDeleted).ToList();

		public override void PreRemove(UlearnDb db)
		{
			db.Set<ExerciseCodeReview>().RemoveRange(Reviews);
		}
	}

	[Index(nameof(CourseId), nameof(SlideId))]
	[Index(nameof(CourseId), nameof(SlideId), nameof(UserId))]
	[Index(nameof(CourseId), nameof(SlideId), nameof(Timestamp))]
	[Index(nameof(CourseId), nameof(UserId))]
	public class AutomaticQuizChecking : AbstractAutomaticSlideChecking
	{
		/* This field is not identity and is not database-generated because EF generates Id as foreign key to UserQuizSubmission.Id */
		[Key]
		public override int Id { get; set; }

		public virtual UserQuizSubmission Submission { get; set; }

		public int Score { get; set; }

		public bool IgnoreInAttemptsCount { get; set; }
	}

	[Index(nameof(CourseId), nameof(SlideId), nameof(UserId), nameof(Timestamp))]
	[Index(nameof(CourseId), nameof(SlideId), nameof(Timestamp))]
	[Index(nameof(CourseId), nameof(IsChecked), nameof(UserId), nameof(SlideId), nameof(Timestamp))]
	[Index(nameof(CourseId), nameof(IsChecked), nameof(SlideId), nameof(Timestamp))]
	public class ManualQuizChecking : AbstractManualSlideChecking
	{
		/* This field is not identity and is not database-generated because EF generates Id as foreign key to UserQuizSubmission.Id */
		[Key]
		public override int Id { get; set; }

		public virtual UserQuizSubmission Submission { get; set; }

		public int Score { get; set; }

		public bool IgnoreInAttemptsCount { get; set; }
	}
}