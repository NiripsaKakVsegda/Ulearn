using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class DeadLine
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id { get; set; }

		[Required]
		public DateTime Date { get; set; }

		[Required]
		[StringLength(100)]
		public string CourseId { get; set; }

		[Required]
		public int GroupId { get; set; }

		[Required]
		public Guid UnitId { get; set; }

		public Guid? SlideId { get; set; }

		public Guid? UserId { get; set; }

		[Required]
		public int ScorePercent { get; set; }
	}
}