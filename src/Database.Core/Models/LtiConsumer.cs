using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	/* For backward compatibility: EF Core changed table naming convention.
	   See https://github.com/aspnet/Announcements/issues/167 for details */
	[Table("LtiConsumers")]
	public class LtiConsumer
	{
		[Key]
		public int ConsumerId { get; set; }

		[Required]
		[StringLength(64)]
		public string Name { get; set; }

		[Required]
		[StringLength(64)]
		public string Key { get; set; }

		[Required]
		[StringLength(64)]
		public string Secret { get; set; }
	}
}