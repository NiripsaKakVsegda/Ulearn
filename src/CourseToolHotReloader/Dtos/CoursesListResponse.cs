using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CourseToolHotReloader.Dtos
{
	public class CoursesListResponse
	{
		[JsonPropertyName("courses")]
		public List<ShortCourseInfo> Courses { get; set; }
	}

	public class ShortCourseInfo
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("title")]
		public string Title { get; set; }

		[JsonPropertyName("apiUrl")]
		public string ApiUrl { get; set; }

		[JsonPropertyName("isTempCourse")]
		public bool IsTempCourse { get; set; }
	}
}