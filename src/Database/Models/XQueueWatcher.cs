using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class XQueueWatcher
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public string Name { get; set; }

		[Required]
		public string BaseUrl { get; set; }

		[Required(AllowEmptyStrings = true)]
		public string QueueName { get; set; }

		[Required(AllowEmptyStrings = true)]
		public string UserName { get; set; }

		[Required(AllowEmptyStrings = true)]
		public string Password { get; set; }

		public bool IsEnabled { get; set; }

		[Required]
		[StringLength(64)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }
	}
}