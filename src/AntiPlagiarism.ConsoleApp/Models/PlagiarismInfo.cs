using Ulearn.Common;

namespace AntiPlagiarism.ConsoleApp.Models
{
	public class PlagiarismInfo
	{
		public string AuthorName { get; set; }
		public string TaskTitle { get; set; }
		public Language Language { get; set; }
		public double FaintSuspicion { get; set; }
		public double StrongSuspicion { get; set; }
	}
}