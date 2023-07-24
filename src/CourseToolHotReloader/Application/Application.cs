using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CourseToolHotReloader.Configs;
using CourseToolHotReloader.Infrastructure;

namespace CourseToolHotReloader.Application;

public interface IApplication
{
	Task InitializeAsync();
	Task LoginAsync();
	Task AddNewCourseAsync();
	void RemoveCourse();
	Task ReloadCourseAsync(bool reloadLastUpdated = false);
	void OpenCourseInBrowser(bool openLastUpdated = false);
	void PrintCourses();
	void ToggleErrorNotifications();
	void RegisterCourseUpdate(CourseInfo course);
	void NotifyError();
}

public class ConsoleApplication : IApplication
{
	private readonly IConfig config;
	private readonly ILoginManager loginManager;
	private readonly ICoursesManager coursesManager;

	public ConsoleApplication(IConfig config, ILoginManager loginManager, ICoursesManager coursesManager)
	{
		this.config = config;
		this.loginManager = loginManager;
		this.coursesManager = coursesManager;
	}

	public async Task InitializeAsync()
	{
		await LoginAsync(false);
#pragma warning disable CS4014
		RunAutoRenewTokenTask();
#pragma warning restore CS4014

		if (!CheckAuthorized(out _))
			return;

		foreach (var course in config.Courses)
		{
			coursesManager.StartWatchingCourse(course)
				.RefineError($"Ошибка во время инициализации курса {course.CourseId}")
				.OnFail(ConsoleWorker.WriteError);
		}

		ConsoleWorker.WriteLine(
			"Не закрывайте программу и редактируйте файлы курса у себя на компьютере. " +
			"Сделанные изменения будут автоматически загружаться на ulearn. " +
			"Перезагрузите страницу браузера, чтобы увидеть результат"
		);
		Console.WriteLine();
	}

	public async Task LoginAsync() => await LoginAsync(true);

	public async Task AddNewCourseAsync()
	{
		if (!CheckAuthorized(out var userId))
			return;

		if (!TryGetPath(out var path))
			return;

		var courseId = ConsoleWorker.GetCourseId();
		var existing = config.Courses.FirstOrDefault(course => course.CourseId.Equals(courseId, StringComparison.OrdinalIgnoreCase));
		if (existing is not null)
			ConsoleWorker.WriteError($"Курс с таким id уже добавлен в список слежения для папки: {existing.Path}");

		var excludeCriterias = CourseConfig.ReadCourseConfig(path).GetValueOrDefault(null)?.CourseToolHotReloader?.ExcludeCriterias
								?? new List<string> { "bin/", "obj/", ".vs/", ".idea/", ".git/", "_ReSharper.Caches/" };

		var course = new CourseInfo
		{
			Path = path,
			CourseId = courseId,
			ExcludeCriterias = excludeCriterias,
			CourseState = CourseState.NotSent,
			SendFullArchive = false
		};

		ConsoleWorker.WriteLine("Загружаем курс на Ulearn...");
		var result = await coursesManager.UploadCourse(course, userId)
			.RefineError("Не удалось добавить курс");
		if (!result.IsSuccess)
		{
			ConsoleWorker.WriteError(result.Error, result.Exception);
			return;
		}

		config.Courses.Add(course);
		ConsoleWorker.WriteLine($"Курс {course.CourseId} был добавлен в список отслеживаемых");
		config.Flush().OnFail(ConsoleWorker.WriteError);

		OpenCourseInBrowser(courseId, userId);
	}

	public void RemoveCourse()
	{
		if (!TryGetCourseFromUser(out var course, ", который хотите удалить"))
			return;

		coursesManager.StopWatchingCourse(course);
		config.Courses.Remove(course);
		ConsoleWorker.WriteLine($"Курс {course.CourseId} был удален из списка отслеживаемых.");
		config.Flush().OnFail(ConsoleWorker.WriteError);
	}

