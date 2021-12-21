using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntiPlagiarism.Api;
using AntiPlagiarism.Api.Models.Parameters;
using AntiPlagiarism.Api.Models.Results;
using AntiPlagiarism.ConsoleApp.Models;
using AntiPlagiarism.ConsoleApp.Models.CsvPlagiarismInfo;
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

		public async Task GetPlagiarismsBrief()
		{
			var briefPlagiarismInfos = new List<BriefPlagiarismInfo>();
			
			foreach (var author in repository.SubmissionsInfo.Authors)
			{
				var authorPlagiarismCount = 0;
				var authorStrongPlagiarismCount = 0;
				foreach (var submission in repository.SubmissionsInfo.Submissions
							.Where(s => s.AuthorId == author.Id))
				{
					var response = await antiPlagiarismClient.GetSubmissionPlagiarismsAsync(new GetSubmissionPlagiarismsParameters
					{
						SubmissionId = submission.SubmissionId
					});
					
					if (!response.Plagiarisms.Any())
						continue;
					
					var maxWeight = response.Plagiarisms.Max(p => p.Weight);
					if (maxWeight >= response.SuspicionLevels.FaintSuspicion)
						authorPlagiarismCount++;
					if (maxWeight >= response.SuspicionLevels.StrongSuspicion)
						authorStrongPlagiarismCount++;
				}
				
				briefPlagiarismInfos.Add(new BriefPlagiarismInfo
				{
					AuthorName = author.Name,
					TotalSuspicionCount = authorPlagiarismCount,
					StrongSuspicionCount = authorStrongPlagiarismCount
				});
			}
			
			csvWriter.WritePlagiarism(briefPlagiarismInfos
				.OrderByDescending(b => b.TotalSuspicionCount)
				.ThenByDescending(b => b.StrongSuspicionCount)
				.ToList());
		}

		public async Task GetPlagiarismsByAuthorAsync(Author author)
		{
			var plagiarisms = new List<AuthorPlagiarismInfo>();
			
			ConsoleWorker.WriteLine($"Получение данных о плагиате {author.Name}");
			foreach (var submission in repository.SubmissionsInfo.Submissions
						.Where(s => s.AuthorId == author.Id))
			{
				log.Info("Get AntiPlagiarism levels");
				var response = await antiPlagiarismClient.GetSubmissionPlagiarismsAsync(new GetSubmissionPlagiarismsParameters
				{
					SubmissionId = submission.SubmissionId
				});

				var taskTitle = repository.SubmissionsInfo.Tasks.First(t => t.Id == submission.TaskId).Title;
				if (!response.Plagiarisms.Any())
					continue;

				var mostSimilarSubmission = response.Plagiarisms.OrderByDescending(p => p.Weight).First();
				
				var authorOfMostSimilarSubmission = repository.SubmissionsInfo.Authors.Single(
					a => a.Id == mostSimilarSubmission.SubmissionInfo.AuthorId);
				var levels = await antiPlagiarismClient.GetSuspicionLevelsAsync(new GetSuspicionLevelsParameters
				{
					TaskId = submission.TaskId,
					Language = submission.Language
				});
				
				var weight = response.Plagiarisms.Max(p => p.Weight);
				if (weight < levels.SuspicionLevels.FaintSuspicion)
					continue;
				
				plagiarisms.Add(new AuthorPlagiarismInfo
				{
					AuthorName = author.Name,
					TaskTitle = taskTitle,
					PlagiarismAuthorName = authorOfMostSimilarSubmission.Name,
					SuspicionLevel = weight < levels.SuspicionLevels.StrongSuspicion ? "слабый" : "сильный", 
					Language = submission.Language,
					Weight = $"{Math.Round(weight * 100, 2)}%" 
				});
			}
			csvWriter.WritePlagiarism(plagiarisms);
		}
	}
}