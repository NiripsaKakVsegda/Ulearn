using System;

namespace AntiPlagiarism.ConsoleApp.Models
{
	public class Submission
	{
		public Guid AuthorId;
		public Guid TaskId;
		public Guid SubmissionId;
		public string AttemptId;
	}
}