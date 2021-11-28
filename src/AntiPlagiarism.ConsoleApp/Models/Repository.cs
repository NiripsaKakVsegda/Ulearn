using System;
using System.IO;
using Newtonsoft.Json;
using Ulearn.Common.Extensions;

namespace AntiPlagiarism.ConsoleApp.Models
{
	public class Repository
	{
		private readonly string configFile;
		private readonly string submissionsInfoFile;
		public Config Config;
		public SubmissionsInfo SubmissionsInfo;

		public Repository(string rootDirectory)
		{
			submissionsInfoFile = rootDirectory.PathCombine("submissions.json");
			configFile = rootDirectory.PathCombine("config.json");
			LoadSubmissionsInfo();
			LoadConfig();
		}

		public void SetAccessToken(string token)
		{
			Config.Token = token;
			SaveInJsonFile(Config, configFile);
		}

		public void AddSubmissionInfo(SubmissionInfo submission)
		{
			SubmissionsInfo.Submissions.Add(submission);
			SaveInJsonFile(SubmissionsInfo, submissionsInfoFile);
		}

		public void AddAuthor(Author author)
		{
			SubmissionsInfo.Authors.Add(author);
			SaveInJsonFile(SubmissionsInfo, submissionsInfoFile);
		}
		
		public void AddTask(TaskInfo taskInfo)
		{
			SubmissionsInfo.Tasks.Add(taskInfo);
			SaveInJsonFile(SubmissionsInfo, submissionsInfoFile);
		}
		
		private void LoadSubmissionsInfo()
		{
			if (!File.Exists(submissionsInfoFile))
			{
				SubmissionsInfo = new SubmissionsInfo();
				return;
			}
			SubmissionsInfo = JsonConvert.DeserializeObject<SubmissionsInfo>(
				File.ReadAllText(submissionsInfoFile));
		}
		
		private void LoadConfig()
		{
			if (!File.Exists(configFile))
			{
				Config = new Config();
				return;
			}
			Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFile));
		}

		private void SaveInJsonFile<T>(T content, string file)
		{
			File.WriteAllText(file, JsonConvert.SerializeObject(content, Formatting.Indented));
		}
	}
}