	public async Task ReloadCourseAsync(bool reloadLastUpdated = false)
	{
		if (!CheckAuthorized(out var userId))
			return;

		if (config.Courses.Count == 0)
		{
			ConsoleWorker.WriteLine("Нет ни одного отслеживаемого курса.");
			return;
		}

		if (reloadLastUpdated)
		{
			var lastUpdated = config.LastUpdatedCourse;
			if (lastUpdated is null)
			{
				ConsoleWorker.WriteLine("Ни один курс не был обновлен с момента запуска приложения.");
				return;
			}

			await ReloadCourseAsync(lastUpdated, userId);
		}
		else
		{
			if (!TryGetCourseFromUser(out var course, ", который хотите перезагрузить"))
				return;

			await ReloadCourseAsync(course, userId);
		}
	}

	public void OpenCourseInBrowser(bool openLastUpdated = false)
	{
		if (!CheckAuthorized(out var userId))
			return;

		if (config.Courses.Count == 0)
		{
			ConsoleWorker.WriteLine("Нет ни одного отслеживаемого курса.");
			return;
		}

		if (openLastUpdated)
		{
			var lastUpdated = config.LastUpdatedCourse;
			if (lastUpdated is null)
			{
				ConsoleWorker.WriteLine("Ни один курс не был обновлен с момента запуска приложения.");
				return;
			}

			OpenCourseInBrowser(lastUpdated.CourseId, userId);
		}
		else
		{
			if (!TryGetCourseFromUser(out var course, ", который хотите открыть в браузере"))
				return;

			OpenCourseInBrowser(course.CourseId, userId);
		}
	}

	public void PrintCourses()
	{
		var coursesIndexes = config.Courses
			.OrderBy(c => c.CourseId)
			.ToArray();
		PrintCourses(coursesIndexes);
	}

	public void ToggleErrorNotifications()
	{
		config.IsErrorNotificationsEnabled = !config.IsErrorNotificationsEnabled;
		ConsoleWorker.WriteLine($"Звуковые уведомления {(config.IsErrorNotificationsEnabled ? "включены" : "выключены")}.");
	}

	public void RegisterCourseUpdate(CourseInfo course)
	{
		config.LastUpdatedCourse = course;
	}

	public void NotifyError()
	{
		if (config.IsErrorNotificationsEnabled)
			ConsoleWorker.NotifyError();
	}

	private async Task RunAutoRenewTokenTask()
	{
		while (true)
		{
			await Task.Delay(TimeSpan.FromDays(1));
			if (config.JwtToken is null)
				continue;

			await LoginWithConfigAsync(true)
				.RefineError("Не удалось обновить токен")
				.OnFail(ConsoleWorker.WriteError);
			config.Flush().OnFail(ConsoleWorker.WriteError);
		}
		// ReSharper disable once FunctionNeverReturns
	}

	private async Task LoginAsync(bool forceRelogin)
	{
		ResetCourseStates();
		if (!forceRelogin && config.JwtToken is not null)
		{
			var authResult = await LoginWithConfigAsync();
			if (authResult.IsSuccess)
			{
				config.Flush().OnFail(ConsoleWorker.WriteError);
				return;
			}
		}

		config.JwtToken = null;
		config.Flush().OnFail(ConsoleWorker.WriteError);

		var tokenUrl = $"{config.SiteUrl}/token";
		var jwtToken = new NetworkCredential(null, ConsoleWorker.GetToken(tokenUrl)).Password;
		if (string.IsNullOrEmpty(jwtToken))
		{
			ConsoleWorker.WriteError("Некорректный токен.");
			return;
		}

		config.JwtToken = jwtToken;

		await LoginWithConfigAsync().OnFail(ConsoleWorker.WriteError);
		config.Flush().OnFail(ConsoleWorker.WriteError);
	}

	private Task<Result<None>> LoginWithConfigAsync(bool isSilent = false) =>
		loginManager.RenewTokenAsync()
			.Then(async token =>
			{
				config.JwtToken = token;
				return await loginManager.SingInAsync();
			})
			.Then(info =>
			{
				if (!isSilent)
					ConsoleWorker.WriteLine($"Вы вошли на {config.SiteUrl} под пользователем {info.Login}");
				config.UserId = info.Id;
			})
			.OnFail((_, _) =>
			{
				config.JwtToken = null;
				config.UserId = null;
			})
			.RefineError("Произошла ошибка при попытке входе");

	private void ResetCourseStates()
	{
		foreach (var course in config.Courses)
			course.CourseState = CourseState.NotSent;
	}

