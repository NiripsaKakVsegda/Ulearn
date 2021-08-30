using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class GroupMember
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[Index("IDX_GroupMember_MembersByGroup")]
		public int GroupId { get; set; }

		public virtual Group Group { get; set; }

		[Required]
		[StringLength(64)]
		[Index("IDX_GroupMember_GroupByMember")]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		public DateTime? AddingTime { get; set; }
	}
}