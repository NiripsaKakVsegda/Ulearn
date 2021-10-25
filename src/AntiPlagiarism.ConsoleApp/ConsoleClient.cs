using System;
using AntiPlagiarism.Api;
using AntiPlagiarism.ConsoleApp.SubmissionPreparer;
using Ulearn.Common.Extensions;

namespace AntiPlagiarism.ConsoleApp
{
	public class ConsoleClient
	{
		private IAntiPlagiarismClient antiPlagiarismClient;
		private SubmissionSearcher submissionSearcher;
        
		public ConsoleClient(IAntiPlagiarismClient antiPlagiarismClient, SubmissionSearcher submissionSearcher)
		{
			this.antiPlagiarismClient = antiPlagiarismClient;
			this.submissionSearcher = submissionSearcher;
		}

		public void SendNewSubmissions()
		{
			submissionSearcher.GetSubmissions().ForEach(s =>
			{
			});
		}

		public void ShowAuthorPlagiarisms()
		{
			// тут будет запрос к апи
			throw new NotImplementedException();
		}
	}
}