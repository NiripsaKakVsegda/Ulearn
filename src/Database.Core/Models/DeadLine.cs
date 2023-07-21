using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Database.Models
{
	[Index(nameof(CourseId), nameof(GroupId))]
	[Index(nameof(CourseId), nameof(GroupId), nameof(UserIds))]
	[Index(nameof(CourseId), nameof(GroupId), nameof(UnitId), nameof(SlideType), nameof(SlideValue), nameof(UserIds))]
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

		[Required]
		public DeadLineSlideType SlideType { get; set; }
		
		[CanBeNull]
		public string SlideValue { get; set; }
		
		[CanBeNull]
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