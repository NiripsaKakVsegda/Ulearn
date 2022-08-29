using System;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace Database.Models;

public class UserRole
{
	[Required]
	[Key]
	public int Id { get; set; }

	public virtual ApplicationUser User { get; set; }

	[StringLength(64)]
	public string UserId { get; set; }

	[Required]
	public string CourseId { get; set; }

	[Required]
	public CourseRole Role { get; set; }

	[CanBeNull]
	public string GrantedById { get; set; }

	[CanBeNull]
	public DateTime? GrantTime { get; set; }

	[CanBeNull]
	public bool? IsEnabled { get; set; }

	[CanBeNull]
	public string Comment { get; set; }
}