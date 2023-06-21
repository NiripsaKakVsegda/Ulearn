using System.Text.Json.Serialization;

namespace CourseToolHotReloader.Dtos;

public class TokenResponseDto
{
	[JsonPropertyName("token")]
	public string Token { get; set; }
}