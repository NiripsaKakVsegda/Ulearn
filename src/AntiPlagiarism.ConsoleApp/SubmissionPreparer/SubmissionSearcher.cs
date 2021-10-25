using System.Collections.Generic;
using System.IO;
using System.Linq;
using AntiPlagiarism.ConsoleApp.Models;
using Ulearn.Common.Extensions;

namespace AntiPlagiarism.ConsoleApp.SubmissionPreparer
{
	public class SubmissionSearcher
	{
		private readonly string rootDirectory;
		private readonly CodeExtractor codeExtractor;
		private readonly Repository repository;

		public SubmissionSearcher(string rootDirectory, CodeExtractor codeExtractor)
		{
			this.rootDirectory = rootDirectory;
			this.codeExtractor = codeExtractor;
			repository = new Repository(this.rootDirectory);
		}

		public List<Submission> GetSubmissions()
		{
			ActualizeInfo();

			var submissions = new List<Submission>();

			foreach (var task in repository.SubmissionsInfo.Tasks)
			{
				foreach (var author in repository.SubmissionsInfo.Authors)
				{
					var path = rootDirectory.PathCombine(task.Title).PathCombine(author.Name);
					if (Directory.Exists(path) 
						&& repository.SubmissionsInfo.Submissions.All(s => s.TaskId != task.Id && s.AuthorId != author.Id))
						submissions.Add(new Submission { AuthorId = author.Id, TaskId = task.Id }); 
				}
			}

			return submissions;
		}
		
		private void ActualizeInfo()
		{
			foreach (var taskPath in GetNewDirectories(rootDirectory, 
				repository.SubmissionsInfo.Tasks.Select(t => t.Title)))
			{
				repository.AddTask(new(Path.GetFileName(taskPath)));
			}
			
			foreach (var taskPath in repository.SubmissionsInfo.Tasks
					.Select(t => Path.Combine(rootDirectory, t.Title)))
			{
				foreach (var authorPath in GetNewDirectories(taskPath,
						repository.SubmissionsInfo.Authors.Select(a => a.Name)))
				{
					repository.AddAuthor(new(Path.GetFileName(authorPath)));
				}
			}
		}
		
		private static string[] GetNewDirectories(string path, IEnumerable<string> recordedDirs) =>
			Directory.GetDirectories(path)
				.Select(Path.GetFileName)
				.Where(dir => !recordedDirs.Contains(dir))
				.ToArray();
	}
}