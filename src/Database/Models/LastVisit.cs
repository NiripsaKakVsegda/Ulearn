using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class LastVisit : ITimedSlideAction
	{
		[Key]
		public int Id { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		[StringLength(128)]
		[Index("IDX_LastVisits_ByCourseAndUser", 2)]
		public string UserId { get; set; }

		[Required]
		[StringLength(100)]
		[Index("IDX_LastVisits_ByCourseAndUser", 1)]
		public string CourseId { get; set; }

		///<summary>Слайд, на который пользователь заходил последний раз</summary>
		[Required]
		public Guid SlideId { get; set; }

		///<summary>Последний заход</summary>
		[Required]
		public DateTime Timestamp { get; set; }
	}
}