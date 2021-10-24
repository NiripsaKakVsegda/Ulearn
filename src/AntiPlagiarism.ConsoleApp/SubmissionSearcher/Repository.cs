using System.Collections.Generic;
using System.IO;
using System.Linq;
using AntiPlagiarism.ConsoleApp.Models;
using Newtonsoft.Json;
using Ulearn.Common.Extensions;

namespace AntiPlagiarism.ConsoleApp.SubmissionsRepository
{
	public class Repository : ISubmissionRepository
	{
		private readonly string rootDirectory;
		private string submissionsInfoFile;
		private SubmissionsInfo submissionsInfo;

		public Repository(string rootDirectory = null)
		{
			this.rootDirectory = rootDirectory ?? Directory.GetCurrentDirectory();
			submissionsInfoFile = rootDirectory.PathCombine("submissions.json");
			LoadSubmissionsInfo();
		}

		public List<Submission> GetSubmissions()
		{
			ActualizeInfo();

			var submissions = new List<Submission>();

			foreach (var task in submissionsInfo.Tasks)
			{
				foreach (var author in submissionsInfo.Authors)
				{
					var path = rootDirectory.PathCombine(task.Title).PathCombine(author.Name);
					if (Directory.Exists(path) 
						&& submissionsInfo.Submissions.All(s => s.TaskId != task.Id && s.AuthorId != author.Id))
						submissions.Add(new Submission{ AuthorId = author.Id, TaskId = task.Id }); 
				}
			}

			return submissions;
		}

		public string GetSubmissionPath(Submission submission)
		{
			// логировать при падении
			var task = submissionsInfo.Tasks.First(t => t.Id == submission.TaskId);
			var author = submissionsInfo.Authors.First(a => a.Id == submission.AuthorId);
			return rootDirectory.PathCombine(task.Title).PathCombine(author.Name);
		}

		public void AddSubmission(Submission submission)
		{
			submissionsInfo.Submissions.Add(submission);
			SaveSubmissionsInfo();
		}
		
		private void LoadSubmissionsInfo()
		{
			if (!File.Exists(submissionsInfoFile))
			{
				submissionsInfo = new SubmissionsInfo();
			}
			else
			{
				submissionsInfo = JsonConvert.DeserializeObject<SubmissionsInfo>(
					File.ReadAllText(submissionsInfoFile));                
			}
		}

		private void ActualizeInfo()
		{
			GetNewDirectories(rootDirectory, 
				submissionsInfo.Tasks.Select(t => t.Title))
				.ForEach(taskPath => submissionsInfo.Tasks.Add(
					new(Path.GetDirectoryName(taskPath))));
			
			submissionsInfo.Tasks
				.Select(t => Path.Combine(rootDirectory, t.Title))
				.ForEach(path => GetNewDirectories(path,
					submissionsInfo.Authors.Select(a => a.Name)));
			
			SaveSubmissionsInfo();
		}

		private void SaveSubmissionsInfo()
		{
			File.WriteAllText(
					submissionsInfoFile, 
				JsonConvert.SerializeObject(submissionsInfo));
		}
		
		private static string[] GetNewDirectories(string path, IEnumerable<string> recordedDirs) =>
			Directory.GetDirectories(path)
				.Select(Path.GetFileName)
				.Where(dir => !recordedDirs.Contains(dir))
				.ToArray();
	}
}