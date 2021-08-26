using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using AntiPlagiarism.Web.Database;
using Database;

using Newtonsoft.Json;

namespace ManualUtils.AntiPlagiarism
{
	[DataContract]
	public class BlackOrWhiteLabels
	{
		[DataMember(Name = "submissionId")]
		public int SubmissionId;
		[DataMember(Name = "userId")]
		public string UserId;
		[DataMember(Name = "courseId")]
		public string CourseId;
		[DataMember(Name = "taskId")]
		public string SlideId;
		[DataMember(Name = "isBlack")]
		public bool IsBlack;
		[DataMember(Name = "antiplagSubmissionId")]
		public int AntiplagSubmissionId;
		[DataMember(Name = "weight")]
		public double? Weight;
		[DataMember(Name = "instructor")]
		public string Instructor;
	}

	public static class PlagiarismInstructorDecisions
	{
		public static IEnumerable<string> GetBlackAndWhiteLabels(UlearnDb db, AntiPlagiarismDb adb, IEnumerable<BestPairWeight> bestPairWeights)
		{
			var weightsDict = bestPairWeights.ToDictionary(e => e.Submission, e => e.Weight);
			var blackLabels = GetBlackLabels(db).ToList();
			Console.WriteLine($"Black labels count: {blackLabels.Count}");
			var whiteLabels = GetWhiteLabels(db).ToList();
			var concat = blackLabels.Concat(whiteLabels).ToList();
			Console.WriteLine($"Labels count: {concat.Count}");
			Thread.Sleep(5000);
			var i = 0;
			foreach (var e in concat)
			{
				var info1 = $"{{\"SubmissionId\": {e.SubmissionId}}}";
				var info2 = $"{{\r\n  \"SubmissionId\": {e.SubmissionId}\r\n}}";
				var asi = adb.Submissions
					.Where(s => s.AdditionalInfo == info1 || s.AdditionalInfo == info2)
					.Select(s => new {s.Id})
					.FirstOrDefault();
				i++;
				if(i % 1000 == 0)
					Console.WriteLine($"i: {i}");
				if (asi == null)
				{
					Console.WriteLine("asi is null");
					continue;
				}
				e.AntiplagSubmissionId = asi.Id;
				if (weightsDict.ContainsKey(e.AntiplagSubmissionId))
					e.Weight = weightsDict[e.AntiplagSubmissionId];
			}
			return concat
				.Where(e => e.Weight != null)
				.Select(JsonConvert.SerializeObject);
		}

		private static IEnumerable<BlackOrWhiteLabels> GetBlackLabels(UlearnDb db)
		{
			var dateTime = DateTime.Parse("2019-01-17 08:42:47.343");
			var blackLabels = db.ExerciseCodeReviews
				.Where(r => r.ExerciseChecking.CourseId == "basicprogramming" || r.ExerciseChecking.CourseId == "basicprogramming2")
				.Where(r => r.Submission.Timestamp > dateTime)
				.Where(r => r.Comment.StartsWith("Ой! Наш робот") && !r.IsDeleted)
				.Select(r => new
				{
					SubmissionId = r.ExerciseChecking.Id,
					r.ExerciseChecking.UserId,
					r.ExerciseChecking.CourseId,
					r.ExerciseChecking.SlideId,
					r.AuthorId
				})
				.AsEnumerable()
				.Select(b => new BlackOrWhiteLabels
				{
					SubmissionId = b.SubmissionId,
					UserId = b.UserId,
					CourseId = b.CourseId,
					SlideId = b.SlideId.ToString(),
					IsBlack = true,
					Instructor = b.AuthorId
				});
			return blackLabels;
		}

		private static IEnumerable<BlackOrWhiteLabels> GetWhiteLabels(UlearnDb db)
		{
			var dateTime = DateTime.Parse("2019-01-17 08:42:47.343");
			var whiteLabels = db.ManualExerciseCheckings
				.Where(r => r.CourseId == "basicprogramming" || r.CourseId == "basicprogramming2")
				.Where(r => r.Score > 0 || r.Percent > 0)
				.Where(r => r.Submission.Timestamp > dateTime)
				.Select(r => new
				{
					SubmissionId = r.Id,
					r.UserId,
					r.CourseId,
					r.SlideId,
					r.Timestamp,
					r.LockedById
				})
				.AsEnumerable()
				.GroupBy(r => (r.UserId, r.CourseId, r.SlideId))
				.Select(g => g.OrderBy(r => r.Timestamp).Last())
				.Select(b => new BlackOrWhiteLabels
				{
					SubmissionId = b.SubmissionId,
					UserId = b.UserId,
					CourseId = b.CourseId,
					SlideId = b.SlideId.ToString(),
					Instructor = b.LockedById
				});
			return whiteLabels;
		}
	}
}