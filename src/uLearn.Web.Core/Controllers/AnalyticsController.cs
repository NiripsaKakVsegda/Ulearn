using System.Globalization;
using System.Net;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Groups;
using Database.Repos.SystemAccessesRepo;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Telegram.Bot.Types;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Core.Courses.Units;
using uLearn.Web.Core.Authorization;
using uLearn.Web.Core.Extensions;
using uLearn.Web.Core.Models;

namespace uLearn.Web.Core.Controllers;

[Authorize(Policy = UlearnAuthorizationBuilder.StudentsPolicyName)]//ULearnAuthorize(MinAccessLevel = CourseRoleType.Student)
public class AnalyticsController : JsonDataContractController
{
	private readonly UlearnDb db;
	private readonly ICourseStorage courseStorage;

	private readonly IVisitsRepo visitsRepo;
	private readonly IUserSolutionsRepo userSolutionsRepo;
	private readonly IGroupsRepo groupsRepo;
	private readonly IGroupMembersRepo groupMembersRepo;
	private readonly IGroupAccessesRepo groupAccessesRepo;
	private readonly IUsersRepo usersRepo;
	private readonly IUnitsRepo unitsRepo;
	private readonly IAdditionalScoresRepo additionalScoresRepo;
	private readonly UlearnConfiguration configuration;
	private readonly IGoogleSheetExportTasksRepo googleSheetExportTasksRepo;
	private readonly ISystemAccessesRepo systemAccessesRepo;

	public AnalyticsController(
		UlearnDb db,
		ICourseStorage courseStorage,
		IVisitsRepo visitsRepo,
		IUserSolutionsRepo userSolutionsRepo,
		IGroupsRepo groupsRepo,
		IGroupMembersRepo groupMembersRepo,
		IGroupAccessesRepo groupAccessesRepo,
		IUsersRepo usersRepo,
		IUnitsRepo unitsRepo,
		IAdditionalScoresRepo additionalScoresRepo,
		IGoogleSheetExportTasksRepo googleSheetExportTasksRepo,
		ISystemAccessesRepo systemAccessesRepo
	)
	{
		this.db = db;
		this.courseStorage = courseStorage;
		this.additionalScoresRepo = additionalScoresRepo;
		this.userSolutionsRepo = userSolutionsRepo;
		this.groupsRepo = groupsRepo;
		this.groupMembersRepo = groupMembersRepo;
		this.groupAccessesRepo = groupAccessesRepo;
		this.usersRepo = usersRepo;
		this.visitsRepo = visitsRepo;
		this.unitsRepo = unitsRepo;
		this.googleSheetExportTasksRepo = googleSheetExportTasksRepo;
		this.systemAccessesRepo = systemAccessesRepo;
		this.configuration = ApplicationConfiguration.Read<UlearnConfiguration>();
	}

