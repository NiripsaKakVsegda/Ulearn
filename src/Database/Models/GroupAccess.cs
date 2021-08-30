using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class GroupAccess
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[Index("IDX_GroupAccess_ByGroup")]
		[Index("IDX_GroupAccess_ByGroupAndIsEnabled", 1)]
		[Index("IDX_GroupAccess_ByGroupUserAndIsEnabled", 1)]
		public int GroupId { get; set; }

		public virtual Group Group { get; set; }

		[StringLength(64)]
		[Index("IDX_GroupAccess_ByUser")]
		[Index("IDX_GroupAccess_ByGroupUserAndIsEnabled", 2)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[StringLength(64)]
		public string GrantedById { get; set; }

		public virtual ApplicationUser GrantedBy { get; set; }

		[Required]
		public GroupAccessType AccessType { get; set; }

		[Index("IDX_GroupAccess_ByGrantTime")]
		public DateTime GrantTime { get; set; }

		[Required]
		[Index("IDX_GroupAccess_ByGroupAndIsEnabled", 2)]
		[Index("IDX_GroupAccess_ByGroupUserAndIsEnabled", 3)]
		public bool IsEnabled { get; set; }
	}

	public enum GroupAccessType : short
	{
		FullAccess = 1,

		/* Can't be stored in database. Only for internal needs */
		Owner = 100,
	}
}