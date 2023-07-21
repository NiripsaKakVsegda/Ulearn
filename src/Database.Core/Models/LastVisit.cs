using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Database.Models
{
	[Index(nameof(CourseId), nameof(UserId))]
	public class LastVisit : ITimedSlideAction
	{
		[Key]
		public int Id { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		[StringLength(128)]
		public string UserId { get; set; }

		[Required]
		[StringLength(64)]
		public string CourseId { get; set; }

		///<summary>Слайд, на который пользователь заходил последний раз</summary>
		[Required]
		public Guid SlideId { get; set; }

		///<summary>Последний заход</summary>
		[Required]
		public DateTime Timestamp { get; set; }
	}
}