	[Authorize(Policy = UlearnAuthorizationBuilder.InstructorsPolicyName)]//[Authorize(Policy = UlearnAuthorization.InstructorsPolicyName)] //[ULearnAuthorize(MinAccessLevel = CourseRoleType.Instructor)]
	public async Task<ActionResult> UnitSheet(UnitSheetParams param)
	{
		const int usersLimit = 200;

		if (param.CourseId == null)
			return new NotFoundResult();

		var courseId = param.CourseId;
		var unitId = param.UnitId;
		var periodStart = param.PeriodStartDate;
		var periodFinish = param.PeriodFinishDate;
		var groupsIds = Request.GetMultipleValuesFromQueryString("group");

		var realPeriodFinish = periodFinish.Add(TimeSpan.FromDays(1));

		var course = courseStorage.GetCourse(courseId);
		var visibleUnitsIds = await unitsRepo.GetVisibleUnitIds(course, User.GetUserId());
		var visibleUnits = course.GetUnits(visibleUnitsIds);
		if (!unitId.HasValue)
			return View("UnitSheetList", new UnitSheetPageModel
			{
				CourseId = courseId,
				CourseTitle = course.Title,
				Units = visibleUnits
			});
		var selectedUnit = visibleUnits.FirstOrDefault(x => x.Id == unitId);
		if (selectedUnit == null)
			return new NotFoundResult();

		var slides = selectedUnit.GetSlides(false);
		var slidesIds = slides.Select(s => s.Id).ToList();

		var groups = await groupAccessesRepo.GetAvailableForUserGroupsAsync(courseId, User.GetUserId(), true, true, false);
		var groupsAccesses = groupAccessesRepo.GetGroupsAccesses(groups.Select(g => g.Id));
		var filterOptions = await ControllerUtils.GetFilterOptionsByGroup<VisitsFilterOptions>(groupsRepo, groupAccessesRepo, User, courseId, groupsIds);
		filterOptions.SlidesIds = slidesIds;
		filterOptions.PeriodStart = periodStart;
		filterOptions.PeriodFinish = realPeriodFinish;

		var slidesVisits = await visitsRepo.GetVisitsInPeriodForEachSlide(filterOptions);

		List<string> usersIds;
		/* If we filtered out users from one or several groups show them all */
		if (filterOptions.UserIds != null && !filterOptions.IsUserIdsSupplement)
			usersIds = filterOptions.UserIds;
		else
			usersIds = visitsRepo.GetVisitsInPeriod(filterOptions).Select(v => v.UserId).Distinct().ToList();

		var visitedUsers = GetUnitStatisticUserInfos(usersIds);
		var isMore = visitedUsers.Count > usersLimit;

		var visitedSlidesCountByUser = GetVisitedSlidesCountByUser(filterOptions);
		var visitedSlidesCountByUserAllTime = GetVisitedSlidesCountByUserAllTime(filterOptions);

		/* Get `usersLimit` best by slides count and order them by name */
		visitedUsers = visitedUsers
			.OrderByDescending(u => visitedSlidesCountByUserAllTime.GetOrDefault(u.UserId, 0))
			.Take(usersLimit)
			.OrderBy(u => u.UserLastName)
			.ThenBy(u => u.UserVisibleName)
			.ToList();

		var visitedUsersIds = visitedUsers.Select(v => v.UserId).ToList();
		var additionalScores = await GetAdditionalScores(courseId, unitId.Value, visitedUsersIds);
		var usersGroupsIds = await groupMembersRepo.GetUsersGroupsIdsAsync(courseId, visitedUsersIds);
		var enabledAdditionalScoringGroupsForGroups = await GetEnabledAdditionalScoringGroupsForGroups(courseId);

		var model = new UnitSheetPageModel
		{
			CourseId = courseId,
			CourseTitle = course.Title,
			Units = visibleUnits,
			Unit = selectedUnit,
			SelectedGroupsIds = groupsIds,
			Groups = groups,
			GroupsAccesses = groupsAccesses,
			ShowStatisticsLink = User.HasAccessFor(courseId, CourseRoleType.CourseAdmin),

			PeriodStart = periodStart,
			PeriodFinish = periodFinish,

			Slides = slides,
			SlidesVisits = slidesVisits,

			VisitedUsers = visitedUsers,
			VisitedUsersIsMore = isMore,
			VisitedSlidesCountByUser = visitedSlidesCountByUser,
			VisitedSlidesCountByUserAllTime = visitedSlidesCountByUserAllTime,

			AdditionalScores = additionalScores,
			UsersGroupsIds = usersGroupsIds,
			EnabledAdditionalScoringGroupsForGroups = enabledAdditionalScoringGroupsForGroups,
		};
		return View(model);
	}

	[Authorize(Policy = UlearnAuthorizationBuilder.CourseAdminsPolicyName)]//[Authorize(Policy = UlearnAuthorization.CourseAdminsPolicyName)] //[ULearnAuthorize(MinAccessLevel = CourseRoleType.CourseAdmin)]
	public async Task<ActionResult> UnitStatistics(UnitSheetParams param)
	{
		if (param.CourseId == null)
			return new NotFoundResult();

		var courseId = param.CourseId;
		var unitId = param.UnitId;
		var periodStart = param.PeriodStartDate;
		var periodFinish = param.PeriodFinishDate;
		var groupsIds = Request.GetMultipleValuesFromQueryString("group");

		var realPeriodFinish = periodFinish.Add(TimeSpan.FromDays(1));

		var course = courseStorage.GetCourse(courseId);
		var visibleUnitsIds = await unitsRepo.GetVisibleUnitIds(course, User.GetUserId());
		var visibleUnits = course.GetUnits(visibleUnitsIds);
		if (!unitId.HasValue)
			return View("UnitSheetList", new UnitSheetPageModel
			{
				CourseId = courseId,
				CourseTitle = course.Title,
				Units = visibleUnits
			});

		var selectedUnit = visibleUnits.FirstOrDefault(x => x.Id == unitId);
		if (selectedUnit == null)
			return new NotFoundResult();

		var slides = selectedUnit.GetSlides(false);
		var slidesIds = slides.Select(s => s.Id).ToList();

		var groups = await groupAccessesRepo.GetAvailableForUserGroupsAsync(courseId, User.GetUserId(), true, true, false);
		var groupsAccesses = groupAccessesRepo.GetGroupsAccesses(groups.Select(g => g.Id));
		var filterOptions = await ControllerUtils.GetFilterOptionsByGroup<VisitsFilterOptions>(groupsRepo, groupAccessesRepo, User, courseId, groupsIds);
		filterOptions.SlidesIds = slidesIds;
		filterOptions.PeriodStart = periodStart;
		filterOptions.PeriodFinish = realPeriodFinish;

		var slidesVisits = await visitsRepo.GetVisitsInPeriodForEachSlide(filterOptions);

		var visitedSlidesCountByUser = GetVisitedSlidesCountByUser(filterOptions);
		var visitedSlidesCountByUserAllTime = GetVisitedSlidesCountByUserAllTime(filterOptions);

		var model = new UnitStatModel
		{
			CourseId = courseId,
			CourseTitle = course.Title,
			Units = visibleUnits,
			Unit = selectedUnit,
			SelectedGroupsIds = groupsIds,
			Groups = groups,
			GroupsAccesses = groupsAccesses,

			PeriodStart = periodStart,
			PeriodFinish = periodFinish,

			Slides = slides,
			SlidesVisits = slidesVisits,

			VisitedSlidesCountByUser = visitedSlidesCountByUser,
			VisitedSlidesCountByUserAllTime = visitedSlidesCountByUserAllTime,
		};

		model.UsersVisitedAllSlidesInPeriodCount = (await visitsRepo.GetUsersVisitedAllSlides(filterOptions)).Count;

		var quizzes = slides.OfType<QuizSlide>();
		model.QuizzesAverageScore = quizzes.ToDictionary(q => q.Id,
			q => (int)slidesVisits.GetOrDefault(q.Id, new List<Visit>())
				.Where(v => v.IsPassed)
				.Select(v => 100 * Math.Min(v.Score, q.MaxScore) / (q.MaxScore != 0 ? q.MaxScore : 1))
				.DefaultIfEmpty(-1)
				.Average()
		);

		model.ExercisesSolutionsCount = GetExercisesSolutionsCount(courseId, slidesIds, periodStart, realPeriodFinish);
		model.ExercisesAcceptedSolutionsCount = GetExercisesAcceptedSolutionsCount(courseId, slidesIds, periodStart, realPeriodFinish);

		return View(model);
	}

