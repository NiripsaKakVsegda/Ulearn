using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CourseToolHotReloader
{
	internal class CourseConfig
	{
		[JsonPropertyName("courseToolHotReloader")]
		public CourseToolHotReloaderCourseConfig CourseToolHotReloader { get; set; }
	}

	internal class CourseToolHotReloaderCourseConfig
	{
		[JsonPropertyName("excludeCriterias")]
		public List<string> ExcludeCriterias { get; set; } // Формат описан в ZipUtils.GetExcludeRegexps
	}
}