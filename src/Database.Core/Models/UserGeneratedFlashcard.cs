using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Database.Models;

[Index(nameof(CourseId), nameof(OwnerId))]
[Index(nameof(CourseId), nameof(ModerationStatus))]
[Index(nameof(CourseId), nameof(UnitId), nameof(ModerationStatus))]
public class UserGeneratedFlashcard
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; set; }

	[Required]
	public string OwnerId { get; set; }

	public virtual ApplicationUser Owner { get; set; }

	[Required]
	[StringLength(100)]
	public string CourseId { get; set; }

	[Required]
	public Guid UnitId { get; set; }

	[Required]
	public string Question { get; set; }

	[Required]
	public string Answer { get; set; }

	[Required]
	public DateTime LastUpdateTimestamp { get; set; }

	[Required]
	public FlashcardModerationStatus ModerationStatus { get; set; }

	[CanBeNull]
	public string ModeratorId { get; set; }

	[CanBeNull]
	public virtual ApplicationUser Moderator { get; set; }

	[CanBeNull]
	public DateTime? ModerationTimestamp { get; set; }
}

public enum FlashcardModerationStatus
{
	New = 0,
	Approved = 1,
	Declined = 2
}