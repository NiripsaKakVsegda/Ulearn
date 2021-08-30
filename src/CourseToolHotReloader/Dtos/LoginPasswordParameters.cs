using System.Text.Json.Serialization;

namespace CourseToolHotReloader.Dtos
{
	public class LoginPasswordParameters
	{
		[JsonPropertyName("login")]
		public string Login { get; set; }

		[JsonPropertyName("password")]
		public string Password { get; set; }
	}
}