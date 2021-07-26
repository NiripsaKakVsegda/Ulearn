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
		public string CourseId { get; set; }

		[Required]
		public Guid SlideId { get; set; }

		[Required(AllowEmptyStrings = false)]
		public string Text { get; set; }
		
		public virtual IList<FavouriteReviewByUser> FavouriteReviewsByUser { get; set; }
	}
}