using System;
using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
	public class ExerciseAttemptedUsersCount
	{
		public const string ViewName = "ExerciseAttemptedUsersCounts";

		[Required]
		public string CourseId { get; set; }

		[Required]
		public Guid SlideId { get; set; }

		[Required]
		public int AttemptedUsersCount { get; set; }
	}

	public class ExerciseUsersWithRightAnswerCount
	{
		public const string ViewName = "ExerciseUsersWithRightAnswerCounts";

		[Required]
		public string CourseId { get; set; }

		[Required]
		public Guid SlideId { get; set; }

		[Required]
		public int UsersWithRightAnswerCount { get; set; }
	}
}