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
		[Index("IDX_FeedUpdateTimestamp_ByUser")]
		[Index("IDX_FeedUpdateTimestamp_ByUserAndTransport", 1)]
		public string UserId { get; set; }

		[Index("IDX_FeedUpdateTimestamp_ByUserAndTransport", 2)]
		public int? TransportId { get; set; }

		public virtual NotificationTransport Transport { get; set; }

		[Index("IDX_FeedUpdateTimestamp_ByTimestamp")]
		public DateTime Timestamp { get; set; }
	}
}