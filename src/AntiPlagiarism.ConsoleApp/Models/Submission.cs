using System;

namespace AntiPlagiarism.ConsoleApp.Models
{
	public class Submission
	{
		public Guid AuthorId;
		public Guid TaskId;
		public int SubmissionId;
		public string AttemptId;
	}
}