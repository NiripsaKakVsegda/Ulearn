using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntiPlagiarism.Api;
using AntiPlagiarism.Api.Models.Parameters;
using AntiPlagiarism.ConsoleApp.Models;
using Vostok.Logging.Abstractions;

namespace AntiPlagiarism.ConsoleApp
{
	public class PlagiarismWriter
	{
		private readonly ILog log = LogProvider.Get();
		private IAntiPlagiarismClient antiPlagiarismClient;
		private readonly PlagiarismCsvWriter csvWriter;
		private readonly Repository repository;

		public PlagiarismWriter(IAntiPlagiarismClient antiPlagiarismClient, PlagiarismCsvWriter csvWriter, Repository repository)
		{
			this.antiPlagiarismClient = antiPlagiarismClient;
			this.csvWriter = csvWriter;
			this.repository = repository;
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

				var mostSimilarSubmission = response.Plagiarisms.OrderByDescending(p => p.Weight).First();
				
				var authorOfMostSimilarSubmission = repository.SubmissionsInfo.Authors.Single(
					a => a.Id == mostSimilarSubmission.SubmissionInfo.AuthorId);
				plagiarisms.Add(new PlagiarismInfo
				{
					AuthorName = authorName,
					TaskTitle = taskTitle,
					PlagiarismAuthorName = authorOfMostSimilarSubmission.Name,
					Language = submission.Language,
					Weight = $"{Math.Round(response.Plagiarisms.Max(p => p.Weight) * 100, 2)}%" 
				});
			}
			csvWriter.WritePlagiarism(plagiarisms);
		}
	}
}