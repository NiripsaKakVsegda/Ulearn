using System;
using System.Threading.Tasks;
using AntiPlagiarism.Api;
using AntiPlagiarism.Api.Models.Parameters;
using AntiPlagiarism.ConsoleApp.Models;
using AntiPlagiarism.ConsoleApp.SubmissionPreparer;
using Ulearn.Common;
using Ulearn.Common.Extensions;

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

		public async Task SendNewSubmissionsAsync()
		{
			var newSubmissions = submissionSearcher.GetSubmissionsWithCode();
			foreach (var submission in newSubmissions.Keys)
			{
				// todo: отправлять не все сразу, а по несколько штук
				var response = await antiPlagiarismClient.AddSubmissionAsync(new AddSubmissionParameters
				{
					TaskId = submission.TaskId,
					AuthorId = submission.AuthorId,
					Code = newSubmissions[submission],
					Language = Language.CSharp,
					AdditionalInfo = "some important info",
					ClientSubmissionId = "client Id (name + task)"
				});
				submission.SubmissionId = response.SubmissionId;
				repository.AddSubmission(submission);
			}
		}

		public void ShowAuthorPlagiarisms()
		{
			// тут будет запрос к апи
			throw new NotImplementedException();
		}
	}
}