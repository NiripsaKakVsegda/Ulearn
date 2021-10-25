using System.IO;
using AntiPlagiarism.ConsoleApp.SubmissionPreparer;
using AntiPlagiarism.ConsoleApp.trash;
using Ulearn.Common;

namespace AntiPlagiarism.ConsoleApp
{
	class Program
	{
		static void Main(string[] args)
		{
			var client = new ConsoleClient(new FakeAntiPlagiarismClient(), 
				new SubmissionSearcher(Directory.GetCurrentDirectory(), new CodeExtractor(Language.Python3)));
			client.SendNewSubmissions();
		}
	}
}