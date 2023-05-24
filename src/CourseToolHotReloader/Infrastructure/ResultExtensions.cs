using System.Threading.Tasks;
using CourseToolHotReloader.Configs;
using CourseToolHotReloader.Dtos;

namespace CourseToolHotReloader.Infrastructure;

public static class ResultExtensions
{
	public static Task<Result<TempCourseUpdateResponse>> ThenCheckNoErrors(this Task<Result<TempCourseUpdateResponse>> result) =>
		result.Then(response => response.ErrorType is ErrorType.NoErrors or ErrorType.CourseError
			? response.AsResult()
			: Result.Fail<TempCourseUpdateResponse>(response.Message ?? response.ErrorType.ToString())
		);
}