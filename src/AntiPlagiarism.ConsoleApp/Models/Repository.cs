using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Ulearn.Common.Extensions;

namespace AntiPlagiarism.ConsoleApp.Models
{
	public class Repository
	{
		private readonly string submissionsInfoFile;
		public SubmissionsInfo SubmissionsInfo;

		public Repository(string rootDirectory)
		{
			submissionsInfoFile = rootDirectory.PathCombine("submissions.json");
			LoadSubmissionsInfo();
		}

		public void AddSubmission(Submission submission)
		{
			SubmissionsInfo.Submissions.Add(submission);
			SaveSubmissionsInfo();
		}

		public void AddAuthor(Author author)
		{
			SubmissionsInfo.Authors.Add(author);
			SaveSubmissionsInfo();
		}
		
		public void AddTask(Task task)
		{
			SubmissionsInfo.Tasks.Add(task);
			SaveSubmissionsInfo();
		}
		
		private void LoadSubmissionsInfo()
		{
			if (!File.Exists(submissionsInfoFile))
			{
				SubmissionsInfo = new SubmissionsInfo();
			}
			else
			{
				SubmissionsInfo = JsonConvert.DeserializeObject<SubmissionsInfo>(
					File.ReadAllText(submissionsInfoFile));                
			}
		}

		private void SaveSubmissionsInfo()
		{
			File.WriteAllText(
				submissionsInfoFile, 
				JsonConvert.SerializeObject(SubmissionsInfo, Formatting.Indented));
		}
	}
}