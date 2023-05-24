using System.IO;
using System.Threading.Tasks;
using CourseToolHotReloader.ApiClient;
using CourseToolHotReloader.Configs;
using CourseToolHotReloader.Dtos;
using CourseToolHotReloader.Infrastructure;
using CourseToolHotReloader.UpdateWorkers;
using Vostok.Logging.Abstractions;

namespace CourseToolHotReloader.Application;

public interface ICoursesManager
{
	Task<Result<None>> UploadCourse(CourseInfo course, string userId);
	Result<None> StartWatchingCourse(CourseInfo course);
	void StopWatchingCourse(CourseInfo course);
}

public class CoursesManager : ICoursesManager
{
	private readonly ICoursesWatcher coursesWatcher;
	private readonly IUlearnApiClient ulearnApiClient;

	private static ILog Log => LogProvider.Get().ForContext<CoursesManager>();

	public CoursesManager(ICoursesWatcher coursesWatcher, IUlearnApiClient ulearnApiClient)
	{
		this.coursesWatcher = coursesWatcher;
		this.ulearnApiClient = ulearnApiClient;
	}

	public Task<Result<None>> UploadCourse(CourseInfo course, string userId)
	{
		return CreateCourseIfNotExist(course, userId)
			.Then(_ => ulearnApiClient.SendFullCourse(course.Path, course.CourseId, course.ExcludeCriterias))
			.Then(response => HandleSendCourseResponse(response, course))
			.OnFail((_, _) =>
			{
				course.CourseState = CourseState.FatalError;
				StopWatchingCourse(course);
			});
	}

	public Result<None> StartWatchingCourse(CourseInfo course)
	{
		if (coursesWatcher.HasWatcherFor(course))
			return Result.Ok();

		if (!Directory.Exists(course.Path))
		{
			course.CourseState = CourseState.FatalError;
			return Result.Fail<None>($"Папка {course.Path} не найдена!");
		}

		coursesWatcher.AddWatcher(course);
		return Result.Ok();
	}

	public void StopWatchingCourse(CourseInfo course)
	{
		if (coursesWatcher.HasWatcherFor(course))
			coursesWatcher.RemoveWatcher(course);
	}

	private Task<Result<None>> CreateCourseIfNotExist(CourseInfo course, string userId)
	{
		var tempCourseId = $"{course.CourseId}_{userId}";
		return ulearnApiClient.HasCourse(tempCourseId)
			.Then(isExist => isExist
				? Task.CompletedTask
				: ulearnApiClient.CreateCourse(course.CourseId)
			);
	}

	private void HandleSendCourseResponse(TempCourseUpdateResponse response, CourseInfo course)
	{
		course.CourseState = response.ErrorType switch
		{
			ErrorType.NoErrors => CourseState.OK,
			ErrorType.CourseError => CourseState.CourseValidationError,
			_ => CourseState.FatalError
		};

		if (course.CourseState is CourseState.FatalError)
			StopWatchingCourse(course);
		else
			StartWatchingCourse(course);
	}
}