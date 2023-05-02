using System.Net;
using System.Runtime.Serialization;
using Database;
using Database.Models;
using Database.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Telegram.Bot.Types;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Manager;
using uLearn.Web.Core.Authorization;

namespace uLearn.Web.Core.Controllers;

[Authorize(Policy = UlearnAuthorizationConstants.StudentsPolicyName)]
public class FeedController : Controller
{
	private readonly IServiceProvider serviceProvider;
	private readonly ICourseStorage courseStorage;
	private readonly IFeedRepo feedRepo;

	private static FeedNotificationTransport commonFeedNotificationTransport;


	public FeedController(ICourseStorage courseStorage, IFeedRepo feedRepo, IServiceProvider serviceProvider)
	{
		this.courseStorage = courseStorage;
		this.feedRepo = feedRepo;
		this.serviceProvider = serviceProvider;

		if (commonFeedNotificationTransport == null)
			commonFeedNotificationTransport = feedRepo.GetCommonFeedNotificationTransport().Result;
	}

	public override void OnActionExecuting(ActionExecutingContext context)
	{
		base.OnActionExecuting(context);

		var userId = User.GetUserId();
		AddFeedNotificationTransportIfNeeded(userId).Wait(5000);
	}

	private async Task AddFeedNotificationTransportIfNeeded(string userId)
	{
		await feedRepo.AddFeedNotificationTransportIfNeeded(userId).ConfigureAwait(false);
	}

	public async Task<ActionResult> Index()
	{
		var userId = User.GetUserId();

		var feedNotificationsModel = await GetFeedNotificationsModel();
		await feedRepo.UpdateFeedViewTimestamp(userId, commonFeedNotificationTransport.Id, DateTime.Now);
		return View(feedNotificationsModel);
	}

	public async Task<ActionResult> UnreadCount(string lastTimestamp)
	{
		var userId = User.GetUserId();
		DateTime? lastTimestampDateTime = null;
		if (DateTime.TryParse(lastTimestamp, out var dt))
			lastTimestampDateTime = dt;

		var userNotificationTransport = await feedRepo.GetUsersFeedNotificationTransport(userId);
		var unreadCountAndLastTimestamp = await GetUnreadNotificationsCountAndLastTimestamp(userId, userNotificationTransport, lastTimestampDateTime);

		return Json(new UnreadCountModel
		{
			Status = "ok",
			Count = unreadCountAndLastTimestamp.Item1,
			LastTimestamp = unreadCountAndLastTimestamp.Item2,
		}); //, JsonRequestBehavior.AllowGet);
	}

	private async Task<Tuple<int, DateTime?>> GetUnreadNotificationsCountAndLastTimestamp(string userId, FeedNotificationTransport transport, DateTime? from = null)
	{
		var realFrom = from ?? await feedRepo.GetFeedViewTimestamp(userId, transport.Id) ?? DateTime.MinValue;
		var unreadCount = await feedRepo.GetNotificationsCount(userId, realFrom, transport.Id);
		if (unreadCount > 0)
		{
			from = await feedRepo.GetLastDeliveryTimestamp(transport.Id);
		}

		return Tuple.Create(unreadCount, from);
	}

	public async Task<ActionResult> UpdateLastViewTimestamp(int transportId, DateTime timestamp)
	{
		await feedRepo.UpdateFeedViewTimestamp(User.GetUserId(), transportId, timestamp);
		return new OkResult();
	}

	public async Task<ActionResult> NotificationsTopbarPartial(bool isMobile = false)
	{
		var userId = User.GetUserId();
		var userNotificationTransport = await feedRepo.GetUsersFeedNotificationTransport(userId);
		var unreadCountAndLastTimestamp = await GetUnreadNotificationsCountAndLastTimestamp(userId, userNotificationTransport);
		return PartialView(new NotificationsTopbarPartialModel
		{
			UnreadCount = unreadCountAndLastTimestamp.Item1,
			LastViewTimestamp = unreadCountAndLastTimestamp.Item2,
			IsMobile = isMobile,
		});
	}

