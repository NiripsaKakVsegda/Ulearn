using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AntiPlagiarism.Api;
using AntiPlagiarism.Api.Models.Parameters;
using AntiPlagiarism.Api.Models.Results;
using Newtonsoft.Json;
using Ulearn.Common;
using Ulearn.Core.Configuration;

namespace ManualUtils.AntiPlagiarism
{
	public record TaskName
	{
		public string Practice;
		public string Letter;
		public Language language;
	}

	public record PlagiarismsResponse
	{
		public int Student;
		public TaskName Task;
		public GetAuthorPlagiarismsResponse AuthorPlagiarisms;
	}

	public record PreparedResult
	{
		public static string Header = "Practice\tTask\tLanguage\tStudent1\tSubmission1\tWeight\tStudent2\tSubmission2";
		public string Practice;
		public string Task;
		public string Language;
		public string Student1;
		public string Submission1;
		public string Weight;
		public string Student2;
		public string Submission2;

		public override string ToString()
		{
			return $"{Practice}\t{Task}\t{Language}\t{Student1}\t{Submission1}\t{Weight}\t{Student2}\t{Submission2}";
		}
	}

	public static class FindExternalSolutionsPlagiarism
	{
		private static readonly DirectoryInfo directory = new("D://timusForPlugiarism/");
		private static readonly string[] practices = { "1", "2", "3", "4" };

		public static async Task UploadSolutions()
		{
			var parsed = await ParseFiles();
			var tasks = new Dictionary<TaskName, Guid>();
			var students = new Dictionary<int, Guid>();
			var addSubmissionParameters = parsed.Select(p =>
			{
				var (task, student, submission, filePath) = p;
				if (!tasks.ContainsKey(task))
					tasks[task] = Guid.NewGuid();
				if (!students.ContainsKey(student))
					students[student] = Guid.NewGuid();
				return new AddSubmissionParameters
				{
					TaskId = tasks[task],
					AuthorId = students[student],
					Code = File.ReadAllText(filePath),
					Language = task.language,
					ClientSubmissionId = submission
				};
			}).ToList();
			await File.WriteAllTextAsync("D://timusForPlugiarism/students.json", JsonConvert.SerializeObject(students, Formatting.Indented));
			await File.WriteAllTextAsync("D://timusForPlugiarism/tasks.json", JsonConvert.SerializeObject(tasks, Formatting.Indented));

			var antiplagiarismClientConfiguration = ApplicationConfiguration.Read<UlearnConfiguration>().AntiplagiarismClient;
			var client = new AntiPlagiarismClient(antiplagiarismClientConfiguration.Endpoint, antiplagiarismClientConfiguration.Token);
			foreach (var p in addSubmissionParameters)
			{
				await client.AddSubmissionAsync(p);
			}
		}

		public static async Task GetRawResults()
		{
			var students = JsonConvert.DeserializeObject<Dictionary<int, Guid>>(await File.ReadAllTextAsync("D://timusForPlugiarism/students.json"));
			var tasks = JsonConvert.DeserializeObject<Dictionary<string, Guid>>(await File.ReadAllTextAsync("D://timusForPlugiarism/tasks.json"))
				.ToDictionary(kvp =>
				{
					var match = Regex.Match(kvp.Key, @"TaskName { Practice = (\d+), Letter = (\w), language = (\w+) }");
					var practice = match.Groups[1].Value;
					var letter = match.Groups[2].Value;
					var language = (Language)Enum.Parse(typeof(Language), match.Groups[3].Value);
					return new TaskName { Practice = practice, Letter = letter, language = language };
				}, kvp => kvp.Value);

			var antiplagiarismClientConfiguration = ApplicationConfiguration.Read<UlearnConfiguration>().AntiplagiarismClient;
			var client = new AntiPlagiarismClient(antiplagiarismClientConfiguration.Endpoint, antiplagiarismClientConfiguration.Token);

			var parsed = await ParseFiles();
			var result = new List<PlagiarismsResponse>();
			var i = 0;
			foreach (var (task, student) in parsed.Select(s => (s.Task, s.Student)).Distinct())
			{
				var studentId = students[student];
				var taskId = tasks[task];
				var authorPlagiarisms = await client.GetAuthorPlagiarismsAsync(new GetAuthorPlagiarismsParameters { TaskId = taskId, AuthorId = studentId });
				result.Add(new PlagiarismsResponse { AuthorPlagiarisms = authorPlagiarisms, Student = student, Task = task });
				i++;
				Console.WriteLine(i);
			}

			await File.WriteAllTextAsync("D://timusForPlugiarism/plagiarismsResponses.json", JsonConvert.SerializeObject(result, Formatting.Indented));
		}

		public static async Task PrepareResults()
		{
			var students = JsonConvert.DeserializeObject<Dictionary<int, Guid>>(await File.ReadAllTextAsync("D://timusForPlugiarism/students.json"))
				.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
			var responses = JsonConvert.DeserializeObject<List<PlagiarismsResponse>>(await File.ReadAllTextAsync("D://timusForPlugiarism/plagiarismsResponses.json"));
			var rows = responses.SelectMany(s =>
			{
				return s.AuthorPlagiarisms.ResearchedSubmissions.SelectMany(r => r.Plagiarisms.Select(p =>
					new PreparedResult
					{
						Practice = s.Task.Practice,
						Task = s.Task.Letter,
						Language = s.Task.language.ToString(),
						Student1 = s.Student.ToString(),
						Submission1 = r.SubmissionInfo.ClientSubmissionId,
						Weight = Math.Round(p.Weight, 3).ToString(),
						Student2 = students[p.SubmissionInfo.AuthorId].ToString(),
						Submission2 = p.SubmissionInfo.ClientSubmissionId
					}));
			}).ToList();
			await using (var sw = File.CreateText("D://timusForPlugiarism/preparedResult.txt"))
			{
				await sw.WriteLineAsync(PreparedResult.Header);
				foreach (var row in rows)
					await sw.WriteLineAsync(row.ToString());
			}
		}

		private static async Task<List<(TaskName Task, int Student, string Submission, string FileName)>> ParseFiles()
		{
			if (!directory.Exists)
				throw new Exception();
			var fileNames = practices.SelectMany(p => directory.GetDirectories(p.ToString())[0].GetFiles().Select(f => f.FullName)).ToList();
			return fileNames.Select(filePath =>
			{
				var match = Regex.Match(filePath, @"[\\/](\d+)[\\/](\d+)_(\w)_(\d+)\.(\w+)$");
				var practice = match.Groups[1].Value;
				var student = int.Parse(match.Groups[2].Value);
				var letter = match.Groups[3].Value;
				var submission = match.Groups[4].Value;
				var extension = match.Groups[5].Value;
				var language = LanguageHelpers.GuessByExtension("." + extension);
				var task = new TaskName { Practice = practice, Letter = letter, language = language };
				return (task, student, submission, filePath);
			}).ToList();
		}
	}
}