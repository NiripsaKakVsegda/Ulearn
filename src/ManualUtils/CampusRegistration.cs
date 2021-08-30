using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ManualUtils
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	internal static class CampusRegistration
	{
		public static void Run(UlearnDb db, string courseId, int groupId, Guid slideWithRegistrationQuiz, bool exportCode)
		{
			var regData = new Dictionary<string, RegistrationData>();

			var users = db.GroupMembers
				.Where(gm => gm.GroupId == groupId)
				.ToList()
				.ToDictSafe(gm => gm.UserId, gm => gm.User);
			
			SetProfileData(users, regData);
			SetQuizAnswers(slideWithRegistrationQuiz, db, users, regData);
			SetIpAddresses(courseId, db, users, regData);
			
			var slides = new[]
			{
				"6e33f73a-67a3-47e2-9ec4-32fd3ec9f94f",
				"7d886a4b-c5da-4c84-b1d6-4f276b8a092e",
				"5185bf31-e336-4af4-9bd0-85b7e9593e9f",
				"d89b4498-dae6-481c-8ec5-7a8dd0e4eb1d",
				"4f2c38b5-891e-48a8-a202-a3f0239b5d45",
				"82f41a06-fcab-4aa2-ac59-a437eb4415fe"
			}.Select(s => new Guid(s)).ToArray();
			SetTasksCount(slides, regData, db);
			if(exportCode)
				SetCode(db, slides, regData);

			var regs = regData.Values.OrderBy(r => r.LastName).ThenBy(r => r.FirstName).ThenBy(r => r.Login).ToList();
			File.WriteAllText("students.tsv", "UserId\tFirstName\tLastName\tLogin\tEmail\tPhoneNumber\tCity\tUniversity\tSpecialty\tCourseNumber\tTasksCount\tIpAddress\r\n");
			File.WriteAllLines("students.tsv", regs.Select(v => $"{v.UserId}\t{v.FirstName}\t{v.LastName}\t{v.Login}\t{v.Email}\t{v.PhoneNumber}\t{v.City}\t{v.University}\t{v.Specialty}\t{v.CourseNumber}\t{v.TasksCount}\t{v.IpAddress}"));

			var json = JsonConvert.SerializeObject(regs);
			File.WriteAllText("students.json",json);
		}

		private static void SetProfileData(Dictionary<string, ApplicationUser> users, Dictionary<string, RegistrationData> regData)
		{
			foreach (var user in users.Values)
			{
				regData[user.Id] = new RegistrationData
				{
					UserId = user.Id,
					FirstName = user.FirstName,
					LastName = user.LastName,
					Login = user.UserName,
					Email = user.Email,
				};
			}
		}

		private static void SetQuizAnswers(Guid slideWithRegistrationQuiz, UlearnDb db, Dictionary<string, ApplicationUser> users, Dictionary<string, RegistrationData> regData)
		{
			var submissions = db.UserQuizSubmissions.Where(qs => qs.SlideId == slideWithRegistrationQuiz)
				.ToList()
				.GroupBy(gm => gm.UserId)
				.Where(g => users.ContainsKey(g.Key))
				.Select(g => g.OrderByDescending(s => s.Timestamp).First())
				.ToList();

			foreach (var submission in submissions)
			{
				var answers = db.UserQuizAnswers.Where(a => a.SubmissionId == submission.Id).ToList();
				var userId = submission.User.Id;
				var reg = regData[userId];
				reg.CourseNumber = answers.FirstOrDefault(a => a.BlockId == "CourseNumber")?.Text;
				reg.PhoneNumber = answers.FirstOrDefault(a => a.BlockId == "PhoneNumber")?.Text;
				reg.City = answers.FirstOrDefault(a => a.BlockId == "City")?.Text;
				reg.Specialty = answers.FirstOrDefault(a => a.BlockId == "Specialty")?.Text;
				reg.University = answers.FirstOrDefault(a => a.BlockId == "University")?.Text;
			}
		}

		private static void SetTasksCount(Guid[] slides, Dictionary<string, RegistrationData> regData, UlearnDb db)
		{
			foreach (var data in regData.Values)
			{
				foreach (var slide in slides)
				{
					var visit = db.Visits
						.Where(es => es.UserId == data.UserId && es.SlideId == slide && es.IsPassed && es.Score > 0)
						.ToList()
						.OrderByDescending(es => es.Timestamp)
						.FirstOrDefault();
					if (visit != null)
						data.TasksCount++;
				}
			}
		}

		private static void SetIpAddresses(string courseId, UlearnDb db, Dictionary<string, ApplicationUser> users, Dictionary<string, RegistrationData> regData)
		{
			var time = DateTime.Now.AddMonths(-4);
			var visits = db.Visits
				.Where(v =>
					v.Timestamp > time
					&& v.IpAddress != null
					&& v.CourseId == courseId
				)
				.GroupBy(v => v.UserId)
				.Select(kvp => kvp.OrderByDescending(v => v.Timestamp).FirstOrDefault())
				.ToList()
				.Where(v => users.ContainsKey(v.UserId))
				.ToList();
			foreach (var visit in visits)
			{
				var userId = visit.User.Id;
				var reg = regData[userId];
				reg.IpAddress = visit.IpAddress;
			}
		}

		private static void SetCode(UlearnDb db, Guid[] slides, Dictionary<string, RegistrationData> regData)
		{
			foreach (var data in regData.Values)
			{
				for (var i = 0; i < slides.Length; i++)
				{
					var slide = slides[i];
					var submission = db.UserExerciseSubmissions
						.Where(es => es.UserId == data.UserId && es.SlideId == slide)
						.ToList()
						.OrderByDescending(es => es.Timestamp)
						.FirstOrDefault();
					if(submission == null)
						continue;
					data.Code[i] = db.Texts.FirstOrDefault(t => t.Hash == submission.SolutionCodeHash)?.Text;
				}
			}
		}
	}

	public class RegistrationData
	{
		public string UserId;
		public string FirstName;
		public string LastName;
		public string Login;
		public string Email;
		public string CourseNumber;
		public string PhoneNumber;
		public string City;
		public string Specialty;
		public string University;
		public int TasksCount;
		public string IpAddress;
		public readonly string[] Code = {null, null, null, null, null, null};
	}
}