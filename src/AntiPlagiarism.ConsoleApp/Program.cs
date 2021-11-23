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
		private static bool isWorkingFlag;
		private static Repository repo;
		
		static void Main(string[] args)
		{
			repo = new Repository(Directory.GetCurrentDirectory());
			//todo 
			var client = new ConsoleClient(new AntiPlagiarismClient("placeHolder", "placeHolder"), 
				new SubmissionSearcher(Directory.GetCurrentDirectory(), new CodeExtractor(Language.Python3), repo),
				repo);
			
			// Todo: нормальный консольный интерфейс
			var isWorking = true;
			while (isWorking)
			{
				var command = ConsoleWorker.GetNextCommand();
				switch (command)
				{
					case "send":
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
						break;
					case "end":
						isWorking = false;
						break;
					default:
						ConsoleWorker.WriteLine("Неверная команда");
						break;
				}

				Console.WriteLine();
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