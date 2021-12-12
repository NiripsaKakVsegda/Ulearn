using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntiPlagiarism.Api;
using AntiPlagiarism.Api.Models.Parameters;
using AntiPlagiarism.ConsoleApp.Models;
using AntiPlagiarism.ConsoleApp.SubmissionPreparer;
using Ulearn.Common;
using Vostok.Logging.Abstractions;

namespace AntiPlagiarism.ConsoleApp
{
	public class AntiplagiarismConsoleApp
	{
		private IAntiPlagiarismClient antiPlagiarismClient;
		private SubmissionSearcher submissionSearcher;
		private readonly Repository repository;
		private readonly PlagiarismCsvWriter csvWriter;
		private const int MaxInQuerySubmissionsCount = 2;
		private readonly ILog log = LogProvider.Get();

		public AntiplagiarismConsoleApp(IAntiPlagiarismClient antiPlagiarismClient,
			SubmissionSearcher submissionSearcher,
			Repository repository,
			PlagiarismCsvWriter csvWriter)
		{
			this.antiPlagiarismClient = antiPlagiarismClient;
			this.submissionSearcher = submissionSearcher;
			this.repository = repository;
			this.csvWriter = csvWriter;
		}

		public List<Submission> GetNewSubmissionsAsync()
		{
			return submissionSearcher.GetSubmissions();
		}

		public async Task GetPlagiarismsAsync()
		{
			var plagiarisms = new List<PlagiarismInfo>();
			
			ConsoleWorker.WriteLine("Получение данных о плагиате в предыдущих посылках");
			foreach (var submission in repository.SubmissionsInfo.Submissions)
			{
				log.Info("Get AntiPlagiarism levels");
				var response = await antiPlagiarismClient.GetSubmissionPlagiarismsAsync(new GetSubmissionPlagiarismsParameters
				{
					SubmissionId = submission.SubmissionId
				});

				var authorName = repository.SubmissionsInfo.Authors.First(a => a.Id == submission.AuthorId).Name;
				var taskTitle = repository.SubmissionsInfo.Tasks.First(t => t.Id == submission.TaskId).Title;
				if (!response.Plagiarisms.Any())
					continue;
				plagiarisms.Add(new PlagiarismInfo
				{
					AuthorName = authorName,
					TaskTitle = taskTitle,
					Language = submission.Language,
					Weight = response.Plagiarisms.Max(p => p.Weight)
				});
			}
			csvWriter.WritePlagiarism(plagiarisms);
		}

		public async Task SendSubmissionsAsync(List<Submission> submissions)
		{
			var remainingSubmissions = new Queue<Submission>(submissions);
			var inQueueSubmissionIds = new List<int>();
			
			log.Info("Send submissions to AntiPlagiarism");
			Console.WriteLine("Отправка решений на проверку АнтиПлагиатом. Это может занять несколько минут	");
			
			while (remainingSubmissions.Any() || inQueueSubmissionIds.Any())
			{
				var processedSubmissionsCount = submissions.Count - (remainingSubmissions.Count + inQueueSubmissionIds.Count);
				
				while (remainingSubmissions.Any() && inQueueSubmissionIds.Count < MaxInQuerySubmissionsCount)
				{
					var submission = remainingSubmissions.Dequeue();
					await SendSubmissionAsync(submission);
					inQueueSubmissionIds.Add(submission.Info.SubmissionId);
				}
				
				// Узнаем, проверены ли уже отправленные
				// Если еще не проверены - засыпаем на 5 секунд
				if (inQueueSubmissionIds.Any())
				{
					var getProcessingStatusResponse = await antiPlagiarismClient
						.GetProcessingStatusAsync(
							new GetProcessingStatusParameters
							{
								SubmissionIds = inQueueSubmissionIds.ToArray()
							});
					inQueueSubmissionIds = getProcessingStatusResponse.InQueueSubmissionIds.ToList();
					
					if (inQueueSubmissionIds.Count > 0)
						await Task.Delay(5 * 1000);
				}
				ConsoleWorker.ReWriteLine($"Обработано {processedSubmissionsCount}/{submissions.Count} посылок");
			}
			Console.WriteLine();
		}

		private async Task SendSubmissionAsync(Submission submission)
		{
			var authorName = repository.SubmissionsInfo.Authors.First(a => a.Id == submission.Info.AuthorId).Name;
			var taskTitle = repository.SubmissionsInfo.Tasks.First(t => t.Id == submission.Info.TaskId).Title;
			
			var response = await antiPlagiarismClient.AddSubmissionAsync(new AddSubmissionParameters
			{
				TaskId = submission.Info.TaskId,
				AuthorId = submission.Info.AuthorId,
				Code = submission.Code,
				Language = submission.Info.Language,
				AdditionalInfo = $"Task: {taskTitle}; Author: {authorName}",
				ClientSubmissionId = "client Id (name + task)"
			});
			submission.Info.SubmissionId = response.SubmissionId;
			
			log.Info($"Send submission {submission.Info.SubmissionId}");
			repository.AddSubmissionInfo(submission.Info);
		}
	}
}