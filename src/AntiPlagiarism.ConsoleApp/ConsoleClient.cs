using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AntiPlagiarism.Api;
using AntiPlagiarism.Api.Models.Parameters;
using AntiPlagiarism.ConsoleApp.Models;
using AntiPlagiarism.ConsoleApp.SubmissionPreparer;
using Ulearn.Common;

namespace AntiPlagiarism.ConsoleApp
{
	public class ConsoleClient
	{
		private IAntiPlagiarismClient antiPlagiarismClient;
		private SubmissionSearcher submissionSearcher;
		private readonly Repository repository;

		public ConsoleClient(IAntiPlagiarismClient antiPlagiarismClient,
			SubmissionSearcher submissionSearcher,
			Repository repository)
		{
			this.antiPlagiarismClient = antiPlagiarismClient;
			this.submissionSearcher = submissionSearcher;
			this.repository = repository;
		}

		public List<Submission> GetNewSubmissionsAsync()
		{
			return submissionSearcher.GetSubmissions();
		}

		public async Task SendSubmissionsAsync(List<Submission> submissions)
		{
			var inQueueCount = 0;

			foreach (var submission in submissions)
			{
				
				// todo: отправлять не все сразу, а по несколько штук
				await SendSubmissionAsync(submission);
			}
		}

		public void ShowAuthorPlagiarisms(Guid authorId)
		{
			// тут будет запрос к апи
			throw new NotImplementedException();
		}

		private async Task SendSubmissionAsync(Submission submission)
		{
			var response = await antiPlagiarismClient.AddSubmissionAsync(new AddSubmissionParameters
			{
				TaskId = submission.Info.TaskId,
				AuthorId = submission.Info.AuthorId,
				Code = submission.Code,
				Language = Language.CSharp,
				AdditionalInfo = "some important info",
				ClientSubmissionId = "client Id (name + task)"
			});
			submission.Info.SubmissionId = response.SubmissionId;
			
			repository.AddSubmissionInfo(submission.Info);
		}
	}
}