	private async Task<Dictionary<int, List<string>>> GetEnabledAdditionalScoringGroupsForGroups(string courseId)
	{
		return (await groupsRepo.GetEnabledAdditionalScoringGroupsAsync(courseId))
			.GroupBy(e => e.GroupId)
			.ToDictionary(g => g.Key, g => g.Select(e => e.ScoringGroupId).ToList());
	}

	private async Task<Dictionary<Tuple<string, string>, int>> GetAdditionalScores(string courseId, Guid unitId, List<string> visitedUsersIds)
	{
		return (await additionalScoresRepo
				.GetAdditionalScoresForUsers(courseId, unitId, visitedUsersIds))
			.ToDictionary(kv => kv.Key, kv => kv.Value.Score);
	}

	private Dictionary<string, int> GetVisitedSlidesCountByUserAllTime(VisitsFilterOptions filterOptions)
	{
		return visitsRepo.GetVisitsInPeriod(filterOptions.WithPeriodStart(DateTime.MinValue).WithPeriodFinish(DateTime.MaxValue))
			.GroupBy(v => v.UserId)
			.Select(g => new { g.Key, Count = g.Count() })
			.ToDictionary(g => g.Key, g => g.Count);
	}

	private Dictionary<string, int> GetVisitedSlidesCountByUser(VisitsFilterOptions filterOptions)
	{
		return visitsRepo.GetVisitsInPeriod(filterOptions)
			.GroupBy(v => v.UserId)
			.Select(g => new { g.Key, Count = g.Count() })
			.ToDictionary(g => g.Key, g => g.Count);
	}

	private Dictionary<Guid, int> GetExercisesAcceptedSolutionsCount(string courseId, List<Guid> slidesIds, DateTime periodStart, DateTime realPeriodFinish)
	{
		return userSolutionsRepo.GetAllAcceptedSubmissions(courseId, slidesIds, periodStart, realPeriodFinish)
			.Select(s => new { s.SlideId, s.UserId })
			.Distinct()
			.GroupBy(s => s.SlideId)
			.Select(g => new { g.Key, Count = g.Count() })
			.ToDictionary(g => g.Key, g => g.Count);
	}

	private Dictionary<Guid, int> GetExercisesSolutionsCount(string courseId, List<Guid> slidesIds, DateTime periodStart, DateTime realPeriodFinish)
	{
		/* Dictionary<SlideId, count (distinct by user)> */
		return userSolutionsRepo.GetAllSubmissions(courseId, slidesIds, periodStart, realPeriodFinish)
			.Select(s => new { s.SlideId, s.UserId })
			.Distinct()
			.GroupBy(s => s.SlideId)
			.Select(g => new { g.Key, Count = g.Count() })
			.ToDictionary(g => g.Key, g => g.Count);
	}

	private async Task<bool> CanStudentViewGroupsStatistics(string userId, List<string> groupsIds)
	{
		foreach (var groupId in groupsIds)
		{
			int groupIdInt;
			if (!int.TryParse(groupId, out groupIdInt))
				return false;
			var usersIds = (await groupMembersRepo.GetGroupMembersAsUsersAsync(groupIdInt))
				.Select(u => u.Id);
			if (!usersIds.Contains(userId))
				return false;
		}

		return true;
	}


	[Authorize(Policy = UlearnAuthorizationBuilder.StudentsPolicyName)]//[ULearnAuthorize(MinAccessLevel = CourseRoleType.Student)]
	public async Task<ActionResult> CourseStatistics(CourseStatisticsParams param, int max = 200)
	{
		if (param.CourseId == null)
			return new NotFoundResult();

		var usersLimit = max;
		if (usersLimit > 400)
			usersLimit = 400;
		if (usersLimit < 0)
			usersLimit = 100;

		var model = await GetCourseStatisticsModel(param, usersLimit);
		if (model == null)
			return new NotFoundResult();
		return View(model);
	}

