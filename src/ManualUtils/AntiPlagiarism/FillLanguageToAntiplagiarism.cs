using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AntiPlagiarism.Web.Database;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common;

namespace ManualUtils.AntiPlagiarism
{

	public class FillLanguageToAntiplagiarism
	{
		private static Dictionary<Guid, Language?> taskIdToSubmission = new();

		private static Language? GetLanguageByTaskId(Guid taskId, AntiPlagiarismDb adb)
		{
			if (!taskIdToSubmission.ContainsKey(taskId))
			{
				var submission = adb.Submissions
					.OrderByDescending(s => s.AddingTime)
					.FirstOrDefault(s => s.TaskId == taskId);
				taskIdToSubmission[taskId] = submission?.Language;
			}
			return taskIdToSubmission[taskId];
		}

		private static void FillLanguageTasksStatisticsParameters(AntiPlagiarismDb adb)
		{
			Console.WriteLine("FillLanguageTasksStatisticsParameters");

			var parameterses = adb
				.TasksStatisticsParameters
				.Where(p => p.Language == 0)
				.ToList();

			var count = parameterses.Count;

			Console.WriteLine($"Count {count}");

			foreach (var parameters in parameterses)
			{
				var newLanguage = GetLanguageByTaskId(parameters.TaskId, adb);
				if (newLanguage == null)
					continue;
				adb.TasksStatisticsParameters.Remove(parameters);
				adb.SaveChanges();
				parameters.Language = newLanguage.Value;
				adb.TasksStatisticsParameters.Add(parameters);
				adb.SaveChanges();
			}
		}

		private static void FillLanguageSnippetsStatistics(AntiPlagiarismDb adb, bool @do)
		{
			Console.WriteLine("FillLanguageSnippetsStatistics");

			var taskIds = adb
				.SnippetsStatistics
				.Select(s => s.TaskId)
				.Distinct()
				.ToList();

			Console.WriteLine($"Count taskIds {taskIds.Count}");

			var taskIdToLanguage = new Dictionary<Guid, Language?>();
			foreach (var taskId in taskIds)
			{
				if (!taskIdToLanguage.ContainsKey(taskId))
					taskIdToLanguage[taskId] = GetLanguageByTaskId(taskId, adb);
			}

			var snippets = adb
				.SnippetsStatistics
				.Select(c => new {c.Id, c.TaskId, c.Language})
				.AsNoTracking();

			var count = snippets.Count();

			Console.WriteLine($"Count snippets {count}");

			var changesCount = 0;
			var changes = new List<(int Id, Language Language)>();
			using (var changesFile = new StreamWriter("changes.txt"))
			{
				var getChangesCompleted = 0;
				foreach (var snippet in snippets)
				{
					getChangesCompleted++;
					if (getChangesCompleted % 10000 == 0)
						Console.WriteLine($"getChangesCompleted {getChangesCompleted} / {count}");
					var newLanguage = taskIdToLanguage[snippet.TaskId];
					if (newLanguage == null)
						newLanguage = 0;
					if (newLanguage == snippet.Language)
						continue;
					changesFile.WriteLine($"{snippet.Id} {newLanguage.Value}");
					changes.Add((snippet.Id, newLanguage.Value));
					changesCount++;
				}
			}

			Console.WriteLine($"Found changes {changesCount}");

			var batchSize = 300;
			var currentBatch = 0;

			var batches = new List<List<(int Id, Language Language)>>();
			while (currentBatch * batchSize < changes.Count)
			{
				var nextBatch = changes.Skip(currentBatch * batchSize)
					.Take(batchSize).ToList();
				batches.Add(nextBatch);
				currentBatch++;
			}

			if (@do)
			{
				adb.DisableAutoDetectChanges();
				var completed = 0;
				foreach (var batch in batches)
				{
					var batchIds = batch.Select(s => s.Id).ToList();
					var batchDict = batch.ToDictionary(s => s.Id, s => s.Language);
					var batchData = adb.SnippetsStatistics.Where(s => batchIds.Contains(s.Id)).ToList();
					foreach (var data in batchData)
					{
						data.Language = batchDict[data.Id];
						adb.SnippetsStatistics.Update(data);
					}
					completed += batch.Count;
					Console.WriteLine($"FillLanguageSnippetsStatistics - Completed {completed} / {changesCount}");
					adb.SaveChanges();
				}
				adb.EnableAutoDetectChanges();
			}
		}

		private static void FillLanguageManualSuspicionLevels(AntiPlagiarismDb adb)
		{
			Console.WriteLine("FillLanguageManualSuspicionLevels");

			var suspicionLevels = adb
				.ManualSuspicionLevels
				.Where(p => p.Language == 0)
				.ToList();

			var count = suspicionLevels.Count;

			Console.WriteLine($"Count {count}");

			foreach (var level in suspicionLevels)
			{
				var newLanguage = GetLanguageByTaskId(level.TaskId, adb);
				if (newLanguage == null)
					continue;
				adb.ManualSuspicionLevels.Remove(level);
				adb.SaveChanges();
				level.Language = newLanguage.Value;
				adb.ManualSuspicionLevels.Add(level);
				adb.SaveChanges();
			}
		}

		public static void FillLanguage(AntiPlagiarismDb adb)
		{
			FillLanguageManualSuspicionLevels(adb);
			FillLanguageTasksStatisticsParameters(adb);
			FillLanguageSnippetsStatistics(adb, true);
		}
	}
}