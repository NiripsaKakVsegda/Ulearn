using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

public abstract class GroupBase
{
	[Required]
	public virtual GroupType GroupType { get; }
	
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	[Required]
	[StringLength(100)]
	public string CourseId { get; set; }

	[Required]
	[StringLength(300)]
	public string Name { get; set; }

	[Required]
	[StringLength(64)]
	public string OwnerId { get; set; }

	public virtual ApplicationUser Owner { get; set; }

	[Required]
	public bool IsDeleted { get; set; }

	[Required]
	/* Архивная группа не учитываются в фильтрах «Мои группы» и всегда показывается позже неархивных */
	public bool IsArchived { get; set; }

	[Required]
	public Guid InviteHash { get; set; }

	[Required]
	public bool IsInviteLinkEnabled { get; set; }

	public DateTime? CreateTime { get; set; } // При разархивировании обновляется
}