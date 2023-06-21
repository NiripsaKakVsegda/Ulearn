using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CourseToolHotReloader.Configs;
using CourseToolHotReloader.Dtos;
using CourseToolHotReloader.Infrastructure;
using CourseToolHotReloader.Infrastructure.Exceptions;

namespace CourseToolHotReloader.ApiClient;

public interface IHttpMethods
{
	Task<Result<TokenResponseDto>> RenewToken(CancellationToken token = default);
	Task<Result<ShortUserInfo>> GetUserInfo(CancellationToken token = default);
	Task<Result<CoursesListResponse>> GetCoursesList(CancellationToken token = default);
	Task<Result<TempCourseUpdateResponse>> CreateCourse(string id, CancellationToken token = default);
	Task<Result<TempCourseUpdateResponse>> UploadCourseChanges(MemoryStream memoryStream, string id, CancellationToken token = default);
	Task<Result<TempCourseUpdateResponse>> UploadFullCourse(MemoryStream memoryStream, string id, CancellationToken token = default);
}

public class HttpMethods : IHttpMethods
{
	private readonly IConfig config;

	public HttpMethods(IConfig config)
	{
		this.config = config;
	}

	public async Task<Result<TokenResponseDto>> RenewToken(CancellationToken token = default)
	{
		var url = $"{config.ApiUrl}/account/api-token?days=3";

		return await GetResponseAsync(url, HttpMethod.Post, token)
			.Then(CheckStatusCode)
			.Then(DeserializeResponseContent<TokenResponseDto>)
			.RefineError("Ошибка при обновлении токена");
	}


	public async Task<Result<ShortUserInfo>> GetUserInfo(CancellationToken token = default)
	{
		var url = $"{config.ApiUrl}/account";

		return await GetResponseAsync(url, HttpMethod.Get, token)
			.Then(CheckStatusCode)
			.Then(DeserializeResponseContent<AccountResponse>)
			.Then(account => account.User)
			.RefineError("Ошибка при получении id пользователя");
	}

	public async Task<Result<CoursesListResponse>> GetCoursesList(CancellationToken token = default)
	{
		var url = $"{config.ApiUrl}/courses";
		using var client = HttpClient();

		return await GetResponseAsync(url, HttpMethod.Get, token)
			.Then(CheckStatusCode)
			.Then(DeserializeResponseContent<CoursesListResponse>)
			.RefineError("Ошибка при получении списка курсов");
	}

	public async Task<Result<TempCourseUpdateResponse>> CreateCourse(string id, CancellationToken token = default)
	{
		var url = $"{config.ApiUrl}/temp-courses/{id}";

		return await GetResponseAsync(url, HttpMethod.Post, token)
			.Then(CheckStatusCode)
			.Then(DeserializeResponseContent<TempCourseUpdateResponse>)
			.ThenCheckNoErrors()
			.RefineError("Ошибка при создании временного курса");
	}

	public async Task<Result<TempCourseUpdateResponse>> UploadCourseChanges(MemoryStream memoryStream, string id, CancellationToken token = default)
	{
		var url = $"{config.ApiUrl}/temp-courses/{id}";
		return await UpdateTempCourse(memoryStream, url, HttpMethod.Patch, token)
			.RefineError("Ошибка при отправке изменений");
	}

	public async Task<Result<TempCourseUpdateResponse>> UploadFullCourse(MemoryStream memoryStream, string id, CancellationToken token = default)
	{
		var url = $"{config.ApiUrl}/temp-courses/{id}";
		return await UpdateTempCourse(memoryStream, url, HttpMethod.Put, token)
			.RefineError("Ошибка при отправке курса");
	}

	private async Task<Result<TempCourseUpdateResponse>> UpdateTempCourse(
		MemoryStream memoryStream,
		string url,
		HttpMethod httpMethod,
		CancellationToken token
	)
	{
		memoryStream.Position = 0;
		var fileContent = new ByteArrayContent(memoryStream.ToArray());
		var multiContent = new MultipartFormDataContent { { fileContent, "files", "course.zip" } };

		return await GetResponseAsync(url, httpMethod, token, multiContent)
			.Then(CheckStatusCode)
			.Then(DeserializeResponseContent<TempCourseUpdateResponse>)
			.ThenCheckNoErrors();
	}

	private Task<Result<HttpResponseMessage>> GetResponseAsync(
		string url,
		HttpMethod method,
		CancellationToken token,
		HttpContent? content = null
	) => ResultAsync.Of(async () =>
	{
		using var client = HttpClient();
		return method switch
		{
			HttpMethod.Get => await client.GetAsync(url, token),
			HttpMethod.Post => await client.PostAsync(url, content, token),
			HttpMethod.Put => await client.PutAsync(url, content, token),
			HttpMethod.Patch => await client.PatchAsync(url, content, token),
			_ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
		};
	}, e => e.GetMessage());

	private Result<HttpResponseMessage> CheckStatusCode(HttpResponseMessage response)
	{
		return response.StatusCode switch
		{
			HttpStatusCode.OK => response,
			HttpStatusCode.Unauthorized => Result.Fail<HttpResponseMessage>(
				"Сервер вернул код 401. Повторите процесс авторизации",
				new UnauthorizedException()
			),
			HttpStatusCode.Forbidden => Result.Fail<HttpResponseMessage>(
				"Сервер вернул код 403. Нет прав на операцию",
				new ForbiddenException()
			),
			HttpStatusCode.InternalServerError => TryGetResponseMessage(response, out var message)
				? Result.Fail<HttpResponseMessage>(
					"Сервер вернул код 500. Это похоже на баг. Подробнее в логах",
					new InternalServerErrorException(message)
				)
				: Result.Fail<HttpResponseMessage>(
					"Сервер вернул код 500. Это похоже на баг"
				),
			_ => Result.Fail<HttpResponseMessage>(
				$"Сервер вернул код {response.StatusCode}",
				new StatusCodeException(response.StatusCode)
			)
		};
	}

	private bool TryGetResponseMessage(HttpResponseMessage response, [NotNullWhen(true)] out string? message)
	{
		var result = DeserializeResponseContent<ServerErrorDto>(response);
		message = result.IsSuccess
			? result.Value.Message
			: null;
		return message is not null;
	}

	private Result<T> DeserializeResponseContent<T>(HttpResponseMessage response)
	{
		return Result.Of(
				() => JsonSerializer.Deserialize<T>(response.Content.ReadAsStringAsync().Result),
				e => e.GetMessage(config.ApiUrl)
			)
			.Then(result => result is null
				? Result.Fail<T>("Не удалось прочитать ответ сервера.")
				: result.AsResult()
			);
	}

	private HttpClient HttpClient()
	{
		var client = new HttpClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", config.JwtToken);
		return client;
	}

	private enum HttpMethod
	{
		Get,
		Post,
		Put,
		Patch
	}
}