using System;
using System.Collections.Generic;
using System.Linq;
using AntiPlagiarism.ConsoleApp.Models;
using AntiPlagiarism.ConsoleApp.SubmissionPreparer;
using Vostok.Logging.Abstractions;

namespace AntiPlagiarism.ConsoleApp
{
	public class AntiplagiarismConsoleApp
	{
		private readonly SubmissionSearcher submissionSearcher;
		private readonly SubmissionSender submissionSender;
		private readonly PlagiarismWriter plagiarismWriter;
		private readonly Repository repository;
		private readonly ILog log = LogProvider.Get();
		
		private readonly List<UserActions> actions;
		
		public AntiplagiarismConsoleApp(SubmissionSearcher submissionSearcher,
			SubmissionSender submissionSender,
			PlagiarismWriter plagiarismWriter,
			Repository repository)
		{
			this.submissionSearcher = submissionSearcher;
			this.repository = repository;
			this.plagiarismWriter = plagiarismWriter;
			this.submissionSender = submissionSender;

			actions = new()
			{
				new UserActions("send", SendNewSubmissions, "отправляет новые посылки на проверку антиплагиатом"),
				new UserActions("get-plagiarism", GetPlagiarisms, "получает информацию о плагиате и записывает в файл")
			};
		}

		public void Run()
		{
			try
			{
				while (true)
				{
					var userInput = ConsoleWorker.GetUserChoice(actions.Select(
						a => new ConsoleOption{ Option = a.Command, Description = a.Help}).ToList());
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

		private List<Submission> GetNewSubmissions()
		{
			return submissionSearcher.GetSubmissions();
		}

		private void SendNewSubmissions()
		{
			var newSubmissions = GetNewSubmissions();
			if (newSubmissions.Count == 0)
			{
				Console.WriteLine("Новых посылок не найдено");
				return;
			}
			if (ValidateSubmissions(newSubmissions))
			{
				submissionSender.SendSubmissionsAsync(newSubmissions).GetAwaiter().GetResult();
				ConsoleWorker.WriteLine("Посылки успешно отправлены");
			}
			else
				ConsoleWorker.WriteLine("Новые посылки не были отправлены");
		}

		private void GetPlagiarisms()
		{
			plagiarismWriter.GetPlagiarismsAsync().GetAwaiter().GetResult();
		}

		private void ShowHelpMessage()
		{
			foreach (var command in actions)
			{
				Console.WriteLine($"{command.Command} - {command.Help}");
			}
		}

		private bool ValidateSubmissions(IEnumerable<Submission> submissions)
		{
			Console.WriteLine("Будут отправлены следующие посылки:");
			ConsoleWorker.PrintSubmissions(submissions.ToArray(), 
				repository.SubmissionsInfo.Authors.ToArray(),
				repository.SubmissionsInfo.Tasks.ToArray());

			Console.Write("Отправить посылки? ");
			return ConsoleWorker.GetUserAnswer();
		}
	}
}