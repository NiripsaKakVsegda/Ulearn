using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class FavouriteReview
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		[Index("IDX_FavouriteReviewByUser_CourseId_SlideId", 1)]
		public string CourseId { get; set; }

		[Required]
		[Index("IDX_FavouriteReviewByUser_CourseId_SlideId", 2)]
		public Guid SlideId { get; set; }

		[Required(AllowEmptyStrings = false)]
		[Index("IDX_FavouriteReviewByUser_Text", IsUnique = true)]
		public string Text { get; set; }

		public virtual IList<FavouriteReviewByUser> FavouriteReviewsByUser { get; set; }
	}
}