using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Database.Models
{
	[Index(nameof(GroupId))]
	[Index(nameof(GroupId), nameof(IsEnabled))]
	[Index(nameof(GroupId), nameof(UserId), nameof(IsEnabled))]
	[Index(nameof(UserId))]
	[Index(nameof(GrantTime))]
	public class GroupAccess
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public int GroupId { get; set; }

		public virtual SingleGroup Group { get; set; }

		[StringLength(64)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[StringLength(64)]
		public string GrantedById { get; set; }

		public virtual ApplicationUser GrantedBy { get; set; }

		[Required]
		public GroupAccessType AccessType { get; set; }

		public DateTime GrantTime { get; set; }

		[Required]
		public bool IsEnabled { get; set; }
	}

	[JsonConverter(typeof(StringEnumConverter), true)]
	public enum GroupAccessType : short
	{
		FullAccess = 1,

		/* Can't be stored in database. Only for internal usage */
		Owner = 100,
	}
}