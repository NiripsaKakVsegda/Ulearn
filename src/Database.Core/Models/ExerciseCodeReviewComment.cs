﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database.Models
{
	[Index(nameof(ReviewId), nameof(IsDeleted))]
	[Index(nameof(AddingTime))]
	public class ExerciseCodeReviewComment
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public int ReviewId { get; set; }

		public virtual ExerciseCodeReview Review { get; set; }

		[Required]
		public string Text { get; set; }

		[Required]
		[StringLength(64)]
		public string AuthorId { get; set; }

		public virtual ApplicationUser Author { get; set; }

		[Required]
		public bool IsDeleted { get; set; }

		[Required]
		public DateTime AddingTime { get; set; }
	}
}