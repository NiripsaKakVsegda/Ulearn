using System.Text.Json.Serialization;

namespace CourseToolHotReloader.Dtos
{
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
}