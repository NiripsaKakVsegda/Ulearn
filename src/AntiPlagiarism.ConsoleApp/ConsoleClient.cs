using System;
using AntiPlagiarism.Api;
using AntiPlagiarism.ConsoleApp.SubmissionsRepository;
using Ulearn.Common.Extensions;

namespace AntiPlagiarism.ConsoleApp
{
	public class ConsoleClient
	{
		private IAntiPlagiarismClient antiPlagiarismClient;
		private ISubmissionRepository repository;
        
		public ConsoleClient(IAntiPlagiarismClient antiPlagiarismClient, ISubmissionRepository repository)
		{
			this.antiPlagiarismClient = antiPlagiarismClient;
			this.repository = repository;
		}

		public void SendNewSubmissions()
		{
			repository.GetSubmissions().ForEach(s =>
			{
				var path = repository.GetSubmissionPath(s);
				Console.WriteLine(path);
				repository.AddSubmission(s);
			});
		}

		public void ShowAuthorPlagiarisms()
		{
			// тут будет запрос к апи
			throw new NotImplementedException();
		}
	}
}