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
		private static Repository repo;
		private static ConsoleClient client;

		private static List<UserActions> commands = new()
		{
			new UserActions("send", Send, "отправляет новые посылки на проверку антиплагиатом")
		};

		static void Main(string[] args)
		{
			InitClient();
			
			while (true)
			{
				var userInput = ConsoleWorker.GetUserInput();
				var command = commands.FirstOrDefault(c => c.Command == userInput);
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

		private static void InitClient()
		{
			repo = new Repository(Directory.GetCurrentDirectory());
			
			client = new ConsoleClient(
				new AntiPlagiarismClient(repo.Config.EndPointUrl, GetToken()), 
				new SubmissionSearcher(Directory.GetCurrentDirectory(), 
					new CodeExtractor(Language.CSharp), repo),
				repo);
		}

		private static string GetToken()
		{
			if (repo.Config.Token == null)
			{
				Console.WriteLine("Введите токен преподавателя");
				Console.WriteLine("Этот токен можно получить у администраторов Ulearn.me");
				repo.SetAccessToken(ConsoleWorker.GetUserInput());
			}
			return repo.Config.Token;
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

			Console.Write("Отправить посылки? ");
			return ConsoleWorker.GetUserAnswer();
		} 
	}
}