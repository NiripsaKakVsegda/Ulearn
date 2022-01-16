using System.Collections.Generic;
using System.IO;
using System.Linq;
using AntiPlagiarism.ConsoleApp.Models;
using Ulearn.Common.Extensions;
using Vostok.Logging.Abstractions;

namespace AntiPlagiarism.ConsoleApp.SubmissionPreparer
{
	public class SubmissionSearcher
	{
		private readonly string rootDirectory;
		private readonly CodeExtractor codeExtractor;
		private readonly Repository repository;

		private readonly ILog log = LogProvider.Get();
		
		public SubmissionSearcher(
			string rootDirectory, CodeExtractor codeExtractor, Repository repository)
		{
			this.rootDirectory = rootDirectory;
			this.codeExtractor = codeExtractor;
			this.repository = repository;
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
						&& repository.SubmissionsInfo.Submissions.All(
							s => s.TaskId != task.Id || s.AuthorId != author.Id))
					{
						var lang2Code = codeExtractor.ExtractCode(path);
						foreach (var language in lang2Code.Keys)
						{
							log.Info($"Find new submission: task {task.Title}, author {author.Name}, lang {language}");
							submissions.Add(new Submission
							{
								Info = new SubmissionInfo
								{
									AuthorId = author.Id, 
									TaskId = task.Id,
									Language = language
								},
								Code = lang2Code[language]
							});
						}
					}
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
		
		private string[] GetNewDirectories(string path, IEnumerable<string> recordedDirs) =>
			Directory.GetDirectories(path)
				.Select(Path.GetFileName)
				.Where(dir => !recordedDirs.Contains(dir))
				.Where(dir => !repository.Config.ExcludedPaths.Any(excluded => dir.Contains(excluded)))
				.ToArray();
	}
}