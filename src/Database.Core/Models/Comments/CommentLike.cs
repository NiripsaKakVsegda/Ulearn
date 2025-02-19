﻿using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Database.Models.Comments
{
	[Index(nameof(UserId), nameof(CommentId), IsUnique = true)]
	[Index(nameof(CommentId))]
	public class CommentLike
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[StringLength(64)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		public int CommentId { get; set; }

		public virtual Comment Comment { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }
	}
}