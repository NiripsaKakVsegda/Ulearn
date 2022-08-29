using Database.Models;
using Database.Repos;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Core.Courses.Manager;
using uLearn.Web.Core.Controllers;
using Web.Api.Configuration;

namespace uLearn.Web.Core.ViewComponents;

public class NotificationSettings : ViewComponent
{
	private readonly INotificationsRepo notificationsRepo;
	private readonly ITempCoursesRepo tempCoursesRepo;
	private readonly IVisitsRepo visitsRepo;
	private readonly ICourseStorage courseStorage;
	private readonly UlearnUserManager userManager;
	private readonly string telegramBotName;
	private readonly string secretForHashes;


	public NotificationSettings(INotificationsRepo notificationsRepo, ITempCoursesRepo tempCoursesRepo, IVisitsRepo visitsRepo, ICourseStorage courseStorage, UlearnUserManager userManager, WebConfiguration configuration)
	{
		this.notificationsRepo = notificationsRepo;
		this.tempCoursesRepo = tempCoursesRepo;
		this.visitsRepo = visitsRepo;
		this.courseStorage = courseStorage;
		this.userManager = userManager;
		telegramBotName = configuration.OldWebConfig["ulearn.telegram.botName"];
		secretForHashes = configuration.OldWebConfig["ulearn.secretForHashes"] ?? "";
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var user = await userManager.FindByNameAsync(User.Identity.Name);

		var mailTransport = await notificationsRepo.FindUsersNotificationTransport<MailNotificationTransport>(user.Id, includeDisabled: true);
		var telegramTransport = await notificationsRepo.FindUsersNotificationTransport<TelegramNotificationTransport>(user.Id, includeDisabled: true);

		var allTempCoursesIdsSet = (await tempCoursesRepo.GetAllTempCourses()).Select(c => c.CourseId).ToHashSet(StringComparer.OrdinalIgnoreCase);
		var courseIds = (await visitsRepo.GetUserCourses(user.Id)).Where(c => !allTempCoursesIdsSet.Contains(c) && courseStorage.FindCourse(c) != null).ToList();

		var courseTitles = courseIds.ToDictionary(c => c, c => courseStorage.GetCourse(c).Title);
		var notificationTypesByCourse = courseIds.ToDictionary(c => c, c => notificationsRepo.GetNotificationTypes(user.Id, c).Result);
		var allNotificationTypes = NotificationsRepo.GetAllNotificationTypes();

		var notificationTransportsSettings = courseIds.SelectMany(
			c => notificationsRepo.GetNotificationTransportsSettings(c, user.Id).Result
				.Select(
					kvp => Tuple.Create(Tuple.Create(c, kvp.Key.Item1, kvp.Key.Item2), kvp.Value.IsEnabled)
				)
		).ToDictionary(kvp => kvp.Item1, kvp => kvp.Item2);

		var selectedTransportIdStr = Request.Query["transportId"];
		int.TryParse(selectedTransportIdStr.FirstOrDefault(), out var selectedTransportId);

		var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
		var getEnableLinkSignature = new Func<int, string>(transportId => GetNotificationTransportEnablingSignature(transportId, timestamp));

		return View("Settings", new NotificationSettingsViewModel
		{
			User = user,
			TelegramBotName = telegramBotName,

			MailTransport = mailTransport,
			TelegramTransport = telegramTransport,
			SelectedTransportId = selectedTransportId,

			CourseTitles = courseTitles,
			AllNotificationTypes = allNotificationTypes,
			NotificationTypesByCourse = notificationTypesByCourse,
			NotificationTransportsSettings = notificationTransportsSettings,

			EnableLinkTimestamp = timestamp,
			GetEnableLinkSignature = getEnableLinkSignature,
		});
	}

	public string GetNotificationTransportEnablingSignature(int transportId, long timestamp)
	{
		return NotificationsRepo.GetNotificationTransportEnablingSignature(transportId, timestamp, secretForHashes);
	}
}