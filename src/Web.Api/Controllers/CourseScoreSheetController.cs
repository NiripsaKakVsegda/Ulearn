using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Groups;
using Database.Repos.Users;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Web.Api.Models.Internal;
using Ulearn.Web.Api.Models.Parameters.Analytics;
using Ulearn.Web.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.GoogleSheet;
using Ulearn.Web.Api.Models.Responses.Analytics;
using Web.Api.Configuration;

namespace Ulearn.Web.Api.Controllers
{
	public class CourseScoreSheetController : BaseController
	{
		private readonly ICourseRolesRepo courseRolesRepo;
		private readonly IGroupMembersRepo groupMembersRepo;
		private readonly IUnitsRepo unitsRepo;
		private readonly IGroupsRepo groupsRepo;
		private readonly ControllerUtils controllerUtils;
		private readonly IVisitsRepo visitsRepo;
		private readonly IAdditionalScoresRepo additionalScoresRepo;
		private readonly IGroupAccessesRepo groupAccessesRepo;
		private readonly UlearnConfiguration configuration;
		

		public CourseScoreSheetController(ICourseStorage courseStorage, UlearnDb db, IUsersRepo usersRepo, ICourseRolesRepo courseRolesRepo,
			IGroupMembersRepo groupMembersRepo, IUnitsRepo unitsRepo,
			IGroupsRepo groupsRepo, IGroupAccessesRepo groupAccessesRepo,
			ControllerUtils controllerUtils, IVisitsRepo visitsRepo, IAdditionalScoresRepo additionalScoresRepo,
			IOptions<WebApiConfiguration> options)
			: base(courseStorage, db, usersRepo)
		{
			this.courseRolesRepo = courseRolesRepo;
			this.groupMembersRepo = groupMembersRepo;
			this.unitsRepo = unitsRepo;
			this.groupsRepo = groupsRepo;
			this.groupAccessesRepo = groupAccessesRepo;
			this.controllerUtils = controllerUtils;
			this.visitsRepo = visitsRepo;
			this.additionalScoresRepo = additionalScoresRepo;
			configuration = options.Value;
		}
		
		[HttpGet("course-score-sheet/export/{fileNameWithNoExtension}.json")]
		[Authorize(Policy = "Instructors", AuthenticationSchemes = "Bearer,Identity.Application")]
		public async Task<ActionResult> ExportCourseStatisticsAsJson([FromQuery] CourseStatisticsParams param)
		{
			if (param.CourseId == null)
				return NotFound();

			var model = await GetCourseStatisticsModel(param, 3000);
			var serializedModel = new CourseStatisticsModel(model).JsonSerialize(Formatting.Indented);

			return File(Encoding.UTF8.GetBytes(serializedModel), "application/json", $"{param.FileNameWithNoExtension}.json");
		}
		
		[HttpGet("course-score-sheet/export/{fileNameWithNoExtension}.xml")]
		[Authorize(Policy = "Instructors", AuthenticationSchemes = "Bearer,Identity.Application")]
		public async Task<ActionResult> ExportCourseStatisticsAsXml([FromQuery] CourseStatisticsParams param)
		{
			if (param.CourseId == null)
				return NotFound();
			var model = await GetCourseStatisticsModel(param, 3000); 
			var serializedModel = new CourseStatisticsModel(model).XmlSerialize();
			return File(Encoding.UTF8.GetBytes(serializedModel), "text/xml", $"{param.FileNameWithNoExtension}.xml");
		}

		[HttpGet("course-score-sheet/export/{fileNameWithNoExtension}.xlsx")]
		[Authorize(Policy = "Instructors", AuthenticationSchemes = "Bearer,Identity.Application")]
		public async Task<ActionResult> ExportCourseStatisticsAsXlsx([FromQuery] CourseStatisticsParams param)
		{
			if (param.CourseId == null)
				return NotFound();

			var model = await GetCourseStatisticsModel(param, 3000);
			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
			var package = new ExcelPackage();
			var builder = new ExcelWorksheetBuilder(package.Workbook.Worksheets.Add(model.CourseTitle));
			FillCourseStatisticsWithBuilder(
				builder,
				model,
				exportEmails: true
			);
			builder = new ExcelWorksheetBuilder(package.Workbook.Worksheets.Add("Только полные баллы"));
			FillCourseStatisticsWithBuilder(
				builder,
				model,
				exportEmails: true,
				onlyFullScores: true
			);
			byte[] bytes;
			using (var stream = StaticRecyclableMemoryStreamManager.Manager.GetStream()) {
				await package.SaveAsAsync(stream);
				bytes = stream.ToArray();
			}
			
			return File(bytes, "application/vnd.ms-excel", $"{param.FileNameWithNoExtension}.xlsx");
		}

