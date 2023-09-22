using System.Text.Json.Serialization;

namespace CourseToolHotReloader.Dtos;
#nullable disable
// ReSharper disable once ClassNeverInstantiated.Global
public class TokenResponseDto
{
	[JsonPropertyName("token")]
	public string Token { get; set; }
}