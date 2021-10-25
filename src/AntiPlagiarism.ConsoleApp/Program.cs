using System;
using System.IO;
using AntiPlagiarism.ConsoleApp.Models;
using AntiPlagiarism.ConsoleApp.SubmissionPreparer;
using AntiPlagiarism.ConsoleApp.trash;
using Ulearn.Common;

namespace AntiPlagiarism.ConsoleApp
{
	class Program
	{
		static void Main(string[] args)
		{
			var repo = new Repository(Directory.GetCurrentDirectory());
			var client = new ConsoleClient(new FakeAntiPlagiarismClient(), 
				new SubmissionSearcher(Directory.GetCurrentDirectory(), new CodeExtractor(Language.Python3), repo),
				repo);
			
			// Todo: нормальный консольный клиент
			var isWorking = true;
			while (isWorking)
			{
				var command = Console.ReadLine();
				switch (command)
				{
					case "send":
						client.SendNewSubmissionsAsync().GetAwaiter().GetResult();
						break;
					case "end":
						isWorking = false;
						break;
					default:
						Console.WriteLine("wrong input");
						break;
				}
			}
		}
	}
}