		[HttpGet("course-score-sheet/export/to-google-sheets")]
		[Authorize(Policy = "Instructors")]
		public async Task<ActionResult> ExportCourseStatisticsToGoogleSheets([FromQuery] CourseStatisticsParams courseStatisticsParams)
		{
			if (courseStatisticsParams.CourseId == null)
				return NotFound();
			var model = await GetCourseStatisticsModel(courseStatisticsParams, 3000);
			var listId = courseStatisticsParams.ListId;
			var sheet = new GoogleSheet(200, 200, listId);
			var builder = new GoogleSheetBuilder(sheet);
			FillCourseStatisticsWithBuilder(builder, model);
			
			var credentialsJson = configuration.GoogleAccessCredentials;
			var client = new GoogleApiClient(credentialsJson);
			client.FillSpreadSheet(courseStatisticsParams.SpreadsheetId, sheet);
			return Ok();
		}

		private void FillCourseStatisticsWithBuilder(ISheetBuilder builder, CourseStatisticPageModel model, bool exportEmails = false, bool onlyFullScores = false)
		{
			builder.AddStyleRule(s => s.Font.Bold = true);

			builder.AddCell("", 3);
			builder.AddCell("За весь курс", model.ScoringGroups.Count);
			builder.AddStyleRule(s => s.Border.Left.Style = ExcelBorderStyle.Thin);
			foreach (var unit in model.Units)
			{
				var colspan = 0;
				foreach (var scoringGroup in model.GetUsingUnitScoringGroups(unit, model.ScoringGroups).Values)
				{
					var shouldBeSolvedSlides = model.ShouldBeSolvedSlidesByUnitScoringGroup[Tuple.Create(unit.Id, scoringGroup.Id)];
					colspan += shouldBeSolvedSlides.Count + 1;
					if (shouldBeSolvedSlides.Count > 0 && scoringGroup.CanBeSetByInstructor)
						colspan++;
				}

				builder.AddCell(unit.Title, colspan);
			}

			builder.PopStyleRule(); // Border.Left
			builder.GoToNewLine();

			builder.AddCell("Фамилия Имя");
			if (exportEmails) builder.AddCell("Эл. почта");
			builder.AddCell("Группа");
			foreach (var scoringGroup in model.ScoringGroups.Values)
				builder.AddCell(scoringGroup.Abbreviation);
			foreach (var unit in model.Units)
			{
				builder.AddStyleRuleForOneCell(s => s.Border.Left.Style = ExcelBorderStyle.Thin);
				foreach (var scoringGroup in model.GetUsingUnitScoringGroups(unit, model.ScoringGroups).Values)
				{
					var shouldBeSolvedSlides = model.ShouldBeSolvedSlidesByUnitScoringGroup[Tuple.Create(unit.Id, scoringGroup.Id)];
					builder.AddCell(scoringGroup.Abbreviation);

					builder.AddStyleRule(s => s.TextRotation = 90);
					builder.AddStyleRule(s => s.Font.Bold = false);
					foreach (var slide in shouldBeSolvedSlides)
						builder.AddCell($"{scoringGroup.Abbreviation}: {slide.Title}");
					if (shouldBeSolvedSlides.Count > 0 && scoringGroup.CanBeSetByInstructor)
						builder.AddCell("Доп");
					builder.PopStyleRule();
					builder.PopStyleRule();
				}
			}

			builder.GoToNewLine();

			builder.AddStyleRule(s => s.Border.Bottom.Style = ExcelBorderStyle.Thin);
			builder.AddStyleRuleForOneCell(s =>
			{
				s.HorizontalAlignment = ExcelHorizontalAlignment.Right;
				s.Font.Size = 10;
			});
			builder.AddCell("Максимум:", 3);
			foreach (var scoringGroup in model.ScoringGroups.Values)
				builder.AddCell(model.Units.Sum(unit => model.GetMaxScoreForUnitByScoringGroup(unit, scoringGroup)));
			foreach (var unit in model.Units)
			{
				builder.AddStyleRuleForOneCell(s => s.Border.Left.Style = ExcelBorderStyle.Thin);
				foreach (var scoringGroup in model.GetUsingUnitScoringGroups(unit, model.ScoringGroups).Values)
				{
					var shouldBeSolvedSlides = model.ShouldBeSolvedSlidesByUnitScoringGroup[Tuple.Create(unit.Id, scoringGroup.Id)];
					builder.AddCell(model.GetMaxScoreForUnitByScoringGroup(unit, scoringGroup));
					foreach (var slide in shouldBeSolvedSlides)
						builder.AddCell(slide.MaxScore);
					if (shouldBeSolvedSlides.Count > 0 && scoringGroup.CanBeSetByInstructor)
						builder.AddCell(scoringGroup.MaxAdditionalScore);
				}
			}

			builder.PopStyleRule(); // Bottom.Border
			builder.GoToNewLine();

			builder.AddStyleRule(s => s.Font.Bold = false);
			
			foreach (var user in model.VisitedUsers)
			{
				builder.AddCell(user.UserVisibleName);
				if (exportEmails) builder.AddCell(user.UserEmail);
				var userGroups = model.Groups.Where(g => model.VisitedUsersGroups[user.UserId].Contains(g.Id)).Select(g => g.Name).ToList();
				builder.AddCell(string.Join(", ", userGroups));
				foreach (var scoringGroup in model.ScoringGroups.Values)
				{
					var scoringGroupScore = model.Units.Sum(unit => model.GetTotalScoreForUserInUnitByScoringGroup(user.UserId, unit, scoringGroup));
					var scoringGroupOnlyFullScore = model.Units.Sum(unit => model.GetTotalOnlyFullScoreForUserInUnitByScoringGroup(user.UserId, unit, scoringGroup));
					builder.AddCell(onlyFullScores ? scoringGroupOnlyFullScore : scoringGroupScore);
				}

				foreach (var unit in model.Units)
				{
					builder.AddStyleRuleForOneCell(s => s.Border.Left.Style = ExcelBorderStyle.Thin);
					foreach (var scoringGroup in model.GetUsingUnitScoringGroups(unit, model.ScoringGroups).Values)
					{
						var shouldBeSolvedSlides = model.ShouldBeSolvedSlidesByUnitScoringGroup[Tuple.Create(unit.Id, scoringGroup.Id)];
						var scoringGroupScore = model.GetTotalScoreForUserInUnitByScoringGroup(user.UserId, unit, scoringGroup);
						var scoringGroupOnlyFullScore = model.GetTotalOnlyFullScoreForUserInUnitByScoringGroup(user.UserId, unit, scoringGroup);
						builder.AddCell(onlyFullScores ? scoringGroupOnlyFullScore : scoringGroupScore);
						foreach (var slide in shouldBeSolvedSlides)
						{
							var slideScore = model.ScoreByUserAndSlide[Tuple.Create(user.UserId, slide.Id)];
							builder.AddCell(onlyFullScores ? model.GetOnlyFullScore(slideScore, slide) : slideScore);
						}

						if (shouldBeSolvedSlides.Count > 0 && scoringGroup.CanBeSetByInstructor)
							builder.AddCell(model.AdditionalScores[Tuple.Create(user.UserId, unit.Id, scoringGroup.Id)]);
					}
				}
				builder.GoToNewLine();
			}
		}

