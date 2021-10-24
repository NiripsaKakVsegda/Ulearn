using System.Collections.Generic;
using System.IO;

namespace AntiPlagiarism.ConsoleApp.Models
{
	public class SubmissionsInfo
	{
		public List<Author> Authors = new ();
		public List<Task> Tasks = new ();
		public List<Submission> Submissions = new ();
	}
}