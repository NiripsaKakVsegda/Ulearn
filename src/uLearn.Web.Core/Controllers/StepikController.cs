using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Stepik.Api;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;
using uLearn.Web.Core.Authorization;
using uLearn.Web.Core.Extensions;
using Vostok.Logging.Abstractions;
using Web.Api.Configuration;

namespace uLearn.Web.Core.Controllers;

[Authorize(Policy = UlearnAuthorizationBuilder.CourseAdminsPolicyName)] 
public class StepikController : Controller
{
	private static ILog log => LogProvider.Get().ForContext(typeof(StepikController));

	private readonly IStepikRepo stepikRepo;
	private readonly INotificationsRepo notificationsRepo;
	private readonly IUsersRepo usersRepo;
	private readonly ICourseStorage courseStorage;

	private readonly string stepikClientId;
	private readonly string stepikClientSecret;
	private readonly string defaultXQueueName;

	private readonly string ulearnWebBaseUrl;
	private readonly string ulearnApiBaseUrl;

	public StepikController(ICourseStorage courseStorage, WebConfiguration configuration, IStepikRepo stepikRepo, INotificationsRepo notificationsRepo, IUsersRepo usersRepo)
	{
		this.stepikRepo = stepikRepo;
		this.notificationsRepo = notificationsRepo;
		this.usersRepo = usersRepo;
		this.courseStorage = courseStorage;
		stepikClientId = configuration.OldWebConfig["stepik.clientId"];
		stepikClientSecret = configuration.OldWebConfig["stepik.clientSecret"];
		defaultXQueueName = configuration.OldWebConfig["stepik.defaultXQueueName"];
		ulearnWebBaseUrl = configuration.BaseUrl;
		ulearnApiBaseUrl = configuration.BaseUrlApi;
	}

	public Task<ActionResult> ExportCourse(string courseId)
	{
		return SelectTarget(courseId);
	}

	public ActionResult InitialExportOptions(string courseId, int stepikCourseId)
	{
		var course = courseStorage.GetCourse(courseId);

		return View(new InitialExportOptionsModel
		{
			Course = course,
			StepikCourseId = stepikCourseId,
			DefaultXQueueName = defaultXQueueName,
		});
	}

	public async Task<ActionResult> InitialExport(string courseId, int stepikCourseId, string newLessonsSlidesIds, string xQueueName, string uploadVideo)
	{
		var returnUrl = Url.Action("InitialExportOptions", "Stepik", new { courseId = courseId, stepikCourseId = stepikCourseId });
		/* TODO (andgein): following 4 lines are copy-paste*/
		var oauthAuthorizationUrl = OAuth.GetAuthorizationUrl(stepikClientId, GetStepikOAuthRedirectUri(), OAuth.EncryptState(returnUrl));
		var client = await GetAuthenticatedStepikApiClient();
		if (client == null)
			return Redirect(oauthAuthorizationUrl);

		var exporter = new CourseExporter(stepikClientId, stepikClientSecret, ulearnApiBaseUrl, ulearnWebBaseUrl, client.AccessToken);
		var course = courseStorage.GetCourse(courseId);

		var exportOptions = new CourseInitialExportOptions(stepikCourseId, xQueueName, ConvertStringToGuidList(newLessonsSlidesIds).ToList())
		{
			VideoUploadOptions = (UploadVideoToStepikOption)Enum.Parse(typeof(UploadVideoToStepikOption), uploadVideo)
		};

		var thread = new Thread(async () => await DoInitialExport(exporter, course, exportOptions));
		thread.Start();
		return View("ExportStarted", course);
	}

	public async Task<ActionResult> UpdateOptions(string courseId, int stepikCourseId)
	{
		var course = courseStorage.GetCourse(courseId);
		var slides = course.GetSlidesNotSafe();
		var slidesXmls = slides.ToDictionary(s => s.Id, s => stepikRepo.GetSlideXmlIndicatedChanges(s));

		/* TODO (andgein): following 5 lines are copy-paste*/
		var uri = new Uri(Request.GetDisplayUrl());
		var returnUrl = uri.PathAndQuery;
		var oauthAuthorizationUrl = OAuth.GetAuthorizationUrl(stepikClientId, GetStepikOAuthRedirectUri(), OAuth.EncryptState(returnUrl));
		var client = await GetAuthenticatedStepikApiClient();
		if (client == null)
			return Redirect(oauthAuthorizationUrl);

		var stepikCourse = await client.GetCourse(stepikCourseId);
		var stepikSections = new Dictionary<int, StepikApiSection>();
		// TODO (andgein): download multiple sections in one request
		foreach (var sectionId in stepikCourse.SectionsIds)
			stepikSections[sectionId] = await client.GetSection(sectionId);

		var stepikUnitsIds = stepikSections.SelectMany(kvp => kvp.Value.UnitsIds);
		var stepikUnits = new Dictionary<int, StepikApiUnit>();
		foreach (var unitId in stepikUnitsIds)
			stepikUnits[unitId] = await client.GetUnit(unitId);

		var stepikLessonsIds = stepikUnits.Select(kvp => kvp.Value.LessonId);
		var stepikLessons = new Dictionary<int, StepikApiLesson>();
		foreach (var lessonId in stepikLessonsIds)
			stepikLessons[lessonId] = await client.GetLesson(lessonId);

		var slideStepMaps = stepikRepo.GetStepsExportedFromCourse(courseId, stepikCourseId);

		return View(new UpdateOptionsModel
		{
			Course = course,
			DefaultXQueueName = defaultXQueueName,
			SlidesXmls = slidesXmls,
			StepikCourse = stepikCourse,
			StepikSections = stepikSections,
			StepikUnits = stepikUnits,
			StepikLessons = stepikLessons,
			SlideStepMaps = slideStepMaps,
		});
	}

