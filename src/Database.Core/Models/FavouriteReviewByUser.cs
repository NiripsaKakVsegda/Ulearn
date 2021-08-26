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
		public string CourseId { get; set; }

		[Required]
		public Guid SlideId { get; set; }
		
		[Required]
		[StringLength(64)]
		public string UserId { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }
	}
}