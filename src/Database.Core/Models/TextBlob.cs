using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	/* For backward compatibility: EF Core changed table naming convention.
	   See https://github.com/aspnet/Announcements/issues/167 for details */
	[Table("TextBlobs")]
	public class TextBlob
	{
		[Key]
		[Required]
		[StringLength(40)]
		public string Hash { get; set; }

		public string Text { get; set; }
	}
}