	private bool CheckAuthorized([NotNullWhen(true)] out string? userId)
	{
		userId = config.UserId;
		if (userId is not null)
			return true;

		ConsoleWorker.WriteError("Вы не авторизованы. Любые запросы к апи невозможны.");
		return false;
	}

	private bool TryGetPath([NotNullWhen(true)] out string? path)
	{
		path = ConsoleWorker.GetCoursePath()
			.Replace('\\', Path.DirectorySeparatorChar)
			.Replace('/', Path.DirectorySeparatorChar)
			.Trim();

		if (!Directory.Exists(path))
		{
			ConsoleWorker.WriteError("Папка не найдена. Проверьте корректность указанного пути.");
			return false;
		}

		if (!File.Exists(Path.Combine(path, "course.xml")))
		{
			ConsoleWorker.WriteError("В указанной папке нет course.xml. Проверьте корректность указанного пути.");
			return false;
		}

		if (File.Exists(Path.Combine(path, "deleted.txt")))
		{
			ConsoleWorker.WriteError("В корне курса находится файл deleted.txt. Программа не будет корректно работать, переименуйте его");
			return false;
		}

		var temp = path;
		var existing = config.Courses.FirstOrDefault(course => course.Path == temp);
		if (existing is not null)
		{
			ConsoleWorker.WriteError($"Данная папка уже добавлена в список для слежения для курса: {existing.CourseId}");
			return false;
		}

		return true;
	}

	private bool TryGetCourseFromUser([NotNullWhen(true)] out CourseInfo? course, string message = "")
	{
		course = null;
		var coursesIndexes = config.Courses
			.OrderBy(c => c.CourseId)
			.ToArray();

		PrintCourses(coursesIndexes);
		Console.WriteLine();
		if (coursesIndexes.Length == 0)
			return false;
		var courseIndex = ConsoleWorker.GetCourseIndex(message);
		if (!int.TryParse(courseIndex, out var index) || index < 1 || index > coursesIndexes.Length)
		{
			ConsoleWorker.WriteError("Некорректный номер.");
			return false;
		}

		course = coursesIndexes[index - 1];
		return true;
	}

	private async Task ReloadCourseAsync(CourseInfo course, string userId)
	{
		ConsoleWorker.WriteLine("Загружаем курс на Ulearn...");
		var result = await coursesManager.UploadCourse(course, userId)
			.RefineError($"Не удалось перезагрузить курс {course.CourseId}")
			.OnFail(ConsoleWorker.WriteError);

		if (result.IsSuccess)
			ConsoleWorker.WriteLine($"Курс {course.CourseId} был успешно перезагружен.");
	}

	private static void PrintCourses(IReadOnlyList<CourseInfo> courses)
	{
		if (courses.Count == 0)
		{
			Console.WriteLine("Нет ни одного отслеживаемого курса.");
			return;
		}

		Console.WriteLine("Отслеживаются изменения в курсах: ");
		for (var i = 0; i < courses.Count; i++)
		{
			var course = courses[i];
			var postfix = "";
			switch (course.CourseState)
			{
				case CourseState.OK:
					Console.ForegroundColor = ConsoleColor.Green;
					break;
				case CourseState.NotSent:
					Console.ForegroundColor = ConsoleColor.Gray;
					postfix = "(Без изменений. Не отправлен)";
					break;
				case CourseState.CourseValidationError:
					Console.ForegroundColor = ConsoleColor.Yellow;
					postfix = "(Ошибка валидации курса)";
					break;
				case CourseState.FatalError:
					Console.ForegroundColor = ConsoleColor.Red;
					postfix = "(Фатальная ошибка. Автообновление курса приостановлено. Исправьте ошибку и перезагрузите курс через меню, чтобы возобновить автообновление)";
					break;
			}

			Console.WriteLine($"{i + 1}) {course.CourseId}. Папка: {course.Path} {postfix}");
			Console.ResetColor();
		}
	}

	private void OpenCourseInBrowser(string courseId, string userId)
	{
		var courseUrl = Utils.BuildTempCourseUrl(config.SiteUrl, courseId, userId);
		Utils.OpenInBrowser(courseUrl)
			.RefineError("Ошибка при попытке открыть курс в браузере")
			.OnFail(ConsoleWorker.WriteError);
	}
}