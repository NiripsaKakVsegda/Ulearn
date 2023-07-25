#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Groups;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Core.Courses.Slides.Quizzes.Blocks;
using Ulearn.Core.Extensions;
using Ulearn.Web.Api.Models.Common;

namespace Ulearn.Web.Api.Controllers
{
	[Route("/export")]
	public class ExportController : BaseController
	{
		private readonly IVisitsRepo visitsRepo;
		private readonly IGroupsRepo groupsRepo;
		private readonly IGroupAccessesRepo groupAccessesRepo;
		private readonly ICourseRolesRepo courseRolesRepo;
		private readonly IUnitsRepo unitsRepo;

		public ExportController(ICourseStorage courseStorage, UlearnDb db, IUsersRepo usersRepo, IVisitsRepo visitsRepo, IGroupsRepo groupsRepo, IGroupAccessesRepo groupAccessesRepo, ICourseRolesRepo courseRolesRepo, IUnitsRepo unitsRepo)
			: base(courseStorage, db, usersRepo)
		{
			this.visitsRepo = visitsRepo;
			this.groupsRepo = groupsRepo;
			this.groupAccessesRepo = groupAccessesRepo;
			this.courseRolesRepo = courseRolesRepo;
			this.unitsRepo = unitsRepo;
		}

		[HttpGet("users-info-and-results")]
		[Authorize]
		public async Task<ActionResult> ExportGroupMembersAsTsv(
			[Required] int groupId,
			bool vk = false,
			bool telegram = false,
			bool email = false,
			bool ip = false,
			bool scoring = false,
			bool gender = false,
			Guid? quizSlideId = null
		)
		{
			var group = await groupsRepo.FindGroupByIdAsync(groupId);
			if (group == null)
				return NotFound(new ErrorResponse($"Group with id {groupId} not found"));

			var isCourseAdmin = await courseRolesRepo.HasUserAccessToCourse(UserId, group.CourseId, CourseRoleType.CourseAdmin);
			var hasAccess = isCourseAdmin || await groupAccessesRepo.HasUserGrantedAccessToGroupOrIsOwnerAsync(groupId, UserId);

			if (!hasAccess)
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse($"You has no access to group with id {groupId}"));

			var users = await GetExtendedUserInfo(groupId, vk, ip);

			List<string>? questions = null;
			var courseId = group.CourseId;
			var course = courseStorage.GetCourse(courseId);
			var visibleUnits = await unitsRepo.GetPublishedUnitIds(course);
			if (quizSlideId != null)
			{
				var slide = course.FindSlideById(quizSlideId.Value, false, visibleUnits);
				if (slide == null)
					return StatusCode((int)HttpStatusCode.NotFound, $"Slide not found in course {courseId}");

				if (slide is not QuizSlide quizSlide)
					return StatusCode((int)HttpStatusCode.NotFound, $"Slide is not quiz slide in course {courseId}");

				questions = GetQuizQuestions(quizSlide);
				var answers = await GetQuizAnswers(users.Select(s => s.Id).ToList(), courseId, quizSlide);
				foreach (var user in users)
					user.Answers = answers[user.Id];
			}

			var slides = course.GetSlides(false, visibleUnits)
				.Where(s => s.ShouldBeSolved)
				.Select(s => s.Id)
				.ToList();
			var scores = GetScoresByScoringGroups(
				users.Select(u => u.Id).ToList(),
				slides,
				course
			);
			var scoringGroupsWithScores = scores.Keys
				.Select(key => key.ScoringGroup)
				.ToHashSet();
			var scoringGroups = course.Settings.Scoring.Groups.Values
				.Where(sg => scoringGroupsWithScores.Contains(sg.Id))
				.ToList();

			var headers = new List<string?> { "Id", "Login", "FirstName", "LastName" };

			if (gender)
				headers.Add("Gender");
			if (email)
				headers.Add("Email");
			if (vk)
				headers.Add("VkUrl");
			if (telegram)
				headers.Add("Telegram");
			if (ip)
			{
				headers.Add("IpAddress");
				headers.Add("LastVisit");
			}

			if (questions != null)
				headers.AddRange(questions);
			if (scoring && scoringGroups.Count > 0)
				headers.AddRange(scoringGroups.Select(s => s.Abbreviation));

			var table = new List<List<string?>> { headers };
			foreach (var user in users)
			{
				var row = new List<string?> { user.Id, user.Login, user.FirstName, user.LastName };

				if (gender)
					row.Add(user.Gender?.ToString());
				if (email)
					row.Add(user.Email);
				if (vk)
					row.Add(user.VkUrl);
				if (telegram)
					row.Add(user.Telegram);
				if (ip)
				{
					row.Add(user.IpAddress);
					row.Add(user.LastVisit.ToSortableDate());
				}

				if (user.Answers != null)
					row = row.Concat(user.Answers).ToList();
				if (scoring)
					row.AddRange(scoringGroups
						.Select(scoringGroup => (scores.ContainsKey((user.Id, scoringGroup.Id))
								? scores[(user.Id, scoringGroup.Id)]
								: 0
							).ToString()));
				table.Add(row);
			}

			var content = CreateTsv(table);
			Response.Headers.Add(
				"content-disposition",
				quizSlideId is null
					? $@"attachment;filename=""{groupId}.tsv"""
					: $@"attachment;filename=""{quizSlideId} - {groupId}.tsv"""
			);

			return Content(content, "application/octet-stream");
		}

