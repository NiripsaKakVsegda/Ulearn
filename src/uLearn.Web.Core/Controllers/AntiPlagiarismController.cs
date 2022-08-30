using System.Collections.Concurrent;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Principal;
using AntiPlagiarism.Api;
using AntiPlagiarism.Api.Models.Parameters;
using AntiPlagiarism.Api.Models.Results;
using Database.Models;
using Database.Repos;
using Database.Repos.Groups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;
using uLearn.Web.Core.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using uLearn.Web.Core.Authorization;

namespace uLearn.Web.Core.Controllers;

[Authorize(Policy = UlearnAuthorizationBuilder.InstructorsPolicyName)] //[ULearnAuthorize(MinAccessLevel = CourseRoleType.Instructor)]
public class AntiPlagiarismController : JsonDataContractController
{
	private readonly IUserSolutionsRepo userSolutionsRepo;
	private readonly IGroupsRepo groupsRepo;
	private readonly IGroupAccessesRepo groupAccessesRepo;
	private readonly ICourseStorage courseStorage;
	private static readonly IAntiPlagiarismClient antiPlagiarismClient;

	static AntiPlagiarismController()
	{
		var antiplagiarismClientConfiguration = ApplicationConfiguration.Read<UlearnConfiguration>().AntiplagiarismClient;
		antiPlagiarismClient = new AntiPlagiarismClient(antiplagiarismClientConfiguration.Endpoint, antiplagiarismClientConfiguration.Token);
	}

	public AntiPlagiarismController(IUserSolutionsRepo userSolutionsRepo, IGroupsRepo groupsRepo, ICourseStorage courseStorage, IGroupAccessesRepo groupAccessesRepo)
	{
		this.userSolutionsRepo = userSolutionsRepo;
		this.courseStorage = courseStorage;
		this.groupAccessesRepo = groupAccessesRepo;
		this.groupsRepo = groupsRepo;
	}

	public async Task<ActionResult> Info(string courseId, int submissionId)
	{
		var submission = await userSolutionsRepo.FindSubmissionById(submissionId);
		if (!string.Equals(submission.CourseId, courseId, StringComparison.InvariantCultureIgnoreCase))
			return new NotFoundResult();

		var slide = courseStorage.FindCourse(courseId)?.FindSlideByIdNotSafe(submission.SlideId) as ExerciseSlide;
		if (slide == null)
			return new NotFoundResult();

		if (!slide.Exercise.CheckForPlagiarism)
			return Json(new AntiPlagiarismInfoModel
			{
				Status = "not_checked",
			}); //, JsonRequestBehavior.AllowGet);

		var antiPlagiarismsResult = await GetAuthorPlagiarismsAsync(submission).ConfigureAwait(false);

		var model = new AntiPlagiarismInfoModel
		{
			Status = "checked",
			SuspicionLevel = SuspicionLevel.None,
			SuspiciousAuthorsCount = 0,
		};
		var faintSuspicionAuthorsIds = new HashSet<Guid>();
		var strongSuspicionAuthorsIds = new HashSet<Guid>();
		foreach (var researchedSubmission in antiPlagiarismsResult.ResearchedSubmissions)
		{
			foreach (var plagiarism in researchedSubmission.Plagiarisms)
			{
				if (plagiarism.Weight >= antiPlagiarismsResult.SuspicionLevels.StrongSuspicion)
				{
					strongSuspicionAuthorsIds.Add(plagiarism.SubmissionInfo.AuthorId);
					model.SuspicionLevel = SuspicionLevel.Strong;
				}
				else if (plagiarism.Weight >= antiPlagiarismsResult.SuspicionLevels.FaintSuspicion && model.SuspicionLevel != SuspicionLevel.Strong)
				{
					faintSuspicionAuthorsIds.Add(plagiarism.SubmissionInfo.AuthorId);
					model.SuspicionLevel = SuspicionLevel.Faint;
				}
			}
		}

		model.SuspiciousAuthorsCount = model.SuspicionLevel == SuspicionLevel.Faint ? faintSuspicionAuthorsIds.Count : strongSuspicionAuthorsIds.Count;

		return Json(model); //, JsonRequestBehavior.AllowGet);
	}

