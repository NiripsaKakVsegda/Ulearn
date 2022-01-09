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
		}

		public void Run()
		{
			try
			{
				SendNewSubmissions();
				GetPlagiarisms();
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