using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Database.Models
{
	public class UserFlashcardsVisit
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		[StringLength(100)]
		public string CourseId { get; set; }

		[Required]
		public Guid UnitId { get; set; }

		[Required]
		[StringLength(64)]
		public string FlashcardId { get; set; }

		[Required]
		public Rate Rate { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }
	}

	[JsonConverter(typeof(StringEnumConverter), true)]
	public enum Rate
	{
		NotRated,
		Rate1,
		Rate2,
		Rate3,
		Rate4,
		Rate5
	}
}