	public async Task<ActionResult> Details(string courseId, int submissionId)
	{
		var submission = await userSolutionsRepo.FindSubmissionById(submissionId);
		if (!submission.CourseId.EqualsIgnoreCase(courseId))
			return new NotFoundResult();

		var antiPlagiarismsResult = await GetAuthorPlagiarismsAsync(submission);
		var mostSimilarSubmissionsHistogramData = await GetMostSimilarSubmissionsHistogramData(submission.SlideId, submission.Language, 100);
		//var mostSimilarSubmissionsHistogramData = GetMostSimilarSubmissionsHistogramDataMock(100);
		var suspicionLevelsResponse = await antiPlagiarismClient.GetSuspicionLevelsAsync(new GetSuspicionLevelsParameters { TaskId = submission.SlideId, Language = submission.Language });

		var antiPlagiarismSubmissionInfos = antiPlagiarismsResult.ResearchedSubmissions.Select(s => s.SubmissionInfo);
		var plagiarismsSubmissionInfos = antiPlagiarismsResult.ResearchedSubmissions
			.SelectMany(s => s.Plagiarisms).Select(p => p.SubmissionInfo);

		var submissionsIds = antiPlagiarismSubmissionInfos.Concat(plagiarismsSubmissionInfos)
			.Select(si => si.ClientSubmissionId)
			.Distinct()
			.Select(int.Parse)
			.ToList();

		var submissions = (await userSolutionsRepo.FindSubmissionsByIds(submissionsIds))
			.ToDictionary(s => s.Id);
		submissions[submissionId] = submission;

		var userIds = new HashSet<string>(antiPlagiarismsResult.ResearchedSubmissions.SelectMany(s => s.Plagiarisms).Select(s => s.SubmissionInfo.AuthorId.ToString()));
		userIds.Add(submission.UserId);
		/* Use special MockUserCanSeeAllGroups() instead of User because we want to show all users groups, not only available */
		var usersGroups = (await groupAccessesRepo.GetUsersGroupsNamesAsStrings(courseId, userIds, User.GetUserId(), actual: true, archived: false)).ToDefaultDictionary(); //new MockUserCanSeeAllGroups()
		var usersArchivedGroups = (await groupAccessesRepo.GetUsersGroupsNamesAsStrings(courseId, userIds, User.GetUserId(), actual: false, archived: true)).ToDefaultDictionary(); //new MockUserCanSeeAllGroups()
		var isCourseOrSysAdmin = User.HasAccessFor(courseId, CourseRoleType.CourseAdmin);

		var course = courseStorage.FindCourse(courseId);
		var slide = course?.FindSlideByIdNotSafe(submission.SlideId);
		var details = new AntiPlagiarismDetailsModel
		{
			Course = course,
			Slide = slide,
			SubmissionId = submissionId,
			Language = submission.Language,
			Submissions = submissions,
			UsersGroups = usersGroups,
			UsersArchivedGroups = usersArchivedGroups,
			AntiPlagiarismResponse = antiPlagiarismsResult,
			MostSimilarSubmissionsHistogramData = mostSimilarSubmissionsHistogramData,
			CanEditSuspicionLevels = isCourseOrSysAdmin,
			SuspicionLevels = suspicionLevelsResponse.SuspicionLevels,
			MaxAuthorSubmissionWeight = antiPlagiarismsResult.ResearchedSubmissions.Select(s => s.Plagiarisms.Select(p => p.Weight).DefaultIfEmpty(0).Max()).Max()
		};
		return View(details);
	}

	public async Task<ActionResult> SubmissionsPanel(int submissionId, Dictionary<int, double> submissionWeights)
	{
		var submission = await userSolutionsRepo.FindSubmissionById(submissionId);
		if (submission == null)
			return new NotFoundResult();

		var courseId = submission.CourseId;
		var slideId = submission.SlideId;
		var userId = submission.UserId;
		var slide = courseStorage.FindCourse(courseId)?.FindSlideByIdNotSafe(slideId);
		if (slide == null)
			return new NotFoundResult();
		var submissions = userSolutionsRepo.GetAllAcceptedSubmissionsByUser(courseId, slideId, userId).ToList();

		return PartialView("~/Views/Exercise/SubmissionsPanel.cshtml", new ExerciseSubmissionsPanelModel(courseId, slide)
		{
			Submissions = submissions,
			CurrentSubmissionId = submissionId,
			CanTryAgain = false,
			GetSubmissionDescription = s => GetSubmissionDescriptionForPanel(submissionWeights.ContainsKey(s.Id) ? (double?)submissionWeights[s.Id] : null),
			FormUrl = Url.Action("Details", new { courseId }),
			ShowButtons = false,
			SelectControlName = "submissionId",
		});
	}

	private static string GetSubmissionDescriptionForPanel(double? submissionWeight)
	{
		if (!submissionWeight.HasValue)
			return ", не проверялась на списывание";
		var weightPercents = (int)(submissionWeight.Value * 100);
		return $", подозрительность — {weightPercents}%";
	}

	private static readonly ConcurrentDictionary<Tuple<Guid, Guid>, Tuple<DateTime, GetAuthorPlagiarismsResponse>> plagiarismsCache = new ConcurrentDictionary<Tuple<Guid, Guid>, Tuple<DateTime, GetAuthorPlagiarismsResponse>>();
	private static readonly TimeSpan cacheLifeTime = TimeSpan.FromMinutes(10);

