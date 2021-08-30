using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class GroupLabel
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(64)]
		[Index("IDX_GroupLabel_ByOwner")]
		[Index("IDX_GroupLabel_ByOwnerAndIsDeleted", 1)]
		public string OwnerId { get; set; }

		public virtual ApplicationUser Owner { get; set; }

		[StringLength(100)]
		public string Name { get; set; }

		[StringLength(6)]
		public string ColorHex { get; set; }

		[Required]
		[Index("IDX_GroupLabel_ByOwnerAndIsDeleted", 2)]
		public bool IsDeleted { get; set; }
	}

	public class LabelOnGroup
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[Index("IDX_LabelOnGroup_ByGroup")]
		[Index("IDX_LabelOnGroup_ByGroupAndLabel", 1, IsUnique = true)]
		public int GroupId { get; set; }

		public virtual Group Group { get; set; }

		[Required]
		[Index("IDX_LabelOnGroup_ByLabel")]
		[Index("IDX_LabelOnGroup_ByGroupAndLabel", 2, IsUnique = true)]
		public int LabelId { get; set; }

		public virtual GroupLabel Label { get; set; }
	}
}