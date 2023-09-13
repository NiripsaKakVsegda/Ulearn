using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database.Models
{
	[Index(nameof(GroupId))]
	[Index(nameof(UserId))]
	public class GroupMember
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public int GroupId { get; set; }

		public virtual GroupBase Group { get; set; }

		[Required]
		[StringLength(64)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		public DateTime? AddingTime { get; set; }
	}
}