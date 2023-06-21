using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using CourseToolHotReloader.Infrastructure;

namespace CourseToolHotReloader.Configs;

internal class CourseConfig
{
	[JsonPropertyName("courseToolHotReloader")]
	public CourseToolHotReloaderCourseConfig? CourseToolHotReloader { get; set; }

	public static Result<CourseConfig> ReadCourseConfig(string directory)
	{
		var file = Path.Combine(directory, "CourseConfig.json");
		if (!File.Exists(file))
			return Result.Fail<CourseConfig>("File not exist");

		return Result.Of(() => File.ReadAllText(file))
			.Then(json => JsonSerializer.Deserialize<CourseConfig>(json))
			.Then(cfg => cfg?.AsResult() ?? Result.Fail<CourseConfig>("Can't deserialize file"))
			.RefineError("Не удалось прочитать конфиг курса");
	}
}

internal class CourseToolHotReloaderCourseConfig
{
	[JsonPropertyName("excludeCriterias")]
	public List<string>? ExcludeCriterias { get; set; } // Формат описан в ZipUtils.GetExcludeRegexps
}