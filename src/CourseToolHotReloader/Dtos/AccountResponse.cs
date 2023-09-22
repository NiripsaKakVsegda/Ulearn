using System.Text.Json.Serialization;

namespace CourseToolHotReloader.Dtos;

#nullable disable
// ReSharper disable ClassNeverInstantiated.Global

public class AccountResponse
{
	[JsonPropertyName("user")]
	public ShortUserInfo User { get; set; }
}

public class ShortUserInfo
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("login")]
	public string Login { get; set; }
}