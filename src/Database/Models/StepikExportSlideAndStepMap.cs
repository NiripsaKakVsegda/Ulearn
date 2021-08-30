using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class StepikExportSlideAndStepMap
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		[Index("IDX_StepikExportSlideAndStepMap_ByUlearnCourseId")]
		[Index("IDX_StepikExportSlideAndStepMap_ByUlearnCourseIdAndStepikCourseId", 1)]
		[Index("IDX_StepikExportSlideAndStepMap_ByUlearnCourseIdAndSlideId", 1)]
		public string UlearnCourseId { get; set; }

		[Required]
		[Index("IDX_StepikExportSlideAndStepMap_ByUlearnCourseIdAndStepikCourseId", 2)]
		public int StepikCourseId { get; set; }

		[Required]
		[Index("IDX_StepikExportSlideAndStepMap_ByUlearnCourseIdAndSlideId", 2)]
		public Guid SlideId { get; set; }

		[Required]
		public int StepId { get; set; }

		[Required]
		public string SlideXml { get; set; }
	}
}