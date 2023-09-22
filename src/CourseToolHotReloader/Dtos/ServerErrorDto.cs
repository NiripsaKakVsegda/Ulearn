using System.Text.Json.Serialization;
#nullable disable

namespace CourseToolHotReloader.Dtos;

// ReSharper disable once ClassNeverInstantiated.Global
public class ServerErrorDto
{
	[JsonPropertyName("status")]
	public string Status { get; set; }

	[JsonPropertyName("message")]
	public string Message { get; set; }
		
	[JsonPropertyName("traceId")]
	public string TraceId { get; set; }

	[JsonPropertyName("timestamp")]
	public string Timestamp { get; set; }
}