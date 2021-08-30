using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Autofac;
using CourseToolHotReloader.ApiClient;
using CourseToolHotReloader.DirectoryWorkers;
using CourseToolHotReloader.Exceptions;
using CourseToolHotReloader.LoginAgent;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;
using Ulearn.Common.Extensions;
using Vostok.Logging.File;
using Vostok.Logging.File.Configuration;
using Vostok.Logging.Formatting;
using ErrorType = CourseToolHotReloader.Dtos.ErrorType;

namespace CourseToolHotReloader
{
	internal static class Program
	{
		private static IContainer container;
		private static IConfig config;
		private static IUlearnApiClient ulearnApiClient;
		private static ILog log => LogProvider.Get();

		private static void Main()
		{
			try
			{
				Init();
				Startup().Wait();
			}
			catch (AggregateException e)
			{
				log.Error(e, "Root error");
				switch (e.InnerExceptions.Single())
				{
					case UriFormatException _:
						ConsoleWorker.WriteError("Указанный в config.json baseUrl имеет некорректный формат");
						break;
					case HttpRequestException ee when ee.InnerException is SocketException:
						ConsoleWorker.WriteError($"Не удалось подключиться к {config.ApiUrl}");
						break;
					case HttpRequestException ee when ee.InnerException is IOException && ee.Message.Contains("SSL"):
						ConsoleWorker.WriteError($"Не удалось использовать https при подключении к {config.ApiUrl}");
						break;
					case IOException _:
						ConsoleWorker.WriteError("Ошибка ввода-вывода. Подробнее в логах");
						break;
					case InternalServerErrorException _:
						ConsoleWorker.WriteError("Сервер вернул код 500. Это похоже на баг. Подробнее в логах");
						break;
					case ForbiddenException _:
						ConsoleWorker.WriteError("Сервер вернул код 403. Нет прав на операцию. Перезапустите программу и войдите повторно");
						config.JwtToken = null;
						config.Flush();
						break;
					case UnauthorizedException _:
						ConsoleWorker.WriteError("Сервер вернул код 401. Срок авторизации истек. Перезапустите программу и войдите повторно");
						config.JwtToken = null;
						config.Flush();
						break;
					case StatusCodeException ee:
						ConsoleWorker.WriteError($"Сервер вернул код {ee.StatusCode}");
						break;
					case HttpRequestException _:
						ConsoleWorker.WriteError(e.Message);
						break;
					case CourseLoadingException _:
						ConsoleWorker.WriteError(e.Message);
						break;
					default:
						ConsoleWorker.WriteError("Ошибка. Подробнее в логах");
						break;
				}
			}
			catch (Exception e)
			{
				log.Error(e, "Root error");
				ConsoleWorker.WriteError("Ошибка. Подробнее в логах");
			}
			finally
			{
				BeforeProgramEnd();
			}
		}

		private static void Init()
		{
			InitLogger();
			container = ConfigureAutofac.Build();
			config = container.Resolve<IConfig>();
			ulearnApiClient = container.Resolve<IUlearnApiClient>();
		}

		private static void BeforeProgramEnd()
		{
			FileLog.FlushAll();
		}

		private static void InitLogger()
		{
			var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "{RollingSuffix}.log");
			var fileLogSettings = new FileLogSettings
			{
				FilePath = logPath,
				RollingStrategy = new RollingStrategyOptions
				{
					MaxFiles = 0,
					Type = RollingStrategyType.Hybrid,
					Period = RollingPeriod.Day,
					MaxSize = 4 * 1073741824L,
				},
				OutputTemplate = OutputTemplate.Parse("{Timestamp:HH:mm:ss.fff} {Level:u5} {sourceContext:w}{Message}{NewLine}{Exception}")
			};
			var fileLog = new FileLog(fileLogSettings).WithMinimumLevel(LogLevel.Info);
			LogProvider.Configure(fileLog);
		}

