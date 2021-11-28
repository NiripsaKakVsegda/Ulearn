using System;

namespace AntiPlagiarism.ConsoleApp.Models
{
	public class SubmissionInfo
	{
		public Guid AuthorId;
		public Guid TaskId;
		public int SubmissionId;
	}

	public class Submission
	{
		public SubmissionInfo Info;
		public string Code;
	}
}