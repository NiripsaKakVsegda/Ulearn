using System.Collections.Generic;
using AntiPlagiarism.ConsoleApp.Models;

namespace AntiPlagiarism.ConsoleApp.SubmissionsRepository
{
	public interface ISubmissionRepository
	{
		List<Submission> GetSubmissions();

		string GetSubmissionPath(Submission submission);
		void AddSubmission(Submission submission);
	}
}