		private static async Task Startup()
		{
			if (!File.Exists("course.xml"))
			{
				ConsoleWorker.WriteLine("В текущей папке нет course.xml. Запустите программу в папке курса с course.xml");
				return;
			}

			if (File.Exists(Path.Combine(config.Path, "deleted.txt")))
			{
				ConsoleWorker.WriteError("В корне курса находится файл deleted.txt Программа не будет корректно работать, переименуйте его");
				return;
			}

			var userId = await Login();
			if (userId == null)
				return;

			SetupCourseId();
			var tempCourseForUserExist = await TempCourseExist(userId);
			if (!tempCourseForUserExist)
				if (!await CreateCourse())
					return;
			config.Flush();

			config.ExcludeCriterias = ReadCourseConfig()?.CourseToolHotReloader?.ExcludeCriterias ?? new List<string> { "bin/", "obj/", ".vs/", ".idea/", ".git/", "_ReSharper.Caches/" };

			await SendFullCourse();
			var tempCourseId = GetTmpCourseId(config.CourseId, userId);
			OpenInBrowser(tempCourseId);

			StartWatch();
		}

		private static void OpenInBrowser(string tempCourseId)
		{
			var courseUrl = $"{config.SiteUrl}/Course/{tempCourseId}";
			Process.Start(new ProcessStartInfo(courseUrl) { UseShellExecute = true }); // https://stackoverflow.com/a/61035650/6800354
		}

		private static void StartWatch()
		{
			var courseWatcher = container.Resolve<ICourseWatcher>();
			courseWatcher.StartWatch();
		}

		private static async Task<string> Login()
		{
			var shortUserInfo = await container.Resolve<ILoginAgent>().SignIn();

			if (shortUserInfo != null)
				ConsoleWorker.WriteLine($"Вы вошли на {config.SiteUrl} под пользователем {shortUserInfo.Login}");
			else
				ConsoleWorker.WriteError("Неправильный логин или пароль");

			return shortUserInfo?.Id;
		}

		private static async Task SendFullCourse()
		{
			var tempCourseUpdateResponse = await ulearnApiClient.SendFullCourse(config.Path, config.CourseId, config.ExcludeCriterias);
			if (tempCourseUpdateResponse.ErrorType == ErrorType.NoErrors)
			{
				ConsoleWorker.WriteLine("Первоначальная полная загрузка курса прошла успешно");
				ConsoleWorker.WriteLine("Не закрывайте программу и редактируйте файлы курса у себя на компьютере. Сделанные изменения будут автоматически загружаться на ulearn. Перезагрузите страницу браузера, чтоб увидеть результат");
			}
			else
			{
				throw new CourseLoadingException(tempCourseUpdateResponse.Message);
			}
		}

		private static void SetupCourseId()
		{
			var courseId = config.CourseIds.GetOrDefault(config.Path);
			if (!string.IsNullOrEmpty(courseId))
				return;

			courseId = ConsoleWorker.GetCourseId();
			config.CourseIds[config.Path] = courseId;
		}

		private static async Task<bool> TempCourseExist(string userId)
		{
			var tempCourseId = GetTmpCourseId(config.CourseId, userId);
			var hasTempCourse = await ulearnApiClient.HasCourse(tempCourseId);

			if (hasTempCourse)
				ConsoleWorker.WriteLine($"Обнаружен существующий временный курс с id {tempCourseId}");

			return hasTempCourse;
		}

		private static string GetTmpCourseId(string baseCourseId, string userId)
		{
			return $"{baseCourseId}_{userId}";
		}

		private static async Task<bool> CreateCourse()
		{
			var createResponse = await ulearnApiClient.CreateCourse(config.CourseId);
			if (createResponse.ErrorType != ErrorType.NoErrors)
				ConsoleWorker.WriteError(createResponse.Message);
			else
				ConsoleWorker.WriteLine(createResponse.Message);

			return createResponse.ErrorType == ErrorType.NoErrors;
		}

		[CanBeNull]
		private static CourseConfig ReadCourseConfig()
		{
			var name = "CourseConfig.json";
			if (!File.Exists(name))
				return null;
			var json = File.ReadAllText("CourseConfig.json");
			return JsonSerializer.Deserialize<CourseConfig>(json);
		}
	}
}