		private Task<List<ExtendedUserInfo>> GetExtendedUserInfo(int groupId, bool vk, bool ip)
		{
			var query = db.GroupMembers
				.Where(m => m.GroupId == groupId)
				.Select(m => m.User)
				.Select(user => new
				{
					User = user,
					LastVisit = new DateTime(),
					IpAddress = (string?)null,
					VkUrl = (string?)null
				});

			if (vk)
				query = query
					.GroupJoin(
						db.UserLogins,
						userInfo => userInfo.User.Id,
						login => login.UserId,
						(userInfo, logins) => new
						{
							userInfo.User,
							userInfo.LastVisit,
							userInfo.IpAddress,
							logins
						}
					)
					.SelectMany(userInfo => userInfo.logins
							.Where(login => login.LoginProvider == LoginProviders.Vk)
							.DefaultIfEmpty(),
						(userInfo, login) => new
						{
							userInfo.User,
							userInfo.LastVisit,
							userInfo.IpAddress,
							VkUrl = login == null
								? null
								: $"https://vk.com/id{login.ProviderKey}"
						}
					);

			if (ip)
				query = query
					.Select(userInfo => new
						{
							userInfo.User,
							LastVisit = db.Visits
								.Where(visit => visit.UserId == userInfo.User.Id)
								.OrderByDescending(v => v.Timestamp)
								.Select(v => v.Timestamp)
								.FirstOrDefault(),
							IpAddress = db.Visits
								.Where(visit => visit.UserId == userInfo.User.Id)
								.OrderByDescending(v => v.Timestamp)
								.Select(v => v.IpAddress)
								.FirstOrDefault(),
							userInfo.VkUrl
						}
					);

			return query
				.Select(userInfo => new ExtendedUserInfo
				{
					Id = userInfo.User.Id,
					Login = userInfo.User.UserName,
					Email = userInfo.User.Email,
					Telegram = userInfo.User.TelegramChatTitle,
					FirstName = userInfo.User.FirstName,
					LastName = userInfo.User.LastName,
					VisibleName = userInfo.User.VisibleName,
					AvatarUrl = userInfo.User.AvatarUrl,
					Gender = userInfo.User.Gender,
					LastVisit = userInfo.LastVisit,
					IpAddress = userInfo.IpAddress,
					VkUrl = userInfo.VkUrl
				})
				.ToListAsync();
		}

		private class ExtendedUserInfo : ShortUserInfo
		{
			public DateTime LastVisit;
			public string? IpAddress;
			public string? VkUrl;
			public string Telegram = null!;
			public string[]? Answers;
		}

		private List<string> GetQuizQuestions(QuizSlide slide)
		{
			return slide.Blocks
				.OfType<AbstractQuestionBlock>()
				.Select(q => q.Text.TruncateWithEllipsis(50))
				.ToList();
		}

		private async Task<Dictionary<string, string[]>> GetQuizAnswers(
			List<string> userIds,
			string courseId,
			QuizSlide slide
		)
		{
			var query = db.UserQuizSubmissions
				.Where(s => s.CourseId == courseId && s.SlideId == slide.Id && userIds.Contains(s.UserId))
				.GroupJoin(db.UserQuizAnswers,
					s => s.Id,
					a => a.SubmissionId,
					(s, answers) => new
					{
						s.UserId,
						s.Timestamp,
						Answers = answers
					}
				)
				.SelectMany(
					sa => sa.Answers,
					(sa, a) => new
					{
						sa.UserId,
						sa.Timestamp,
						a.BlockId,
						Answer = a.ItemId ?? a.Text
					}
				)
				.GroupBy(a => new { a.UserId, a.BlockId })
				.Select(g => g.OrderByDescending(a => a.Timestamp).First());

			var usersAnswers = (await query.ToListAsync())
				.GroupBy(e => e.UserId)
				.Select(g => new
				{
					UserId = g.Key,
					Answers = g.AsEnumerable()
				});

			var questionIndexesByIds = slide.Blocks
				.OfType<AbstractQuestionBlock>()
				.Select((q, i) => (q.Id, i))
				.ToDictionary(pair => pair.Id, pair => pair.i);

			var result = userIds
				.ToDictionary(userId => userId, _ => new string[questionIndexesByIds.Count]);

			foreach (var userAnswers in usersAnswers)
			{
				var sortedAnswers = result[userAnswers.UserId];
				foreach (var answer in userAnswers.Answers)
					sortedAnswers[questionIndexesByIds[answer.BlockId]] = answer.Answer;
			}

			return result;
		}

		private Dictionary<(string UserId, string ScoringGroup), int> GetScoresByScoringGroups(List<string> userIds, List<Guid> slides, Course course)
		{
			var filterOptions = new VisitsFilterOptions
			{
				CourseId = course.Id,
				UserIds = userIds,
				SlidesIds = slides,
				PeriodStart = DateTime.MinValue,
				PeriodFinish = DateTime.MaxValue
			};

			return visitsRepo.GetVisitsInPeriod(filterOptions)
				.Select(v => new { v.UserId, v.SlideId, v.Score })
				.AsEnumerable()
				.GroupBy(v => (v.UserId, course.GetSlideByIdNotSafe(v.SlideId).ScoringGroup))
				.ToDictionary(
					g => g.Key,
					g => g.Sum(v => v.Score)
				);
		}

		private static string CreateTsv(IEnumerable<IEnumerable<string?>> table) =>
			table
				.Select(row => row
					.Select(cell => cell is null
						? ""
						: Regex.Replace(cell, "[\t\r\n]", " ").Trim()
					)
					.JoinToString('\t')
				)
				.JoinToString(Environment.NewLine);
	}
}