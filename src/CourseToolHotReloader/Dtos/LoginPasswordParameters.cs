using System.Text.Json.Serialization;

namespace CourseToolHotReloader.Dtos;

#nullable disable
public class LoginPasswordParameters
{
	[JsonPropertyName("login")]
	public string Login { get; set; }

	[JsonPropertyName("password")]
	public string Password { get; set; }
}