		private async Task<CourseStatisticPageModel> GetCourseStatisticsModel(CourseStatisticsParams param, int usersLimit)
		{
			var courseId = param.CourseId;
			var periodStart = param.PeriodStartDate;
			var periodFinish = param.PeriodFinishDate;
			var groupsIds = param.GroupsIds ?? new List<string>();
			var isInstructor = await courseRolesRepo.HasUserAccessToCourse(UserId, courseId, CourseRoleType.Instructor);
			var isStudent = !isInstructor;
			if (isStudent && !await CanStudentViewGroupsStatistics(UserId, groupsIds))
				return null;

			var realPeriodFinish = periodFinish.Add(TimeSpan.FromDays(1));

			var course = courseStorage.GetCourse(courseId);
			var visibleUnitsIds = await unitsRepo.GetVisibleUnitIds(course, UserId);
			var visibleUnits = course.GetUnits(visibleUnitsIds);

			var slidesIds = visibleUnits.SelectMany(u => u.GetSlides(isInstructor).Select(s => s.Id)).ToHashSet();

			var filterOptions = await controllerUtils.GetFilterOptionsByGroup<VisitsFilterOptions>(UserId, courseId, groupsIds, allowSeeGroupForAnyMember: true);
			filterOptions.PeriodStart = periodStart;
			filterOptions.PeriodFinish = realPeriodFinish;

			List<string> usersIds;
			/* If we filtered out users from one or several groups show them all */
			if (filterOptions.UserIds != null && !filterOptions.IsUserIdsSupplement)
				usersIds = filterOptions.UserIds;
			else
				usersIds = await GetUsersIds(filterOptions);

			var visitedUsers = await GetUnitStatisticUserInfos(usersIds);
			var isMore = visitedUsers.Count > usersLimit;

			var unitBySlide = visibleUnits.SelectMany(u => u.GetSlides(isInstructor).Select(s => Tuple.Create(u.Id, s.Id))).ToDictionary(p => p.Item2, p => p.Item1);
			var scoringGroups = course.Settings.Scoring.Groups;

			var totalScoreByUserAllTime = await GetTotalScoreByUserAllTime(filterOptions);

			/* Get `usersLimit` best by slides count */
			visitedUsers = visitedUsers
				.OrderByDescending(u => totalScoreByUserAllTime[u.UserId])
				.Take(usersLimit)
				.ToList();
			var visitedUsersIds = visitedUsers.Select(v => v.UserId).ToList();

			// var visitedUsersGroups = groupsRepo.GetUsersActualGroupsIds(new List<string> { courseId }, visitedUsersIds, User, 10).ToDefaultDictionary();
			var visitedUsersGroups = 
				(await groupMembersRepo.GetUsersGroupsIdsAsync(courseId, visitedUsersIds)).ToDefaultDictionary();

			/* From now fetch only filtered users' statistics */
			filterOptions.UserIds = visitedUsersIds;
			filterOptions.IsUserIdsSupplement = false;
			var scoreByUserUnitScoringGroup = await GetScoreByUserUnitScoringGroup(filterOptions, slidesIds, unitBySlide, course);

			var shouldBeSolvedSlides = visibleUnits.SelectMany(u => u.GetSlides(isInstructor)).Where(s => s.ShouldBeSolved).ToList();
			var shouldBeSolvedSlidesIds = shouldBeSolvedSlides.Select(s => s.Id).ToHashSet();
			var shouldBeSolvedSlidesByUnitScoringGroup = GetShouldBeSolvedSlidesByUnitScoringGroup(shouldBeSolvedSlides, unitBySlide);
			var scoreByUserAndSlide = await GetScoreByUserAndSlide(filterOptions, shouldBeSolvedSlidesIds);

			var additionalScores = await GetAdditionalScores(courseId, visitedUsersIds);
			var usersGroupsIds = await groupMembersRepo.GetUsersGroupsIdsAsync(courseId, visitedUsersIds);
			var enabledAdditionalScoringGroupsForGroups = await GetEnabledAdditionalScoringGroupsForGroups(courseId);

			/* Filter out only scoring groups which are affected in selected groups */
			var additionalScoringGroupsForFilteredGroups = await controllerUtils.GetEnabledAdditionalScoringGroupsForGroups(course, groupsIds, UserId);
			scoringGroups = scoringGroups
				.Where(kv => kv.Value.MaxNotAdditionalScore > 0 || additionalScoringGroupsForFilteredGroups.Contains(kv.Key))
				.ToDictionary(kv => kv.Key, kv => kv.Value)
				.ToSortedDictionary();

			List<Group> groups;
			Dictionary<int, List<GroupAccess>> groupsAccesses = null;
			if (isInstructor)
			{
				groups = await groupAccessesRepo.GetAvailableForUserGroupsAsync(courseId, UserId, true, true, false);
				groupsAccesses = await groupAccessesRepo.GetGroupAccessesAsync(groups.Select(g => g.Id));
			}
			else
				groups = await groupMembersRepo.GetUserGroupsAsync(courseId, UserId);

			var model = new CourseStatisticPageModel
			{
				IsInstructor = isInstructor,
				CourseId = course.Id,
				CourseTitle = course.Title,
				Units = visibleUnits,
				SelectedGroupsIds = groupsIds,
				Groups = groups,
				GroupsAccesses = groupsAccesses,
				PeriodStart = periodStart,
				PeriodFinish = periodFinish,
				VisitedUsers = visitedUsers,
				VisitedUsersIsMore = isMore,
				VisitedUsersGroups = visitedUsersGroups,
				ShouldBeSolvedSlidesByUnitScoringGroup = shouldBeSolvedSlidesByUnitScoringGroup,
				ScoringGroups = scoringGroups,
				ScoreByUserUnitScoringGroup = scoreByUserUnitScoringGroup,
				ScoreByUserAndSlide = scoreByUserAndSlide,
				AdditionalScores = additionalScores,
				UsersGroupsIds = usersGroupsIds,
				EnabledAdditionalScoringGroupsForGroups = enabledAdditionalScoringGroupsForGroups,
			};
			return model;
		}

