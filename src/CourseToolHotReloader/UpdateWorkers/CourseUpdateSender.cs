using System.Threading;
using System.Threading.Tasks;
using CourseToolHotReloader.ApiClient;
using CourseToolHotReloader.Application;
using CourseToolHotReloader.Configs;
using CourseToolHotReloader.Dtos;
using CourseToolHotReloader.Infrastructure;

namespace CourseToolHotReloader.UpdateWorkers;

public interface ICourseUpdateSender
{
	Task SendCourseUpdatesAsync(CourseInfo course, ICourseUpdateQuery updateQuery, CancellationToken token = default);
	Task SendFullCourseAsync(CourseInfo course, CancellationToken token = default);
}

public class CourseUpdateSender : ICourseUpdateSender
{
	private readonly IApplication application;
	private readonly ICoursesManager coursesManager;
	private readonly IUlearnApiClient ulearnApiClient;
	private readonly ILogManager logManager;

	public CourseUpdateSender(
		IApplication application,
		ICoursesManager coursesManager,
		IUlearnApiClient ulearnApiClient,
		ILogManager logManager
	)
	{
		this.application = application;
		this.coursesManager = coursesManager;
		this.ulearnApiClient = ulearnApiClient;
		this.logManager = logManager;
	}

	public async Task SendCourseUpdatesAsync(CourseInfo course, ICourseUpdateQuery updateQuery, CancellationToken token = default)
	{
		using var locker = updateQuery.LockUpdates();

		if (locker.Updated.Count == 0 && locker.Deleted.Count == 0)
			return;

		logManager.LogInfoWithTime($"Загружаем изменения в курсе {course.CourseId} на ulearn");

		var result = await ulearnApiClient.SendCourseUpdates(
			course.Path,
			locker.Updated,
			locker.Deleted,
			course.CourseId,
			course.ExcludeCriterias,
			token
		).RefineError($"Ошибка загрузки изменений в курсе {course.CourseId}");
		token.ThrowIfCancellationRequested();

		if (result is { IsSuccess: true, Value.ErrorType: ErrorType.NoErrors })
		{
			logManager.LogInfoWithTime($"Изменения в курсе {course.CourseId} загружены без ошибок");
			course.CourseState = CourseState.OK;
			locker.ClearOldUpdatesOnUnlock = true;
		}
		else
		{
			application.NotifyError();
			if (result is { IsSuccess: true, Value.ErrorType: ErrorType.CourseError })
			{
				logManager.LogErrorWithTime(result.Value.Message ?? "Ошибка парсинга XML");
				course.CourseState = CourseState.CourseValidationError;
				locker.ClearOldUpdatesOnUnlock = false;
			}
			else
			{
				var message = result.IsSuccess
					? result.Value.Message ?? result.Value.ErrorType.ToString()
					: result.Error;
				logManager.LogErrorWithTime(message, result.Exception);
				course.CourseState = CourseState.FatalError;
				coursesManager.StopWatchingCourse(course);
				locker.ClearOldUpdatesOnUnlock = true;
			}
		}

		application.RegisterCourseUpdate(course);
	}

	public async Task SendFullCourseAsync(CourseInfo course, CancellationToken token = default)
	{
		logManager.LogInfoWithTime($"Загружаем курс {course.CourseId} на ulearn");

		var result = await ulearnApiClient.SendFullCourse(course.Path, course.CourseId, course.ExcludeCriterias, token)
			.RefineError($"Ошибка загрузки курса {course.CourseId}");
		token.ThrowIfCancellationRequested();

		if (result.IsSuccess)
		{
			logManager.LogInfoWithTime($"Курс {course.CourseId} загружен без ошибок");
			course.CourseState = CourseState.OK;
		}
		else
		{
			application.NotifyError();
			if (result is { IsSuccess: true, Value.ErrorType: ErrorType.CourseError })
			{
				logManager.LogErrorWithTime(result.Value.Message ?? "Ошибка парсинга XML");
				course.CourseState = CourseState.CourseValidationError;
			}
			else
			{
				var message = result.IsSuccess
					? result.Value.Message ?? result.Value.ErrorType.ToString()
					: result.Error;
				logManager.LogErrorWithTime(message, result.Exception);
				course.CourseState = CourseState.FatalError;
				coursesManager.StopWatchingCourse(course);
			}
		}

		application.RegisterCourseUpdate(course);
	}
}