	/* TODO: extract copy-paste */
	private async Task<CourseStatisticPageModel> GetCourseStatisticsModel(CourseStatisticsParams param, int usersLimit)
	{
		var courseId = param.CourseId;
		var periodStart = param.PeriodStartDate;
		var periodFinish = param.PeriodFinishDate;
		var groupsIds = Request.GetMultipleValuesFromQueryString("groupsIds").Concat(Request.GetMultipleValuesFromQueryString("group")).ToList();
		var isInstructor = User.HasAccessFor(courseId, CourseRoleType.Instructor);
		var isStudent = !isInstructor;
		var isAdmin = User.HasAccessFor(courseId, CourseRoleType.CourseAdmin);

		var currentUserId = User.GetUserId();
		if (isStudent && !await CanStudentViewGroupsStatistics(currentUserId, groupsIds))
			return null;

		var realPeriodFinish = periodFinish.Add(TimeSpan.FromDays(1));

		var course = courseStorage.GetCourse(courseId);
		var visibleUnitsIds = await unitsRepo.GetVisibleUnitIds(course, User.GetUserId());
		var visibleUnits = course.GetUnits(visibleUnitsIds);

		var slidesIds = visibleUnits.SelectMany(u => u.GetSlides(isInstructor).Select(s => s.Id)).ToHashSet();

		var filterOptions = await ControllerUtils.GetFilterOptionsByGroup<VisitsFilterOptions>(groupsRepo, groupAccessesRepo, User, courseId, groupsIds, allowSeeGroupForAnyMember: true);
		filterOptions.PeriodStart = periodStart;
		filterOptions.PeriodFinish = realPeriodFinish;

		List<string> usersIds;
		/* If we filtered out users from one or several groups show them all */
		if (filterOptions.UserIds != null && !filterOptions.IsUserIdsSupplement)
			usersIds = filterOptions.UserIds;
		else
			usersIds = GetUsersIds(filterOptions);

		var visitedUsers = GetUnitStatisticUserInfos(usersIds);
		var isMore = visitedUsers.Count > usersLimit;

		var unitBySlide = visibleUnits.SelectMany(u => u.GetSlides(isInstructor).Select(s => Tuple.Create(u.Id, s.Id))).ToDictionary(p => p.Item2, p => p.Item1);
		var scoringGroups = course.Settings.Scoring.Groups;

		var totalScoreByUserAllTime = GetTotalScoreByUserAllTime(filterOptions);

		/* Get `usersLimit` best by slides count */
		visitedUsers = visitedUsers
			.OrderByDescending(u => totalScoreByUserAllTime[u.UserId])
			.Take(usersLimit)
			.ToList();
		var visitedUsersIds = visitedUsers.Select(v => v.UserId).ToList();

		var visitedUsersGroups = (await groupAccessesRepo.GetUsersActualGroupsIds(new List<string> { courseId }, visitedUsersIds, User.GetUserId(), 10)).ToDefaultDictionary();

		/* From now fetch only filtered users' statistics */
		filterOptions.UserIds = visitedUsersIds;
		filterOptions.IsUserIdsSupplement = false;
		var scoreByUserUnitScoringGroup = GetScoreByUserUnitScoringGroup(filterOptions, slidesIds, unitBySlide, course);

		var shouldBeSolvedSlides = visibleUnits.SelectMany(u => u.GetSlides(isInstructor)).Where(s => s.ShouldBeSolved).ToList();
		var shouldBeSolvedSlidesIds = shouldBeSolvedSlides.Select(s => s.Id).ToHashSet();
		var shouldBeSolvedSlidesByUnitScoringGroup = GetShouldBeSolvedSlidesByUnitScoringGroup(shouldBeSolvedSlides, unitBySlide);
		var scoreByUserAndSlide = GetScoreByUserAndSlide(filterOptions, shouldBeSolvedSlidesIds);

		var additionalScores = await GetAdditionalScores(courseId, visitedUsersIds);
		var usersGroupsIds = await groupMembersRepo.GetUsersGroupsIdsAsync(courseId, visitedUsersIds);
		var enabledAdditionalScoringGroupsForGroups = await GetEnabledAdditionalScoringGroupsForGroups(courseId);

		/* Filter out only scoring groups which are affected in selected groups */
		var additionalScoringGroupsForFilteredGroups = await ControllerUtils.GetEnabledAdditionalScoringGroupsForGroups(groupsRepo, course, groupsIds, User);
		scoringGroups = scoringGroups
			.Where(kv => kv.Value.MaxNotAdditionalScore > 0 || additionalScoringGroupsForFilteredGroups.Contains(kv.Key))
			.ToDictionary(kv => kv.Key, kv => kv.Value)
			.ToSortedDictionary();

		List<Group> groups;
		Dictionary<int, List<GroupAccess>> groupsAccesses = null;
		if (isInstructor)
		{
			groups = await groupAccessesRepo.GetAvailableForUserGroupsAsync(courseId, User.GetUserId(), true, true, false);
			groupsAccesses = groupAccessesRepo.GetGroupsAccesses(groups.Select(g => g.Id));
		}
		else
			groups = await groupMembersRepo.GetUserGroupsAsync(course.Id, currentUserId);

		var canViewProfiles = await systemAccessesRepo.HasSystemAccessAsync(currentUserId, SystemAccessType.ViewAllProfiles) || User.IsSystemAdministrator();

		var uriBuilder = new ExportUriBuilder(configuration.BaseUrlApi, courseId);
		var jsonExportUrl = uriBuilder.BuildExportJsonUrl();
		var xmlExportUrl = uriBuilder.BuildExportXmlUrl();
		var xlsxExportUrl = uriBuilder.BuildExportXlsxUrl();

		var visibleGoogleSheetTasks = await googleSheetExportTasksRepo
			.GetVisibleGoogleSheetTask(courseId, groups.Where(g => groupsIds.Contains(g.Id.ToString())).ToList(), User.GetUserId());

		var groupNamesToGoogleSheetLink = new List<(string, string, bool)>();

		if (visibleGoogleSheetTasks != null)
			groupNamesToGoogleSheetLink = visibleGoogleSheetTasks
				.Select(t => (string.Join(", ", t.Groups
						.Select(g => g.Group.Name)),
					$"https://docs.google.com/spreadsheets/d/{t.SpreadsheetId}/edit#gid={t.ListId}",
					t.IsVisibleForStudents))
				.ToList();

		var model = new CourseStatisticPageModel
		{
			IsInstructor = isInstructor,
			IsAdmin = isAdmin,
			CanViewProfiles = canViewProfiles,
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
			JsonExportUrl = jsonExportUrl,
			XmlExportUrl = xmlExportUrl,
			XlsxExportUrl = xlsxExportUrl,
			GroupNamesToGoogleSheetLink = groupNamesToGoogleSheetLink
		};
		return model;
	}

