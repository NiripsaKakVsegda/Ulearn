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
		[Index("IX_DeadLines_CourseId_GroupId", 1)]
		[Index("IX_DeadLines_CourseId_UserId", 1)]
		[Index("IX_DeadLines_CourseId_GroupId_UnitId_SlideId_UserId", 1)]
		public string CourseId { get; set; }

		[Required]
		[Index("IX_DeadLines_CourseId_GroupId", 2)]
		[Index("IX_DeadLines_CourseId_GroupId_UnitId_SlideId_UserId", 2)]
		public int GroupId { get; set; }

		[Required]
		[Index("IX_DeadLines_CourseId_GroupId_UnitId_SlideId_UserId", 3)]
		public Guid UnitId { get; set; }

		[Index("IX_DeadLines_CourseId_GroupId_UnitId_SlideId_UserId", 4)]
		public Guid? SlideId { get; set; }

		[Index("IX_DeadLines_CourseId_GroupId_UnitId_SlideId_UserId", 5)]
		[Index("IX_DeadLines_CourseId_UserId", 2)]
		public Guid? UserId { get; set; }

		[Required]
		public int ScorePercent { get; set; }
	}
}