	private static async Task<GetAuthorPlagiarismsResponse> GetAuthorPlagiarismsAsync(UserExerciseSubmission submission)
	{
		RemoveOldValuesFromCache();
		var userId = Guid.Parse(submission.UserId);
		var taskId = submission.SlideId;
		var cacheKey = Tuple.Create(userId, taskId);
		if (plagiarismsCache.TryGetValue(cacheKey, out var cachedValue))
		{
			return cachedValue.Item2;
		}

		var value = await antiPlagiarismClient.GetAuthorPlagiarismsAsync(new GetAuthorPlagiarismsParameters
		{
			AuthorId = userId,
			TaskId = taskId,
			Language = submission.Language
		}).ConfigureAwait(false);
		plagiarismsCache.AddOrUpdate(cacheKey, key => Tuple.Create(DateTime.Now, value), (key, old) => Tuple.Create(DateTime.Now, value));
		return value;
	}

	private static async Task<MostSimilarSubmissionsHistogramData> GetMostSimilarSubmissionsHistogramData(Guid taskId, Language language, int binsCount)
	{
		var mostSimilarSubmissions = await antiPlagiarismClient.GetMostSimilarSubmissionsAsync(new GetMostSimilarSubmissionsParameters
		{
			TaskId = taskId,
			Language = language
		});
		var bins = new List<MostSimilarSubmissionsBin>();
		var step = 1.0 / binsCount;
		for (var i = 0; i < binsCount; i += 1)
		{
			var left = step * i;
			var right = left + step;
			if (right > 1 - 0.00001)
				right = 1.00001;
			var count = mostSimilarSubmissions.MostSimilarSubmissions
				.Count(s => s.Weight >= left && s.Weight < right);
			var bin = new MostSimilarSubmissionsBin
			{
				BinRightBorder = (decimal)Math.Round(right, 2),
				SubmissionsCount = count
			};
			bins.Add(bin);
		}

		return new MostSimilarSubmissionsHistogramData
		{
			Bins = bins,
		};
	}

	private static MostSimilarSubmissionsHistogramData GetMostSimilarSubmissionsHistogramDataMock(int binsCount)
	{
		var bins = new List<MostSimilarSubmissionsBin>();
		var step = 1.0 / binsCount;
		var random = new Random(12);
		for (var i = 0; i < binsCount; i += 1)
		{
			var left = step * i;
			var right = left + step;
			if (right > 1 - 0.00001)
				right = 1.00001;
			var bin = new MostSimilarSubmissionsBin
			{
				BinRightBorder = (decimal)Math.Round(right, 2),
				SubmissionsCount = random.Next(0, 10)
			};
			bins.Add(bin);
		}

		return new MostSimilarSubmissionsHistogramData
		{
			Bins = bins,
		};
	}

	private static void RemoveOldValuesFromCache()
	{
		foreach (var key in plagiarismsCache.Keys.ToList())
		{
			if (plagiarismsCache.TryGetValue(key, out var cachedValue))
			{
				/* Remove cached value if it is too old */
				if (DateTime.Now.Subtract(cachedValue.Item1) > cacheLifeTime)
					plagiarismsCache.TryRemove(key, out _);
			}
		}
	}
}

[JsonConverter(typeof(StringEnumConverter), true)]
public enum SuspicionLevel : short
{
	None = 0,
	Faint = 1,
	Strong = 2,
}

[DataContract]
public class AntiPlagiarismInfoModel
{
	[DataMember(Name = "status")]
	public string Status { get; set; }

	[DataMember(Name = "suspicionLevel")]
	public SuspicionLevel SuspicionLevel { get; set; }

	[DataMember(Name = "suspiciousAuthorsCount")]
	public int SuspiciousAuthorsCount { get; set; }
}

public class AntiPlagiarismDetailsModel
{
	public int SubmissionId { get; set; }

	public Language Language { get; set; }

	public Course Course { get; set; }

	public Slide Slide { get; set; }

	public DefaultDictionary<string, string> UsersGroups { get; set; }

	public DefaultDictionary<string, string> UsersArchivedGroups { get; set; }

	public GetAuthorPlagiarismsResponse AntiPlagiarismResponse { get; set; }

	public Dictionary<int, UserExerciseSubmission> Submissions { get; set; }

	public MostSimilarSubmissionsHistogramData MostSimilarSubmissionsHistogramData { get; set; }

	public bool CanEditSuspicionLevels { get; set; }

	public SuspicionLevels SuspicionLevels { get; set; } // SuspicionLevels в AntiPlagiarismResponse кэшируются на несколько минут, а здесь актуальные

	public double MaxAuthorSubmissionWeight { get; set; }
}

public class MostSimilarSubmissionsHistogramData
{
	public List<MostSimilarSubmissionsBin> Bins { get; set; }
}

public class MostSimilarSubmissionsBin
{
	public decimal BinRightBorder { get; set; }
	public int SubmissionsCount { get; set; }
}

public class MockUserCanSeeAllGroups : ClaimsPrincipal
{
	public bool IsInRole(string role)
	{
		return true;
	}

	public IIdentity Identity => new ClaimsIdentity();
}