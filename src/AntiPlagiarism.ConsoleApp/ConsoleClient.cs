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

		public Dictionary<Submission, string> GetNewSubmissionsAsync()
		{
			return submissionSearcher.GetSubmissionsWithCode();
		}

		public async Task SendSubmissionsAsync(Dictionary<Submission, string> submissions)
		{
			var inQueueCount = 0;

			foreach (var submission in submissions.Keys)
			{
				
				// todo: отправлять не все сразу, а по несколько штук
				await SendSubmissionAsync(submission, submissions[submission]);
			}
		}

		public void ShowAuthorPlagiarisms(Guid authorId)
		{
			// тут будет запрос к апи
			throw new NotImplementedException();
		}

		private async Task SendSubmissionAsync(Submission submission, string code)
		{
			var response = await antiPlagiarismClient.AddSubmissionAsync(new AddSubmissionParameters
			{
				TaskId = submission.TaskId,
				AuthorId = submission.AuthorId,
				Code = code,
				Language = Language.CSharp,
				AdditionalInfo = "some important info",
				ClientSubmissionId = "client Id (name + task)"
			});
			submission.SubmissionId = response.SubmissionId;
			repository.AddSubmission(submission);
		}
	}
}