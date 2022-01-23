using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class AdditionalContentPublication
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id { get; set; }

		[Required]
		[StringLength(100)]
		[Index("IX_AdditionalContentPublications_CourseId_GroupId", 1)]
		[Index("IX_AdditionalContentPublications_CourseId_GroupId_UnitId_SlideId", 1)]
		public string CourseId { get; set; }

		[Required]
		[Index("IX_AdditionalContentPublications_CourseId_GroupId", 2)]
		[Index("IX_AdditionalContentPublications_CourseId_GroupId_UnitId_SlideId", 2)]
		public int GroupId { get; set; }

		[Required]
		[Index("IX_AdditionalContentPublications_CourseId_GroupId_UnitId_SlideId", 3)]
		public Guid UnitId { get; set; }
		
		[Index("IX_AdditionalContentPublications_CourseId_GroupId_UnitId_SlideId", 4)]
		public Guid? SlideId { get; set; }

		[Required]
		public DateTime Date { get; set; }

		[Required]
		[StringLength(64)]
		public string AuthorId { get; set; }
	}
}