	public async Task<ActionResult> UpdateCourse(string courseId, int stepikCourseId, string updateSlidesIds, string xQueueName, string uploadVideo)
	{
		var exportSlideAfterKey = "stepik__course-update__export-slide-after__";

		var returnUrl = Url.Action("UpdateOptions", "Stepik", new { courseId = courseId, stepikCourseId = stepikCourseId });
		/* TODO (andgein): following 4 lines are copy-paste*/
		var oauthAuthorizationUrl = OAuth.GetAuthorizationUrl(stepikClientId, GetStepikOAuthRedirectUri(), OAuth.EncryptState(returnUrl));
		var client = await GetAuthenticatedStepikApiClient();
		if (client == null)
			return Redirect(oauthAuthorizationUrl);

		var exporter = new CourseExporter(stepikClientId, stepikClientSecret, ulearnApiBaseUrl, ulearnWebBaseUrl, client.AccessToken);
		var course = courseStorage.GetCourse(courseId);

		var updateSlidesGuids = ConvertStringToGuidList(updateSlidesIds).ToList();
		var slidesUpdateOptions = new List<SlideUpdateOptions>();
		foreach (var slideId in updateSlidesGuids)
		{
			var stepsIdsForSlide = stepikRepo.GetStepsExportedFromSlide(courseId, stepikCourseId, slideId)
				.Select(m => m.StepId)
				.ToList();
			var insertSlideAfterStepId = int.Parse(Request.Form[exportSlideAfterKey + slideId.GetNormalizedGuid()]);
			slidesUpdateOptions.Add(new SlideUpdateOptions(slideId, insertSlideAfterStepId, stepsIdsForSlide));
		}

		var updateOptions = new CourseUpdateOptions(
			stepikCourseId,
			xQueueName,
			slidesUpdateOptions,
			new List<Guid>()
		)
		{
			VideoUploadOptions = (UploadVideoToStepikOption)Enum.Parse(typeof(UploadVideoToStepikOption), uploadVideo)
		};

		var thread = new Thread(async () => await DoUpdateCourse(exporter, course, updateOptions));
		thread.Start();
		return View("UpdateStarted", course);
	}

	private async Task<ActionResult> SelectTarget(string courseId)
	{
		var uri = new Uri(Request.GetDisplayUrl());
		var returnUrl = uri.PathAndQuery;
		var oauthAuthorizationUrl = OAuth.GetAuthorizationUrl(stepikClientId, GetStepikOAuthRedirectUri(), OAuth.EncryptState(returnUrl));
		var client = await GetAuthenticatedStepikApiClient();
		if (client == null)
			return Redirect(oauthAuthorizationUrl);

		var course = courseStorage.GetCourse(courseId);
		var myCourses = await client.GetMyCourses();
		return View("SelectTarget", new SelectTargetModel
		{
			UlearnCourse = course,
			StepikCourses = myCourses.OrderBy(c => c.Id).ToList(),
		});
	}

	private async Task DoExport(CourseExporter exporter, Course course, CourseExportOptions options, bool isInitialExport)
	{
		var process = await stepikRepo.AddExportProcess(course.Id, options.StepikCourseId, User.GetUserId(), isInitialExport);

		CourseExportResults exportResults;
		if (isInitialExport)
			exportResults = await exporter.InitialExportCourse(course, (CourseInitialExportOptions)options);
		else
			exportResults = await exporter.UpdateCourse(course, (CourseUpdateOptions)options);
		process.StepikCourseTitle = exportResults.StepikCourseTitle;

		await UpdateMapInfoAboutExportedSlides(course, exportResults);
		await stepikRepo.MarkExportProcessAsFinished(process, exportResults.Exception == null, (exportResults.Exception?.ToString() ?? "") + exportResults.Log);

		await notificationsRepo.AddNotification(course.Id, new CourseExportedToStepikNotification { ProcessId = process.Id }, await usersRepo.GetUlearnBotUserId());
	}

