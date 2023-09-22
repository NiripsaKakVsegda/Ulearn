using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourseToolHotReloader.Application;
using CourseToolHotReloader.Configs;
using CourseToolHotReloader.Dtos;
using CourseToolHotReloader.Infrastructure;
using Vostok.Logging.Abstractions;

namespace CourseToolHotReloader.ApiClient;

public interface IUlearnApiClient
{
	Task<Result<string>> RenewToken(CancellationToken token = default);
	Task<Result<ShortUserInfo>> GetShortUserInfo(CancellationToken token = default);
	Task<Result<bool>> HasCourse(string courseId, CancellationToken token = default);
	Task<Result<TempCourseUpdateResponse>> CreateCourse(string courseId, CancellationToken token = default);

	Task<Result<TempCourseUpdateResponse>> SendCourseUpdates(
		string path,
		ICollection<CourseUpdate> updates,
		ICollection<CourseUpdate> deletedFiles,
		string courseId,
		List<string> excludeCriterias,
		CancellationToken token = default
	);

	Task<Result<TempCourseUpdateResponse>> SendFullCourse(
		string path,
		string courseId,
		List<string> excludeCriterias,
		CancellationToken token = default
	);
}

internal class UlearnApiClient : IUlearnApiClient
{
	private static ILog Log => LogProvider.Get();
	private readonly IConfig config;
	private readonly IHttpMethods httpMethods;

	public UlearnApiClient(IConfig config, IHttpMethods httpMethods)
	{
		this.config = config;
		this.httpMethods = httpMethods;
	}

	public async Task<Result<string>> RenewToken(CancellationToken token = default)
	{
		var accountTokenResponseDto = await httpMethods.RenewToken(token);
		return accountTokenResponseDto
			.Then(response => response.Token);
	}

	public Task<Result<ShortUserInfo>> GetShortUserInfo(CancellationToken token = default)
	{
		return httpMethods.GetUserInfo(token);
	}

	public async Task<Result<bool>> HasCourse(string courseId, CancellationToken token = default)
	{
		var coursesList = await httpMethods.GetCoursesList(token);
		return coursesList
			.Then(list => list.Courses.Any(c => string.Equals(c.Id, courseId, StringComparison.OrdinalIgnoreCase)));
	}

	public async Task<Result<TempCourseUpdateResponse>> SendCourseUpdates(
		string path,
		ICollection<CourseUpdate> updates,
		ICollection<CourseUpdate> deletedFiles,
		string courseId,
		List<string> excludeCriterias,
		CancellationToken token = default
	)
	{
		try
		{
			Log.Info($"Пакую изменения курса {courseId} в zip-архив из {path}...");
			using var ms = ZipUpdater.CreateZipByUpdates(path, updates, deletedFiles, excludeCriterias);
			Log.Info($"Отправляю изменения курса {courseId} на сервер. Размер zip: {Math.Ceiling(ms.Length / 1024.0)} Kb");
			return await httpMethods.UploadCourseChanges(ms, courseId, token);
		}
		catch (Exception e)
		{
			return Result.Fail<TempCourseUpdateResponse>($"Ошибка при создании архива с изменениями. {e.GetMessage(config.ApiUrl)}", e);
		}
	}

	public async Task<Result<TempCourseUpdateResponse>> SendFullCourse(
		string path,
		string courseId,
		List<string> excludeCriterias,
		CancellationToken token = default
	)
	{
		try
		{
			Log.Info($"Пакую курс {courseId} в zip-архив из {path}...");
			using var ms = ZipUpdater.CreateZipByFolder(path, excludeCriterias);
			Log.Info($"Отправляю курс {courseId} на сервер. Размер zip: {Math.Ceiling(ms.Length / 1024.0)} Kb");
			return await httpMethods.UploadFullCourse(ms, courseId, token);
		}
		catch (Exception e)
		{
			return Result.Fail<TempCourseUpdateResponse>($"Ошибка при создании архива с курсом. {e.GetMessage(config.ApiUrl)}");
		}
	}

	public Task<Result<TempCourseUpdateResponse>> CreateCourse(string courseId, CancellationToken token = default)
	{
		return httpMethods.CreateCourse(courseId, token);
	}
}