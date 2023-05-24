using System.Collections.Generic;

namespace CourseToolHotReloader.Configs;

public class CourseInfo
{
	public string Path { get; set; } = null!;
	public string CourseId { get; set; } = null!;
	public List<string> ExcludeCriterias { get; set; } = null!;
	public bool SendFullArchive { get; set; }
	public CourseState CourseState { get; set; }
}