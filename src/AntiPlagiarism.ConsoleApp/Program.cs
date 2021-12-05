using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AntiPlagiarism.Api;
using AntiPlagiarism.ConsoleApp.Models;
using AntiPlagiarism.ConsoleApp.SubmissionPreparer;
using Vostok.Logging.Abstractions;
using Vostok.Logging.File;
using Vostok.Logging.File.Configuration;
using Vostok.Logging.Formatting;

namespace AntiPlagiarism.ConsoleApp
{
	class Program
	{
		private static Repository repo;
		private static AntiplagiarismConsoleApp app;

		private static readonly List<UserActions> actions = new()
		{
			new UserActions("send", Send, "отправляет новые посылки на проверку антиплагиатом"),
			new UserActions("get-levels", GetLevels, "получает информацию о плагиате и записывает в файл")
		};

		static void Main(string[] args)
		{
			InitLogger();
			InitApp();

			try
			{
				while (true)
				{
					ShowHelpMessage();
					var userInput = ConsoleWorker.GetUserInput();
					var command = actions.FirstOrDefault(c => c.Command == userInput);
					if (command == null)
					{
						ConsoleWorker.WriteLine("Неверная команда");
						continue;
					}

					command.Action();

					Console.Write("Продолжить? ");
					if (!ConsoleWorker.GetUserAnswer())
						return;
				}
			}
			catch (Exception exception)
			{
				ConsoleWorker.WriteError(exception);
			}

			Console.ReadKey();
		}

		private static void InitApp()
		{
			repo = new Repository(Directory.GetCurrentDirectory());
			
			app = new AntiplagiarismConsoleApp(
				new AntiPlagiarismClient(repo.Config.EndPointUrl, GetToken()), 
				new SubmissionSearcher(Directory.GetCurrentDirectory(), 
					new CodeExtractor(repo.Config.Languages), repo),
				repo,
				new PlagiarismCsvWriter(Directory.GetCurrentDirectory()));
		}

		private static string GetToken()
		{
			if (repo.Config.Token == null)
			{
				Console.WriteLine("Введите токен клиента антиплагиата");
				repo.SetAccessToken(ConsoleWorker.GetUserInput());
			}
			ConsoleWorker.WriteLine($"Используется токен клиента {repo.Config.Token}");
			//todo тк нам нужен id и имя преподавателя, можно сделать метод получения тут, заодно проверив право доступа
			return repo.Config.Token;
		}

		private static void Send()
		{
			var newSubmissions = app.GetNewSubmissionsAsync();
			if (newSubmissions.Count == 0)
			{
				Console.WriteLine("Новых посылок не найдено");
				return;
			}
			if (ValidateSubmissions(newSubmissions))
			{
				app.SendSubmissionsAsync(newSubmissions).GetAwaiter().GetResult();
				ConsoleWorker.WriteLine("Посылки успешно отправлены");
			}
			else
				ConsoleWorker.WriteLine("Новые посылки не были отправлены");
		}

		private static void GetLevels()
		{
			app.GetPlagiarismsAsync().GetAwaiter().GetResult();
		}

		private static void ShowHelpMessage()
		{
			foreach (var command in actions)
			{
				Console.WriteLine($"{command.Command} - {command.Help}");
			}
		}

		private static bool ValidateSubmissions(IEnumerable<Submission> submissions)
		{
			Console.WriteLine("Будут отправлены следующие посылки:");
			ConsoleWorker.PrintSubmissions(submissions.ToArray(), 
				repo.SubmissionsInfo.Authors.ToArray(),
				repo.SubmissionsInfo.Tasks.ToArray());

			Console.Write("Отправить посылки? ");
			return ConsoleWorker.GetUserAnswer();
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
	}
}