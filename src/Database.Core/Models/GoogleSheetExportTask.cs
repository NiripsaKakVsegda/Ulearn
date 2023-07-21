using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database.Models
{
	[Index(nameof(CourseId), nameof(AuthorId))]
	public class GoogleSheetExportTask
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		
		[Required]
		[StringLength(100)]
		public string CourseId { get; set; }

		
		public virtual IList<GoogleSheetExportTaskGroup> Groups { get; set; }

		[Required]
		[StringLength(64)]
		public string AuthorId { get; set; }

		public virtual ApplicationUser Author { get; set; }

		[Required]
		public bool IsVisibleForStudents { get; set; }

		public DateTime? RefreshStartDate { get; set; }
		
		public DateTime? RefreshEndDate { get; set; }
		
		public DateTime? LastUpdateDate { get; set; }
		
		public string LastUpdateErrorMessage { get; set; }
		
		public int? RefreshTimeInMinutes { get; set; }
		
		[Required]
		public string SpreadsheetId { get; set; }
		
		[Required]
		public int ListId { get; set; }
	}
}