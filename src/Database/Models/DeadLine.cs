using System;
using System.Collections.Generic;
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
		[Index("IX_DeadLines_CourseId_GroupId_UserIds", 1)]
		[Index("IX_DeadLines_CourseId_GroupId_UnitId_SlideType_SlideValue_User~", 1)]
		public string CourseId { get; set; }

		[Required]
		[Index("IX_DeadLines_CourseId_GroupId", 2)]
		[Index("IX_DeadLines_CourseId_GroupId_UserIds", 2)]
		[Index("IX_DeadLines_CourseId_GroupId_UnitId_SlideType_SlideValue_User~", 2)]
		public int GroupId { get; set; }

		[Required]
		[Index("IX_DeadLines_CourseId_GroupId_UnitId_SlideType_SlideValue_User~", 3)]
		public Guid UnitId { get; set; }

		[Index("IX_DeadLines_CourseId_GroupId_UnitId_SlideType_SlideValue_User~", 4)]
		[Required]
		public DeadLineSlideType SlideType { get; set; }
		
		[Index("IX_DeadLines_CourseId_GroupId_UnitId_SlideType_SlideValue_User~", 5)]
		public string SlideValue { get; set; }

		[Index("IX_DeadLines_CourseId_GroupId_UnitId_SlideType_SlideValue_User~", 6)]
		public List<Guid> UserIds { get; set; }

		[Required]
		public int ScorePercent { get; set; }
	}
	
	public enum DeadLineSlideType
	{
		All,
		SlideId,
		ScoringGroupId
	}
}