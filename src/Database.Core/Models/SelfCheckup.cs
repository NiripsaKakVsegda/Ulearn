using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database.Models;

[Index(nameof(UserId), nameof(CourseId), nameof(SlideId))]
[Index(nameof(UserId), nameof(CourseId), nameof(SlideId), nameof(CheckupId))]
[Index(nameof(CheckupId))]
public class SelfCheckup
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	[Required]
	[StringLength(64)]
	public string UserId { get; set; }

	[Required]
	public string CourseId { get; set; }
	
	[Required]
	public Guid SlideId { get; set; }
	
	[Required]
	public bool IsChecked { get; set; }
	
	[Required]
	public string CheckupId { get; set; }
}