using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AntiPlagiarism.Api;
using AntiPlagiarism.ConsoleApp.Models;
using AntiPlagiarism.ConsoleApp.SubmissionPreparer;
using Ulearn.Common;

namespace AntiPlagiarism.ConsoleApp
{
	class Program
	{
		// todo вынести в DI (если потребуется)
		private const string EndPointUrl = "http://localhost:33333/documentation/index.html";
		
		private static bool isWorkingFlag = true;
		private static Repository repo;
		private static ConsoleClient client;

		private static List<CommandInfo> commands = new()
		{
			new CommandInfo("help", Help, "показывает это окно"),
			new CommandInfo("exit", () => isWorkingFlag = false, "завершает выполнение программы"),
			new CommandInfo("send", Send, "отправляет новые посылки на проверку антиплагиатом")
		};

		static void Main(string[] args)
		{
			InitClient();
			
			while (isWorkingFlag)
			{
				var userInput = ConsoleWorker.GetUserInput();
				var command = commands.FirstOrDefault(c => c.Command == userInput);
				if (command == null)
				{
					ConsoleWorker.WriteLine("Неверная команда");
					continue;
				}
				command.Action();
			}
		}

		private static void InitClient()
		{
			repo = new Repository(Directory.GetCurrentDirectory());
			
			client = new ConsoleClient(
				new AntiPlagiarismClient(EndPointUrl, GetToken()), 
				new SubmissionSearcher(Directory.GetCurrentDirectory(), 
					new CodeExtractor(Language.CSharp), repo),
				repo);
		}

		private static string GetToken()
		{
			if (repo.SubmissionsInfo.Token == null)
			{
				Console.WriteLine("Введите токен преподавателя");
				// todo
				Console.WriteLine("Этот токен можно получить тут: ...");
				repo.SetAccessToken(ConsoleWorker.GetUserInput());
			}
			return repo.SubmissionsInfo.Token;
		}

		private static void Send()
		{
			var newSubmissions = client.GetNewSubmissionsAsync();
			if (newSubmissions.Count == 0)
			{
				Console.WriteLine("Новых посылок не найдено");
			}
			if (ValidateSubmissions(newSubmissions))
			{
				client.SendSubmissionsAsync(newSubmissions).GetAwaiter().GetResult();
				ConsoleWorker.WriteLine("Посылки успешно отправлены");
			}
			else
				ConsoleWorker.WriteLine("Новые посылки не были отправлены");
		}

		private static void Help()
		{
			foreach (var command in commands)
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

			var answer = "";
			Console.WriteLine("Отправить посылки (yes/no):");

			while (answer != "no" && answer != "yes")
				answer = Console.ReadLine();
			return answer == "yes";
		} 
	}
}