		private async Task<bool> CanStudentViewGroupsStatistics(string userId, List<string> groupsIds)
		{
			foreach (var groupId in groupsIds)
			{
				if (!int.TryParse(groupId, out var groupIdInt))
					return false;
				var usersIds = (await groupMembersRepo.GetGroupMembersAsUsersAsync(groupIdInt)).Select(u => u.Id);
				if (!usersIds.Contains(userId))
					return false;
			}

			return true;
		}

		private async Task<List<string>> GetUsersIds(VisitsFilterOptions filterOptions)
		{
			return await visitsRepo.GetVisitsInPeriod(filterOptions).Select(v => v.UserId).Distinct().ToListAsync();
		}

		private async Task<List<UnitStatisticUserInfo>> GetUnitStatisticUserInfos(List<string> usersIds)
		{
			return (await db.Users.Where(u => usersIds.Contains(u.Id))
					.Select(u => new { u.Id, u.UserName, u.Email, u.FirstName, u.LastName })
					.ToListAsync())
				.Select(u => new UnitStatisticUserInfo(u.Id, u.UserName, u.Email, u.FirstName, u.LastName)).ToList();
		}

		private async Task<DefaultDictionary<string, int>> GetTotalScoreByUserAllTime(VisitsFilterOptions filterOptions)
		{
			return (await visitsRepo.GetVisitsInPeriod(filterOptions.WithPeriodStart(DateTime.MinValue).WithPeriodFinish(DateTime.MaxValue))
					.GroupBy(v => v.UserId)
					.Select(g => new { g.Key, Sum = g.Sum(v => v.Score) })
					.ToListAsync())
				.ToDictionary(g => g.Key, g => g.Sum)
				.ToDefaultDictionary();
		}