	private List<UnitStatisticUserInfo> GetUnitStatisticUserInfos(List<string> usersIds)
	{
		return db.Users.Where(u => usersIds.Contains(u.Id))
			.Select(u => new { u.Id, u.UserName, u.Email, u.FirstName, u.LastName })
			.AsEnumerable()
			.Select(u => new UnitStatisticUserInfo(u.Id, u.UserName, u.Email, u.FirstName, u.LastName)).ToList();
	}

	private List<string> GetUsersIds(VisitsFilterOptions filterOptions)
	{
		return visitsRepo.GetVisitsInPeriod(filterOptions).Select(v => v.UserId).Distinct().ToList();
	}

	private async Task<DefaultDictionary<Tuple<string, Guid, string>, int>> GetAdditionalScores(string courseId, List<string> visitedUsersIds)
	{
		return (await additionalScoresRepo
				.GetAdditionalScoresForUsers(courseId, visitedUsersIds))
			.ToDictionary(kv => kv.Key, kv => kv.Value.Score)
			.ToDefaultDictionary();
	}

	private DefaultDictionary<Tuple<string, Guid>, int> GetScoreByUserAndSlide(VisitsFilterOptions filterOptions, HashSet<Guid> shouldBeSolvedSlidesIds)
	{
		return visitsRepo.GetVisitsInPeriod(filterOptions)
			.Select(v => new { v.UserId, v.SlideId, v.Score })
			.AsEnumerable()
			.Where(e => shouldBeSolvedSlidesIds.Contains(e.SlideId))
			.GroupBy(v => Tuple.Create(v.UserId, v.SlideId))
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

	private DefaultDictionary<Tuple<string, Guid, string>, int> GetScoreByUserUnitScoringGroup(VisitsFilterOptions filterOptions, HashSet<Guid> slides, Dictionary<Guid, Guid> unitBySlide, Course course)
	{
		return visitsRepo.GetVisitsInPeriod(filterOptions)
			.Select(v => new { v.UserId, v.SlideId, v.Score })
			.AsEnumerable()
			.Where(v => slides.Contains(v.SlideId))
			.GroupBy(v => Tuple.Create(v.UserId, unitBySlide[v.SlideId], course.FindSlideByIdNotSafe(v.SlideId)?.ScoringGroup))
			.ToDictionary(g => g.Key, g => g.Sum(v => v.Score))
			.ToDefaultDictionary();
	}

	private DefaultDictionary<string, int> GetTotalScoreByUserAllTime(VisitsFilterOptions filterOptions)
	{
		return visitsRepo.GetVisitsInPeriod(filterOptions.WithPeriodStart(DateTime.MinValue).WithPeriodFinish(DateTime.MaxValue))
			.GroupBy(v => v.UserId)
			.Select(g => new { g.Key, Sum = g.Sum(v => v.Score) })
			.ToDictionary(g => g.Key, g => g.Sum)
			.ToDefaultDictionary();
	}

	[Authorize(Policy = UlearnAuthorizationBuilder.InstructorsPolicyName)] //[ULearnAuthorize(MinAccessLevel = CourseRoleType.Instructor)]
	public async Task<ActionResult> UserUnitStatistics(string courseId, Guid unitId, string userId)
	{
		var course = courseStorage.GetCourse(courseId);
		var user = await usersRepo.FindUserById(userId);
		if (user == null)
			return new NotFoundResult();

		var visibleUnitsIds = await unitsRepo.GetVisibleUnitIds(course, User.GetUserId());
		var visibleUnits = course.GetUnits(visibleUnitsIds);
		var unit = visibleUnits.FirstOrDefault(x => x.Id == unitId);
		if (unit == null)
			return new NotFoundResult();

		var slides = unit.GetSlides(true);
		var exercises = slides.OfType<ExerciseSlide>().ToList();
		var acceptedSubmissions = userSolutionsRepo
			.GetAllAcceptedSubmissionsByUser(courseId, exercises.Select(s => s.Id), userId)
			.OrderByDescending(s => s.Timestamp)
			.Deprecated_DistinctBy(u => u.SlideId)
			.ToList();
		var reviewedSubmissions = userSolutionsRepo
			.GetAllAcceptedSubmissionsByUser(courseId, exercises.Select(s => s.Id), userId)
			.Where(s => s.ManualChecking != null && s.ManualChecking.IsChecked)
			.OrderByDescending(s => s.Timestamp)
			.Deprecated_DistinctBy(u => u.SlideId)
			.ToList();
		var userScores = await visitsRepo.GetScoresForSlides(courseId, userId, slides.Select(s => s.Id));

		var unitIndex = visibleUnits.FindIndex(u => u.Id == unitId);
		var previousUnit = unitIndex == 0 ? null : visibleUnits[unitIndex - 1];
		var nextUnit = unitIndex == visibleUnits.Count - 1 ? null : visibleUnits[unitIndex + 1];

		var model = new UserUnitStatisticsPageModel
		{
			Course = course,
			Unit = unit,
			User = user,
			Slides = slides.ToDictionary(s => s.Id),
			Submissions = acceptedSubmissions,
			ReviewedSubmissions = reviewedSubmissions,
			Scores = userScores,
			PreviousUnit = previousUnit,
			NextUnit = nextUnit,
		};

		return View(model);
	}

	[Authorize(Policy = UlearnAuthorizationBuilder.StudentsPolicyName)]//[ULearnAuthorize(MinAccessLevel = CourseRoleType.Student)]
	public async Task<ActionResult> RatingByPoints(string courseId, Guid slideId, int? groupId = null)
	{
		var course = courseStorage.FindCourse(courseId);
		if (course == null)
			return new NotFoundResult();
		var isInstructor = User.HasAccessFor(courseId, CourseRoleType.Instructor);
		var visibleUnits = await unitsRepo.GetVisibleUnitIds(course, User.GetUserId());
		var slide = course.FindSlideById(slideId, isInstructor, visibleUnits);
		var exerciseBlock = slide?.Blocks.OfType<AbstractExerciseBlock>().FirstOrDefault();
		if (exerciseBlock == null)
			return new NotFoundResult();
		var smallPointsIsBetter = exerciseBlock.SmallPointsIsBetter;

		var currentUserId = User.GetUserId();
		var isAdministrator = User.HasAccessFor(courseId, CourseRoleType.CourseAdmin);
		var isStudent = !isInstructor;

		Group selectedGroup = null;
		List<Group> availableGroups = null;
		List<ApplicationUser> users = null; // null, если все пользователи
		var hideOtherUsersNames = false;
		var showAllUsers = false;
		if (groupId != null)
		{
			selectedGroup = await groupsRepo.FindGroupByIdAsync(groupId.Value);
			if (selectedGroup == null)
				return new NotFoundResult();
			users = await groupMembersRepo.GetGroupMembersAsUsersAsync(groupId.Value);
			if (isStudent && !users.Select(u => u.Id).Contains(currentUserId))
				return new ForbidResult();
			if (isInstructor && !await groupAccessesRepo.IsGroupVisibleForUserAsync(groupId.Value, User.GetUserId())) //groupsRepo.IsGroupAvailableForUser(groupId.Value, User)
				return new ForbidResult();
		}
		else
		{
			if (isInstructor)
			{
				availableGroups = await groupAccessesRepo.GetAvailableForUserGroupsAsync(courseId, User.GetUserId(), true, true, false);
				if (isAdministrator)
				{
					showAllUsers = true;
				}
				else
				{
					if (availableGroups.Count > 0)
						users = groupsRepo.GetGroupsMembersAsUsers(availableGroups.Select(g => g.Id).ToList());
					else
						hideOtherUsersNames = true;
				}
			}
			else
			{
				availableGroups = await groupMembersRepo.GetUserGroupsAsync(courseId, currentUserId);
				if (availableGroups.Count > 0)
					users = groupsRepo.GetGroupsMembersAsUsers(availableGroups.Select(g => g.Id).ToList());
				else
					hideOtherUsersNames = true;
			}
		}

		var userIds = users?.Select(u => u.Id).ToList();
		var pointsByUser = GetPointsByUser(courseId, slideId, userIds);
		var usersOrderedByPoints = GetUsersOrderedByPoints(pointsByUser, smallPointsIsBetter);

		if (showAllUsers)
			users = (await usersRepo.GetUsersByIds(usersOrderedByPoints)).ToList();
		if (users == null)
			users = (await usersRepo.GetUsersByIds(new[] { currentUserId })).ToList();

		var model = new ExerciseRatingByPointsModel
		{
			Course = course,
			Slide = slide,
			SelectedGroup = selectedGroup,
			AvailableGroups = availableGroups,
			Users = users?.ToDictionary(u => u.Id, u => u),
			HideOtherUsersNames = hideOtherUsersNames,
			PointsByUser = pointsByUser.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
			UsersOrderedByPoints = usersOrderedByPoints
		};
		return View(model);
	}

	private Dictionary<string, (float Points, DateTime Timestamp)> GetPointsByUser(string courseId, Guid slideId, List<string> userIds)
	{
		return userSolutionsRepo
			.GetAutomaticExerciseCheckingsByUsers(courseId, slideId, userIds)
			.Where(c => c.Points != null)
			.Select(v => new { v.UserId, Points = v.Points.Value, v.Timestamp })
			.GroupBy(v => v.UserId)
			.Select(g => g.OrderByDescending(r => r.Timestamp).FirstOrDefault())
			.AsEnumerable()
			.ToDictionary(
				g => g.UserId,
				g => (g.Points, g.Timestamp));
	}

	private List<string> GetUsersOrderedByPoints(Dictionary<string, (float Points, DateTime Timestamp)> pointsByUser, bool smallPointsIsBetter)
	{
		var ordered = smallPointsIsBetter
			? pointsByUser.OrderBy(p => p.Value.Points).ThenBy(p => p.Value.Timestamp)
			: pointsByUser.OrderByDescending(p => p.Value.Points).ThenBy(p => p.Value.Timestamp);
		return ordered.Select(p => p.Key).ToList();
	}

	[Authorize(Policy = UlearnAuthorizationBuilder.InstructorsPolicyName)] //[ULearnAuthorize(MinAccessLevel = CourseRoleType.Instructor)]
	public async Task<ActionResult> UsersProgress(string courseId, Guid unitId, DateTime periodStart)
	{
		var course = courseStorage.GetCourse(courseId);
		var unit = course.FindUnitByIdNotSafe(unitId);
		if (unit == null)
			return new NotFoundResult();
		var slides = unit.GetSlides(true).ToArray();
		var users = GetUserInfos(courseId, slides, periodStart).OrderByDescending(GetRating).ToArray();

		return PartialView(new UserProgressViewModel
		{
			Slides = slides,
			Users = users,
			GroupsNames = await groupAccessesRepo.GetUsersGroupsNamesAsStrings(courseId, users.Select(u => u.UserId), User.GetUserId(), actual: true, archived: false),
			CourseId = courseId
		});
	}

	private double GetRating(UserInfo user)
	{
		return
			user.SlidesSlideInfo.Sum(
				s =>
					(s.IsVisited ? 1 : 0)
					+ (s.IsExerciseSolved ? 1 : 0)
					+ (s.IsQuizPassed ? s.QuizPercentage / 100.0 : 0));
	}

	private IEnumerable<UserInfo> GetUserInfos(string courseId, Slide[] slides, DateTime periodStart, DateTime? periodFinish = null)
	{
		if (!periodFinish.HasValue)
			periodFinish = DateTime.Now;

		var slidesIds = slides.Select(s => s.Id).ToHashSet();

		var dq = visitsRepo.GetVisitsInPeriod(courseId, slidesIds, periodStart, periodFinish.Value)
			.Select(v => v.UserId)
			.Distinct()
			.Join(db.Visits, s => s, v => v.UserId, (s, visiters) => visiters)
			.Where(v => slidesIds.Contains(v.SlideId))
			.Select(v => new { v.UserId, v.User.UserName, v.SlideId, v.IsPassed, v.Score, v.AttemptsCount })
			.ToList();

		var r = dq.GroupBy(v => new { v.UserId, v.UserName }).Select(u => new UserInfo
		{
			UserId = u.Key.UserId,
			UserName = u.Key.UserName,
			SlidesSlideInfo = GetSlideInfo(slides, u.Select(arg => Tuple.Create(arg.SlideId, arg.IsPassed, arg.Score, arg.AttemptsCount)))
		});

		return r;
	}

	private static UserSlideInfo[] GetSlideInfo(IEnumerable<Slide> slides, IEnumerable<Tuple<Guid, bool, int, int>> slideResults)
	{
		var results = slideResults.GroupBy(tuple => tuple.Item1).ToDictionary(g => g.Key, g => g.First());
		var defaultValue = Tuple.Create(Guid.Empty, false, 0, 0);
		return slides
			.Select(slide => new
			{
				slide,
				result = results.GetOrDefault(slide.Id, defaultValue)
			})
			.Select(r => new UserSlideInfo
			{
				AttemptsCount = r.result.Item4,
				IsExerciseSolved = r.result.Item2,
				IsQuizPassed = r.result.Item2,
				QuizPercentage = r.slide is QuizSlide ? (double)r.result.Item3 / r.slide.MaxScore : 0.0,
				IsVisited = results.ContainsKey(r.slide.Id)
			})
			.ToArray();
	}

	[Authorize(Policy = UlearnAuthorizationBuilder.InstructorsPolicyName)] //[ULearnAuthorize(MinAccessLevel = CourseRoleType.Instructor)]
	public async Task<ActionResult> UserSolutions(string courseId, string userId, Guid slideId, int? version = null)
	{
		var user = await db.Users.FindAsync(userId);
		if (user == null || user.IsDeleted)
			return new NotFoundResult();

		var course = courseStorage.GetCourse(courseId);
		var visibleUnits = await unitsRepo.GetVisibleUnitIds(course, User.GetUserId());
		var isInstructor = User.HasAccessFor(courseId, CourseRoleType.Instructor);
		var slide = course.FindSlideById(slideId, isInstructor, visibleUnits) as ExerciseSlide;
		if (slide == null)
			return RedirectToAction("CourseInfo", "Account", new { userId, courseId });

		var model = new UserSolutionsViewModel
		{
			User = user,
			Course = course,
			GroupsNames = await groupMembersRepo.GetUserGroupsNamesAsString(course.Id, userId),
			Slide = slide,
			SubmissionId = version
		};
		return View(model);
	}
}

public class StatisticsParams
{
	public string CourseId { get; set; }