	private Task DoInitialExport(CourseExporter exporter, Course course, CourseInitialExportOptions exportOptions)
	{
		return DoExport(exporter, course, exportOptions, true);
	}

	private Task DoUpdateCourse(CourseExporter exporter, Course course, CourseUpdateOptions updateOptions)
	{
		return DoExport(exporter, course, updateOptions, false);
	}

	private async Task UpdateMapInfoAboutExportedSlides(Course course, CourseExportResults exportResults)
	{
		foreach (var kvp in exportResults.SlideIdToStepsIdsMap)
		{
			var slideId = kvp.Key;
			var slide = course.FindSlideByIdNotSafe(slideId);
			var stepsIds = kvp.Value;
			await stepikRepo.SetMapInfoAboutExportedSlide(course.Id, exportResults.StepikCourseId, slide, stepsIds);
		}
	}

	private IEnumerable<Guid> ConvertStringToGuidList(string newLessonsSlidesIds)
	{
		var splitted = newLessonsSlidesIds.Split(',');
		foreach (var part in splitted)
		{
			if (Guid.TryParse(part, out var guid))
				yield return guid;
		}
	}

	private async Task<StepikApiClient> GetAuthenticatedStepikApiClient()
	{
		var userId = User.GetUserId();
		var accessToken = stepikRepo.GetAccessToken(userId);

		if (string.IsNullOrEmpty(accessToken))
			return null;

		var client = CreateStepikApiClientFromToken(accessToken);
		if (!await client.IsAuthenticated())
			return null;

		return client;
	}

	private StepikApiClient CreateStepikApiClientFromToken(string accessToken)
	{
		return new StepikApiClient(new StepikApiOptions
		{
			ClientId = stepikClientId,
			ClientSecret = stepikClientSecret,
			AccessToken = accessToken
		});
	}

	private StepikApiClient CreateStepikApiClientFromCode(string authorizationCode)
	{
		return new StepikApiClient(new StepikApiOptions
		{
			ClientId = stepikClientId,
			ClientSecret = stepikClientSecret,
			AuthorizationCode = authorizationCode,
			RedirectUri = GetStepikOAuthRedirectUri()
		});
	}

	private string GetStepikOAuthRedirectUri()
	{
		return Url.Action("Connect", "Stepik", new { }, Request.GetRealScheme());
	}

	public async Task<ActionResult> Connect(string code = null, string state = null, string error = null)
	{
		if (!string.IsNullOrEmpty(error))
			return View("ConnectError", error);

		if (string.IsNullOrWhiteSpace(code) || string.IsNullOrEmpty(state))
			return View("ConnectError", (object)"Неверные параметры: отсутствует `code` или `state`");

		string returnUrl;
		try
		{
			returnUrl = OAuth.DecryptState(state);
		}
		catch (Exception e)
		{
			log.Error(e, $"Can't decode `state`: {e.Message}");
			return View("ConnectError", (object)"Не смог расшифровать state");
		}

		var userId = User.GetUserId();
		var client = CreateStepikApiClientFromCode(code);
		await client.RetrieveAccessTokenFromAuthorizationCode();
		await stepikRepo.SaveAccessToken(userId, client.AccessToken);

		return Redirect(this.FixRedirectUrl(returnUrl));
	}

	public ActionResult Process(string courseId, int processId)
	{
		var process = stepikRepo.FindExportProcess(processId);
		if (process == null)
			return NotFound();

		var userId = User.GetUserId();
		if (process.OwnerId != userId && !User.IsSystemAdministrator())
			return NotFound();

		var course = courseStorage.GetCourse(process.UlearnCourseId);

		return View(new ProcessModel
		{
			Process = process,
			Course = course,
		});
	}
}

public class ProcessModel
{
	public StepikExportProcess Process { get; set; }
	public Course Course { get; set; }
}

public class UpdateOptionsModel
{
	public Course Course { get; set; }
	public Dictionary<Guid, string> SlidesXmls { get; set; }
	public string DefaultXQueueName { get; set; }
	public StepikApiCourse StepikCourse { get; set; }
	public Dictionary<int, StepikApiSection> StepikSections { get; set; }
	public Dictionary<int, StepikApiLesson> StepikLessons { get; set; }
	public Dictionary<int, StepikApiUnit> StepikUnits { get; set; }
	public List<StepikExportSlideAndStepMap> SlideStepMaps { get; set; }
}

public class InitialExportOptionsModel
{
	public Course Course { get; set; }
	public int StepikCourseId { get; set; }
	public string DefaultXQueueName { get; set; }
}

public class SelectTargetModel
{
	public Course UlearnCourse { get; set; }
	public List<StepikApiCourse> StepikCourses { get; set; }
}