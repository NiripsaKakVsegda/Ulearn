using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AntiPlagiarism.ConsoleApp.Models;
using AntiPlagiarism.ConsoleApp.SubmissionPreparer;
using AntiPlagiarism.ConsoleApp.trash;
using Ulearn.Common;

namespace AntiPlagiarism.ConsoleApp
{
	class Program
	{
		private static Repository repo;
		
		static void Main(string[] args)
		{
			repo = new Repository(Directory.GetCurrentDirectory());
			var client = new ConsoleClient(new FakeAntiPlagiarismClient(), 
				new SubmissionSearcher(Directory.GetCurrentDirectory(), new CodeExtractor(Language.Python3), repo),
				repo);
			
			// Todo: нормальный консольный интерфейс
			var isWorking = true;
			while (isWorking)
			{
				Console.Write("Введите команду: ");
				var command = Console.ReadLine();
				switch (command)
				{
					case "send":
						var newSubmissions = client.GetNewSubmissionsAsync();
						if (newSubmissions.Count == 0)
						{
							Console.WriteLine("Новых посылок не найдено");
						}
						if (ValidateSubmissions(newSubmissions.Keys))
						{
							client.SendSubmissionsAsync(newSubmissions).GetAwaiter().GetResult();
							Console.WriteLine("Посылки успешно отправлены");
						}
						else
							Console.WriteLine("Новые посылки не были отправлены");
						break;
					case "end":
						isWorking = false;
						break;
					default:
						Console.WriteLine("Неверная команда");
						break;
				}

				Console.WriteLine();
			}
		}

		private static bool ValidateSubmissions(IEnumerable<Submission> submissions)
		{
			Console.WriteLine("Будут отправлены следующие посылки:");
			foreach (var author2Submissions in submissions
				.GroupBy(s => s.AuthorId))
			{
				Console.WriteLine(repo.SubmissionsInfo.Authors
					.First(a => a.Id == author2Submissions.Key).Name);
				foreach (var submission in author2Submissions)
				{
					var task = repo.SubmissionsInfo.Tasks.First(t => t.Id == submission.TaskId);
					Console.WriteLine($"->  {task.Title}");
				}

				Console.WriteLine();
			}

			var answer = "";
			Console.WriteLine("Отправить посылки (yes/no):");

			while (answer != "no" && answer != "yes")
				answer = Console.ReadLine();
			return answer == "yes";
		} 
	}
}