	public async Task<ActionResult> NotificationsPartial()
	{
		var feedNotificationsModel = await GetFeedNotificationsModel();
		return PartialView(feedNotificationsModel);
	}

	private async Task<FeedNotificationsModel> GetFeedNotificationsModel()
	{
		var userId = User.GetUserId();
		var notificationTransport = await feedRepo.GetUsersFeedNotificationTransport(userId);

		var importantNotifications = new List<Notification>();
		var commentsNotifications = new List<Notification>();
		if (notificationTransport != null)
		{
			importantNotifications = (await feedRepo.GetFeedNotificationDeliveries(userId, notificationTransport)).Select(d => d.Notification).ToList();
			commentsNotifications = (await feedRepo.GetFeedNotificationDeliveries(userId, commonFeedNotificationTransport)).Select(d => d.Notification).ToList();
		}

		importantNotifications = RemoveBlockedNotifications(importantNotifications).ToList();
		commentsNotifications = RemoveBlockedNotifications(commentsNotifications, importantNotifications).ToList();

		importantNotifications = RemoveNotActualNotifications(importantNotifications).ToList();
		commentsNotifications = RemoveNotActualNotifications(commentsNotifications).ToList();

		var importantLastViewTimestamp = await feedRepo.GetFeedViewTimestamp(userId, notificationTransport?.Id ?? -1);
		var commentsLastViewTimestamp = await feedRepo.GetFeedViewTimestamp(userId, commonFeedNotificationTransport.Id);

		if (notificationTransport != null)
			await feedRepo.UpdateFeedViewTimestamp(userId, notificationTransport.Id, DateTime.Now);

		return new FeedNotificationsModel
		{
			ImportantNotifications = importantNotifications,
			ImportantLastViewTimestamp = importantLastViewTimestamp,
			CommentsNotifications = commentsNotifications,
			CommentsLastViewTimestamp = commentsLastViewTimestamp,
			CommentsNotificationsTransportId = commonFeedNotificationTransport.Id,
			CourseStorage = courseStorage,
		};
	}

	private IEnumerable<Notification> RemoveNotActualNotifications(IEnumerable<Notification> notifications)
	{
		return notifications.Where(notification => notification.IsActual());
	}

	private IEnumerable<Notification> RemoveBlockedNotifications(IReadOnlyCollection<Notification> notifications, IReadOnlyCollection<Notification> searchBlockersAlsoIn = null)
	{
		var notificationsIds = notifications.Select(n => n.Id).ToList();
		var searchBlockersAlsoInIds = searchBlockersAlsoIn?.Select(n => n.Id).ToList();
		foreach (var notification in notifications)
		{
			var blockers = notification.GetBlockerNotifications(serviceProvider).Result;
			if (blockers.Select(b => b.Id).Intersect(notificationsIds).Any())
				continue;
			if (searchBlockersAlsoInIds != null && blockers.Select(b => b.Id).Intersect(searchBlockersAlsoInIds).Any())
				continue;
			yield return notification;
		}
	}

	[DataContract]
	public class UnreadCountModel
	{
		[DataMember(Name = "status")]
		public string Status;

		[DataMember(Name = "count")]
		public int Count;

		[DataMember(Name = "last_timestamp")]
		public DateTime? LastTimestamp;
	}
}

public class FeedNotificationsModel
{
	public List<Notification> ImportantNotifications { get; set; }
	public List<Notification> CommentsNotifications { get; set; }

	public DateTime? ImportantLastViewTimestamp { get; set; }
	public DateTime? CommentsLastViewTimestamp { get; set; }

	public ICourseStorage CourseStorage { get; set; }

	public int CommentsNotificationsTransportId { get; set; }
}

public class NotificationsTopbarPartialModel
{
	public int UnreadCount { get; set; }

	public DateTime? LastViewTimestamp { get; set; }

	public bool IsMobile { get; set; }
}