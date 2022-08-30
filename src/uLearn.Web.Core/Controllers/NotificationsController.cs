using System.Net;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Manager;
using uLearn.Web.Core.Authorization;
using uLearn.Web.Core.Extensions;
using Web.Api.Configuration;

namespace uLearn.Web.Core.Controllers;

[Authorize(Policy = UlearnAuthorizationBuilder.StudentsPolicyName)]//ULearnAuthorize
public class NotificationsController : Controller
{
	private readonly INotificationsRepo notificationsRepo;
	private readonly IUsersRepo usersRepo;
	private readonly ICourseStorage courseStorage;
	private readonly IVisitsRepo visitsRepo;
	private readonly ITempCoursesRepo tempCoursesRepo;

	private readonly UlearnUserManager userManager;

	private readonly string telegramBotName;
	private readonly string secretForHashes;

	private readonly TimeSpan notificationEnablingLinkExpiration = TimeSpan.FromDays(7);

	public NotificationsController(WebConfiguration configuration, INotificationsRepo notificationsRepo, IUsersRepo usersRepo, ICourseStorage courseStorage, IVisitsRepo visitsRepo, ITempCoursesRepo tempCoursesRepo, UlearnUserManager userManager)
	{
		this.notificationsRepo = notificationsRepo;
		this.usersRepo = usersRepo;
		this.courseStorage = courseStorage;
		this.visitsRepo = visitsRepo;
		this.tempCoursesRepo = tempCoursesRepo;
		this.userManager = userManager;
		telegramBotName = configuration.OldWebConfig["ulearn.telegram.botName"];
		secretForHashes = configuration.OldWebConfig["ulearn.secretForHashes"] ?? "";
	}


	[AllowAnonymous]
	public async Task<ActionResult> SuggestMailTransport()
	{
		if (!User.HasAccess(CourseRoleType.Instructor))
			return new OkResult();

		var userId = User.GetUserId();
		var transport = await notificationsRepo.FindUsersNotificationTransport<MailNotificationTransport>(userId, includeDisabled: true);
		if (transport != null)
			return new OkResult();

		var user = await usersRepo.FindUserById(userId);
		if (user == null)
			return new NotFoundResult();
		if (string.IsNullOrEmpty(user.Email) || !user.EmailConfirmed)
			return new OkResult();

		var mailNotificationTransport = new MailNotificationTransport
		{
			UserId = User.GetUserId(),
			IsEnabled = false,
		};
		AddNotificationTransport(mailNotificationTransport).Wait(5000);

		var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
		var signature = GetNotificationTransportEnablingSignature(mailNotificationTransport.Id, timestamp);

		return PartialView(new SuggestMailTransportViewModel
		{
			Transport = mailNotificationTransport,
			TelegramBotName = telegramBotName,
			LinkTimestamp = timestamp,
			LinkSignature = signature,
		});
	}

	public async Task AddNotificationTransport(MailNotificationTransport transport)
	{
		await notificationsRepo.AddNotificationTransport(transport).ConfigureAwait(false);
	}

	public string GetNotificationTransportEnablingSignature(int transportId, long timestamp)
	{
		return NotificationsRepo.GetNotificationTransportEnablingSignature(transportId, timestamp, secretForHashes);
	}

	private bool ValidateNotificationTransportEnablingSignature(int transportId, long timestamp, string signature)
	{
		var currentTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
		var linkExpirationSeconds = notificationEnablingLinkExpiration.TotalSeconds;
		if (currentTimestamp < timestamp || currentTimestamp > timestamp + linkExpirationSeconds)
			return false;

		var correctSignature = GetNotificationTransportEnablingSignature(transportId, timestamp);
		return signature == correctSignature;
	}

	public async Task<ActionResult> EnableNotificationTransport(int transportId, long timestamp, string signature, bool enable = true, string next = "")
	{
		var transport = await notificationsRepo.FindNotificationTransport(transportId);
		if (!User.Identity.IsAuthenticated || transport.UserId != User.GetUserId())
			return new ForbidResult();

		if (!ValidateNotificationTransportEnablingSignature(transportId, timestamp, signature))
			return RedirectToAction("Manage", "Account");

		await notificationsRepo.EnableNotificationTransport(transportId, enable);

		if (next.IsLocalUrl(Request))
			return Redirect(next);

		return RedirectToAction("Manage", "Account");
	}

