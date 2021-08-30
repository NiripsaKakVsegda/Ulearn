using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
	public class TextBlob
	{
		[Key]
		[Required]
		[StringLength(40)]
		public string Hash { get; set; }

		public string Text { get; set; }
	}
}