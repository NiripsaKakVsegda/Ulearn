using System.Text.Json.Serialization;

namespace CourseToolHotReloader.Dtos
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum ErrorType
	{
		NoErrors,
		Forbidden,
		Conflict,
		NotFound,
		CourseError
	}
}