	public string PeriodStart { get; set; }
	public string PeriodFinish { get; set; }

	private static readonly string[] dateFormats = { "dd.MM.yyyy" };

	public DateTime PeriodStartDate
	{
		get
		{
			var defaultPeriodStart = GetDefaultPeriodStart();
			if (string.IsNullOrEmpty(PeriodStart))
				return defaultPeriodStart;
			if (!DateTime.TryParseExact(PeriodStart, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
				return defaultPeriodStart;
			return result;
		}
	}

	private static DateTime GetDefaultPeriodStart()
	{
		return new DateTime(DateTime.Now.Year - 4, 1, 1);
	}

	public DateTime PeriodFinishDate
	{
		get
		{
			var defaultPeriodFinish = DateTime.Now.Date;
			if (string.IsNullOrEmpty(PeriodFinish))
				return defaultPeriodFinish;
			if (!DateTime.TryParseExact(PeriodFinish, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
				return defaultPeriodFinish;
			return result;
		}
	}
}

public class CourseStatisticsParams : StatisticsParams
{
	/* Course statistics can't be filtered by dates */
	public new DateTime PeriodStartDate => DateTime.MinValue;

	public new DateTime PeriodFinishDate => DateTime.MaxValue.Subtract(TimeSpan.FromDays(2));
}

public class UnitSheetParams : StatisticsParams
{
	public Guid? UnitId { get; set; }
}

public class UserUnitStatisticsPageModel
{
	public Course Course { get; set; }
	public Unit Unit { get; set; }
	public ApplicationUser User { get; set; }
	public List<UserExerciseSubmission> Submissions { get; set; }
	public List<UserExerciseSubmission> ReviewedSubmissions { get; set; }
	public Dictionary<Guid, Slide> Slides { get; set; }
	public Dictionary<Guid, int> Scores { get; set; }
	public Unit PreviousUnit { get; set; }
	public Unit NextUnit { get; set; }
}

public class ExerciseRatingByPointsModel
{
	public Course Course { get; set; }
	public Slide Slide { get; set; }
	public Group SelectedGroup { get; set; }
	public List<Group> AvailableGroups { get; set; }
	public Dictionary<string, ApplicationUser> Users { get; set; }
	public bool HideOtherUsersNames { get; set; }
	public Dictionary<string, (float Points, DateTime Timestamp)> PointsByUser { get; set; }
	public List<string> UsersOrderedByPoints { get; set; }
}

public class UserSolutionsViewModel
{
	public ExerciseSlide Slide { get; set; }
	public ApplicationUser User { get; set; }
	public Course Course { get; set; }
	public string GroupsNames { get; set; }
	public int? SubmissionId { get; set; }
}

public class UserProgressViewModel
{
	public string CourseId;
	public UserInfo[] Users;
	public Dictionary<string, string> GroupsNames;
	public Slide[] Slides;
}

public class ExportUriBuilder
{
	private readonly Uri baseUri;
	private readonly string courseId;

	public ExportUriBuilder(string baseUri, string courseId)
	{
		this.baseUri = new Uri(new Uri(baseUri), $"/course-statistics/export/");
		this.courseId = courseId;
	}

	public string BuildExportJsonUrl() => BuildUri($"{courseId}.json");

	public string BuildExportXmlUrl() => BuildUri($"{courseId}.xml");

	public string BuildExportXlsxUrl() => BuildUri($"{courseId}.xlsx");

	private string BuildUri(string fileNameWithExtenstion)
	{
		var uri = new Uri(baseUri, fileNameWithExtenstion);
		var builder = new UriBuilder(uri) { Query = courseId };
		return builder.Uri.ToString();
	}
}