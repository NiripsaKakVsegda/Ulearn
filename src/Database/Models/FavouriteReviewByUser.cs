using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class FavouriteReviewByUser
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public int FavouriteReviewId { get; set; }

		public virtual FavouriteReview FavouriteReview { get; set; }

		[Required]
		[StringLength(100)]
		[Index("IDX_FavouriteReviewByUser_CourseId_SlideId_UserId", 1)]
		[Index("IDX_FavouriteReviewByUser_CourseId_SlideId_Timestamp", 1)]
		public string CourseId { get; set; }

		[Required]
		[Index("IDX_FavouriteReviewByUser_CourseId_SlideId_UserId", 2)]
		[Index("IDX_FavouriteReviewByUser_CourseId_SlideId_Timestamp", 2)]
		public Guid SlideId { get; set; }

		[Required]
		[StringLength(64)]
		[Index("IDX_FavouriteReviewByUser_CourseId_SlideId_UserId", 3)]
		public string UserId { get; set; }

		[Required]
		[Index("IDX_FavouriteReviewByUser_CourseId_SlideId_Timestamp", 3)]
		public DateTime Timestamp { get; set; }
	}
}