	public async Task<ActionResult> CreateMailTransport()
	{
		var mailTransport = new MailNotificationTransport
		{
			UserId = User.GetUserId(),
			IsEnabled = true,
			IsDeleted = false,
		};
		await notificationsRepo.AddNotificationTransport(mailTransport);

		return RedirectToAction("Manage", "Account");
	}

	[HttpGet]
	public async Task<ActionResult> SaveSettings(string courseId, int transportId, int notificationType, bool isEnabled, long timestamp, string signature)
	{
		var userId = User.GetUserId();
		var transport = await notificationsRepo.FindNotificationTransport(transportId);
		if (transport == null || transport.UserId != userId)
		{
			return Forbid();
		}

		// TODO (andgein): Add error message about link expiration
		if (!ValidateNotificationTransportEnablingSignature(transportId, timestamp, signature))
			return RedirectToAction("Manage", "Account");

		NotificationType realNotificationType;
		try
		{
			realNotificationType = (NotificationType)notificationType;
		}
		catch (Exception)
		{
			return new BadRequestResult();
		}

		await notificationsRepo.SetNotificationTransportSettings(courseId, transportId, realNotificationType, isEnabled);

		return RedirectToAction("Manage", "Account");
	}

	[HttpPost]
	public async Task<ActionResult> SaveSettings(string courseId, int transportId, int notificationType, bool isEnabled)
	{
		var userId = User.GetUserId();
		var transport = await notificationsRepo.FindNotificationTransport(transportId);
		if (transport == null || transport.UserId != userId)
		{
			return new ForbidResult();
		}

		NotificationType realNotificationType;
		try
		{
			realNotificationType = (NotificationType)notificationType;
		}
		catch (Exception)
		{
			return new BadRequestResult();
		}

		await notificationsRepo.SetNotificationTransportSettings(courseId, transportId, realNotificationType, isEnabled);

		return Json(new { status = "ok", notificationTransportsSettings = GetNotificationTransportsSettings(userId) });
	}

	private IEnumerable<NotificationTransportsSettingsViewModel> GetNotificationTransportsSettings(string userId)
	{
		var notificationTransportsSettings = courseStorage
			.GetCourses()
			.SelectMany(
				c => notificationsRepo.GetNotificationTransportsSettings(c.Id, userId).Result
					.Select(kvp => new NotificationTransportsSettingsViewModel
						{
							courseId = c.Id,
							transportId = kvp.Key.Item1,
							notificationType = (int)kvp.Key.Item2,
							isEnabled = kvp.Value.IsEnabled
						}
					)
			);
		return notificationTransportsSettings;
	}
}

public class NotificationSettingsViewModel
{
	public ApplicationUser User { get; set; }

	public string TelegramBotName { get; set; }

	public MailNotificationTransport MailTransport { get; set; }

	public TelegramNotificationTransport TelegramTransport { get; set; }

	public int SelectedTransportId { get; set; }

	public Dictionary<string, string> CourseTitles { get; set; }

	public List<NotificationType> AllNotificationTypes { get; set; }

	public Dictionary<string, List<NotificationType>> NotificationTypesByCourse { get; set; }

	// Dictionary<(courseId, notificationTrnasportId, notificationType), isEnabled>
	public Dictionary<Tuple<string, int, NotificationType>, bool> NotificationTransportsSettings { get; set; }

	public long EnableLinkTimestamp { get; set; }

	public Func<int, string> GetEnableLinkSignature { get; set; }
}

public class NotificationTransportsSettingsViewModel
{
	public string courseId;
	public int transportId;
	public int notificationType;
	public bool isEnabled;
}

public class SuggestMailTransportViewModel
{
	public MailNotificationTransport Transport { get; set; }
	public string TelegramBotName { get; set; }
	public long LinkTimestamp { get; set; }
	public string LinkSignature { get; set; }
}