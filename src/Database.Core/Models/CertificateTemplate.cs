using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Database.Models
{
	[Index(nameof(CourseId))]
	public class CertificateTemplate
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		[StringLength(100)]
		public string CourseId { get; set; }

		[Required]
		public string Name { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }

		[Required]
		public bool IsDeleted { get; set; }

		[Required]
		[StringLength(128)]
		public string ArchiveName { get; set; }

		public virtual ICollection<Certificate> Certificates { get; set; }

		public bool IsUploaded => !string.IsNullOrEmpty(ArchiveName);
	}
}