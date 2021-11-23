using System.Collections.Generic;
using System.IO;

namespace AntiPlagiarism.ConsoleApp.Models
{
	public class SubmissionsInfo
	{
		public List<Author> Authors = new ();
		public List<TaskInfo> Tasks = new ();
		public List<SubmissionInfo> Submissions = new ();
		public string Token;
	}
}