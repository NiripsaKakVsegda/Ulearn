using System;
using System.Text.Json.Serialization;

namespace CourseToolHotReloader.Dtos
{
	public class TempCourseUpdateResponse
	{
		[JsonPropertyName("message")]
		public string Message { get; set; }

		[JsonPropertyName("errorType")]
		public ErrorType ErrorType { get; set; }

		[JsonPropertyName("lastUploadTime")]
		public DateTime LastUploadTime { get; set; }
	}
}