		private async Task<DefaultDictionary<Tuple<string, Guid, string>, int>> GetScoreByUserUnitScoringGroup(VisitsFilterOptions filterOptions, HashSet<Guid> slides, Dictionary<Guid, Guid> unitBySlide, Course course)
		{
			return (await visitsRepo.GetVisitsInPeriod(filterOptions)
				.Select(v => new { v.UserId, v.SlideId, v.Score })
				.ToListAsync())
				.Where(v => slides.Contains(v.SlideId))
				.GroupBy(v => Tuple.Create(v.UserId, unitBySlide[v.SlideId], course.FindSlideByIdNotSafe(v.SlideId)?.ScoringGroup))
				.ToDictionary(g => g.Key, g => g.Sum(v => v.Score))
				.ToDefaultDictionary();
		}

		private static DefaultDictionary<Tuple<Guid, string>, List<Slide>> GetShouldBeSolvedSlidesByUnitScoringGroup(List<Slide> shouldBeSolvedSlides, Dictionary<Guid, Guid> unitBySlide)
		{
			return shouldBeSolvedSlides
				.GroupBy(s => Tuple.Create(unitBySlide[s.Id], s.ScoringGroup))
				.ToDictionary(g => g.Key, g => g.ToList())
				.ToDefaultDictionary();
		}

		private async Task<DefaultDictionary<Tuple<string, Guid>, int>> GetScoreByUserAndSlide(VisitsFilterOptions filterOptions, HashSet<Guid> shouldBeSolvedSlidesIds)
		{
			return (await visitsRepo.GetVisitsInPeriod(filterOptions)
				.Select(v => new { v.UserId, v.SlideId, v.Score })
				.ToListAsync())
				.Where(e => shouldBeSolvedSlidesIds.Contains(e.SlideId))
				.GroupBy(v => Tuple.Create(v.UserId, v.SlideId))
				.ToDictionary(g => g.Key, g => g.Sum(v => v.Score))
				.ToDefaultDictionary();
		}

		private async Task<DefaultDictionary<Tuple<string, Guid, string>, int>> GetAdditionalScores(string courseId, List<string> visitedUsersIds)
		{
			return (await additionalScoresRepo
					.GetAdditionalScoresForUsers(courseId, visitedUsersIds))
				.ToDictionary(kv => kv.Key, kv => kv.Value.Score)
				.ToDefaultDictionary();
		}

		private async Task<Dictionary<int, List<string>>> GetEnabledAdditionalScoringGroupsForGroups(string courseId)
		{
			return (await groupsRepo.GetEnabledAdditionalScoringGroupsAsync(courseId))
				.GroupBy(e => e.GroupId)
				.ToDictionary(g => g.Key, g => g.Select(e => e.ScoringGroupId).ToList());
		}
	}
}