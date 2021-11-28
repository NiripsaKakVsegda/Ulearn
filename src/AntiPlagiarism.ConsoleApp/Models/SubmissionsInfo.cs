using System.Collections.Generic;

namespace AntiPlagiarism.ConsoleApp.Models
{
	public class SubmissionsInfo
	{
		public List<Author> Authors = new ();
		public List<TaskInfo> Tasks = new ();
		public List<SubmissionInfo> Submissions = new ();
	}
}