using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Database;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Ulearn.Core.Courses.Manager;

namespace ManualUtils.AntiPlagiarism
{
	[DataContract]
	public class TaskWeights
	{
		[DataMember(Name = "taskId")]
		public string TaskId;

		[DataMember(Name = "name")]
		public string Name;

		[DataMember(Name = "courseId")]
		public string CourseId;

		[DataMember(Name = "weights")]
		public double[] Weights;
	}

	[DataContract]
	public class BestPairWeight
	{
		[DataMember(Name = "s")]
		public int Submission;

		[DataMember(Name = "o")]
		public int Other;

		[DataMember(Name = "w")]
		public double Weight;
	}

	public static class AntiplagiarismLogsParser
	{
		public static IEnumerable<BestPairWeight> GetWeightsOfSubmissionPairs(IEnumerable<string> lines)
		{
			var submission2WithMaxWeight = new Dictionary<int, (int, double)>();
			var weightsRegex = new Regex(@"Link weight between submisions (\d+) and (\d+) is ([0-9.]+)\.", RegexOptions.Compiled);
			foreach (var line in lines)
			{
				var weightsMatch = weightsRegex.Match(line);
				if (weightsMatch.Success)
				{
					var submission = int.Parse(weightsMatch.Groups[1].Value);
					var otherSubmission = int.Parse(weightsMatch.Groups[2].Value);
					var weight = double.Parse(weightsMatch.Groups[3].Value, CultureInfo.InvariantCulture);
					submission2WithMaxWeight[submission] = (otherSubmission, weight);
					if (!submission2WithMaxWeight.ContainsKey(submission))
						submission2WithMaxWeight[submission] = (otherSubmission, weight);
					else
					{
						submission2WithMaxWeight[submission] = submission2WithMaxWeight[submission].Item2 < weight
							? (otherSubmission, weight)
							: submission2WithMaxWeight[submission];
					}
				}
			}

			return submission2WithMaxWeight.Select(kvp => new BestPairWeight { Submission = kvp.Key, Other = kvp.Value.Item1, Weight = kvp.Value.Item2 });
		}

		public static IEnumerable<string> GetWeightsForStatistics(IServiceProvider serviceProvider, IEnumerable<string> lines)
		{
			var db = serviceProvider.GetService<UlearnDb>();
			var courseStorage = serviceProvider.GetService<ICourseStorage>();
			var weights = ParseWeights(lines)
				.GroupBy(w => w.TaskId)
				.Select(g => g.Last())
				.ToList();
			var slideId2CourseId = db.ManualExerciseCheckings
				.Where(v => v.CourseId == "basicprogramming" || v.CourseId == "basicprogramming2")
				.Select(v => new { v.SlideId, v.CourseId })
				.Distinct()
				.AsEnumerable()
				.ToDictionary(p => p.SlideId.ToString(), p => p.CourseId);
			weights = weights
				.Where(w => slideId2CourseId.ContainsKey(w.TaskId))
				.Select(w =>
				{
					w.CourseId = slideId2CourseId[w.TaskId];
					return w;
				})
				.ToList();
			var bp = courseStorage.GetCourse("basicprogramming");
			var bp2 = courseStorage.GetCourse("basicprogramming2");
			weights = weights.Select(w =>
			{
				var slide = bp.FindSlideByIdNotSafe(new Guid(w.TaskId)) ?? bp2.FindSlideByIdNotSafe(new Guid(w.TaskId));
				w.Name = slide?.Title ?? "";
				return w;
			}).ToList();
			return weights.Select(JsonConvert.SerializeObject);
		}

		private static IEnumerable<TaskWeights> ParseWeights(IEnumerable<string> lines)
		{
			var taskIdRegex = new Regex(@"Новые статистические параметры задачи \(TaskStatisticsParameters\) по задаче ([^:]+):", RegexOptions.Compiled);
			var weightsRegex = new Regex(@"по следующему набору весов: (\[[^\]]+\])", RegexOptions.Compiled);
			double[] currentWeights = null;
			foreach (var line in lines)
			{
				var weightsMatch = weightsRegex.Match(line);
				if (weightsMatch.Success)
				{
					currentWeights = JsonConvert.DeserializeObject<double[]>(weightsMatch.Groups[1].Value);
					continue;
				}

				var taskIdMatch = taskIdRegex.Match(line);
				if (taskIdMatch.Success)
				{
					var currentTaskId = taskIdMatch.Groups[1].Value;
					yield return new TaskWeights { TaskId = currentTaskId, Weights = currentWeights };
				}
			}
		}
	}
}