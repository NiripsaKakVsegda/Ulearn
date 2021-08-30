using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class FeedViewTimestamp
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[StringLength(64)]
		public string UserId { get; set; }

		public int? TransportId { get; set; }

		public virtual NotificationTransport Transport { get; set; }

		public DateTime Timestamp { get; set; }
	}
}