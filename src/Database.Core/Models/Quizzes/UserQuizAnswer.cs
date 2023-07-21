using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Database.Models.Quizzes
{
	[Index(nameof(SubmissionId), nameof(BlockId))]
	[Index(nameof(ItemId))]
	public class UserQuizAnswer
	{
		[Required]
		[Key]
		public int Id { get; set; }

		public int SubmissionId { get; set; }

		public virtual UserQuizSubmission Submission { get; set; }

		[StringLength(64)]
		public string BlockId { get; set; }

		[StringLength(64)]
		public string ItemId { get; set; }

		[StringLength(8192)]
		public string Text { get; set; }

		[Required]
		// Корректный ли вариант ItemId
		public bool IsRightAnswer { get; set; }

		[Required]
		// Количество баллов за весь блок
		public int QuizBlockScore { get; set; }

		[Required]
		// Максимально возможное количество баллов за весь блок
		public int QuizBlockMaxScore { get; set; }

		public bool IsQuizBlockScoredMaximum => QuizBlockScore == QuizBlockMaxScore;
	}
}