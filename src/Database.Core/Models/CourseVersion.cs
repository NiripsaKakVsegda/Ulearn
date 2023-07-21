using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Database.Models
{
	[Index(nameof(CourseId), nameof(PublishTime))]
	[Index(nameof(CourseId), nameof(LoadingTime))]
	public class CourseVersion
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		[StringLength(100)]
		public string CourseId { get; set; }

		[Required]
		public DateTime LoadingTime { get; set; }

		public DateTime? PublishTime { get; set; }

		[Required]
		[StringLength(64)]
		public string AuthorId { get; set; }

		public virtual ApplicationUser Author { get; set; }

		public string RepoUrl { get; set; }

		[StringLength(40)]
		public string CommitHash { get; set; }

		public string Description { get; set; }

		// Устанавливается из настройки курса или как пусть единственного course.xml, если курс загружен из репозитория
		public string PathToCourseXml { get; set; }

		[Required]
		public string CourseName { get; set; }
	}
}