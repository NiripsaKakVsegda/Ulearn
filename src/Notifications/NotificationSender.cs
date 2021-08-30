using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Microsoft.Extensions.Options;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Metrics;

namespace Notifications
{
	public interface INotificationSender
	{
		Task Send(NotificationDelivery notificationDelivery);
		Task Send(List<NotificationDelivery> notificationDeliveries);
	}

	public class NotificationSender : INotificationSender
	{
		private readonly MetricSender metricSender;

		private readonly IEmailSender emailSender;
		private readonly ITelegramSender telegramSender;
		private readonly ICourseStorage courseStorage;
		private readonly string baseUrl;
		private readonly string secretForHashes;

		public NotificationSender(ICourseStorage courseStorage, IEmailSender emailSender, ITelegramSender telegramSender, MetricSender metricSender, IOptions<NotificationsConfiguration> options)
		{
			this.emailSender = emailSender;
			this.telegramSender = telegramSender;
			this.courseStorage = courseStorage;
			this.metricSender = metricSender;

			baseUrl = options.Value.BaseUrl ?? "";
			secretForHashes = options.Value.SecretForHashes ?? "";
		}

		private string GetEmailTextSignature()
		{
			return "\n\n—\nВсегда ваши,\nКоманда ulearn.me\n\nВы можете отписаться от получения уведомлений на почту в настройках вашего профиля.";
		}

		private string GetEmailHtmlSignature(int transportId, NotificationType notificationType, string courseId, string courseTitle)
		{
			var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
			var signature = NotificationsRepo.GetNotificationTransportEnablingSignature(transportId, timestamp, secretForHashes);
			var transportUnsubscribeUrl = $"{baseUrl}/Notifications/SaveSettings?courseId={courseId}&transportId={transportId}&notificationType={(int)notificationType}&isEnabled=False&timestamp={timestamp}&signature={signature}";
			return "<p style=\"color: #999; font-size: 12px;\">" +
					$"<a href=\"{transportUnsubscribeUrl}\">Нажмите здесь</a>, если вы не хотите получать такие уведомления от курса «{courseTitle}» на почту.<br/>" +
					$"Если вы вовсе не хотите получать от нас уведомления на почту, выключите их <a href=\"{baseUrl}/Account/Manage\">в профиле</a>." +
					"</p>";
		}

		private string GetEmailHtmlSignature(NotificationDelivery delivery)
		{
			/* Email signature is disabled from 2018.02.11 */
			return "";

			/*
				var courseId = delivery.Notification.CourseId;
				var courseTitle = courseStorage.GetCourse(courseId).Title;
				return GetEmailHtmlSignature(delivery.NotificationTransportId, delivery.Notification.GetNotificationType(), courseId, courseTitle);
			*/
		}

		private async Task Send(MailNotificationTransport transport, NotificationDelivery notificationDelivery)
		{
			if (string.IsNullOrEmpty(transport.User.Email))
				return;

			var notification = notificationDelivery.Notification;
			var course =  courseStorage.GetCourse(notification.CourseId);

			var notificationButton = notification.GetNotificationButton(transport, notificationDelivery, course, baseUrl);
			var htmlMessage = notification.GetHtmlMessageForDelivery(transport, notificationDelivery, course, baseUrl);
			var textMessage = notification.GetTextMessageForDelivery(transport, notificationDelivery, course, baseUrl);
			await emailSender.SendEmailAsync(
				transport.User.Email,
				notification.GetNotificationType().GetDisplayName(),
				textMessage,
				"<p>" + htmlMessage + "</p>",
				button: notificationButton != null ? new EmailButton(notificationButton) : null,
				textContentAfterButton: GetEmailTextSignature(),
				htmlContentAfterButton: GetEmailHtmlSignature(notificationDelivery)
			);
		}

		private async Task Send(MailNotificationTransport transport, List<NotificationDelivery> notificationDeliveries)
		{
			if (string.IsNullOrEmpty(transport.User.Email))
				return;

			if (notificationDeliveries.Count <= 0)
				return;

			var firstDelivery = notificationDeliveries[0];
			var subject = firstDelivery.Notification.GetNotificationType().GetGroupName();

			var htmlBodies = new List<string>();
			var textBodies = new List<string>();
			foreach (var delivery in notificationDeliveries)
			{
				var notification = delivery.Notification;
				var course =  courseStorage.GetCourse(notification.CourseId);

				var htmlMessage = notification.GetHtmlMessageForDelivery(transport, delivery, course, baseUrl);
				var textMessage = notification.GetTextMessageForDelivery(transport, delivery, course, baseUrl);
				var button = notification.GetNotificationButton(transport, delivery, course, baseUrl);
				if (button != null)
				{
					htmlMessage += $"<br/><br/><a href=\"{button.Link.EscapeHtml()}\">{button.Text.EscapeHtml()}</a>";
					textMessage += $"\n\n{button.Text}: {button.Link}";
				}

				htmlBodies.Add(htmlMessage);
				textBodies.Add(textMessage);
			}

			await emailSender.SendEmailAsync(
				transport.User.Email,
				subject,
				string.Join("\n\n", textBodies),
				string.Join("<br/><br/>", htmlBodies),
				textContentAfterButton: GetEmailTextSignature(),
				htmlContentAfterButton: GetEmailHtmlSignature(firstDelivery)
			);
		}

		private async Task Send(TelegramNotificationTransport transport, NotificationDelivery notificationDelivery)
		{
			if (!transport.User.TelegramChatId.HasValue)
				return;

			var notification = notificationDelivery.Notification;
			var course = courseStorage.GetCourse(notification.CourseId);

			var notificationButton = notification.GetNotificationButton(transport, notificationDelivery, course, baseUrl);

			await telegramSender.SendMessageAsync(
				transport.User.TelegramChatId.Value,
				notification.GetHtmlMessageForDelivery(transport, notificationDelivery, course, baseUrl),
				button: notificationButton != null ? new TelegramButton(notificationButton) : null
			);
		}

		private async Task Send(TelegramNotificationTransport transport, List<NotificationDelivery> notificationDeliveries)
		{
			if (!transport.User.TelegramChatId.HasValue)
				return;

			if (notificationDeliveries.Count <= 0)
				return;

			var subject = $"<b>{notificationDeliveries[0].Notification.GetNotificationType().GetGroupName().EscapeHtml()}</b>";

			var htmls = new List<string> { subject };
			foreach (var delivery in notificationDeliveries)
			{
				var notification = delivery.Notification;
				var course = courseStorage.GetCourse(notification.CourseId);

				var htmlMessage = notification.GetHtmlMessageForDelivery(transport, delivery, course, baseUrl);
				var button = notification.GetNotificationButton(transport, delivery, course, baseUrl);
				if (button != null)
					htmlMessage += $"<br/><br/><a href=\"{button.Link.EscapeHtml()}\">{button.Text.EscapeHtml()}</a>";

				htmls.Add(htmlMessage);
			}

			await telegramSender.SendMessageAsync(transport.User.TelegramChatId.Value, string.Join("<br><br>", htmls));
		}

		public async Task Send(NotificationDelivery notificationDelivery)
		{
			var transport = notificationDelivery.NotificationTransport;

			metricSender.SendCount($"send_notification.{notificationDelivery.Notification.GetNotificationType()}");

			if (transport is MailNotificationTransport || transport is TelegramNotificationTransport)
				await Send((dynamic)transport, notificationDelivery);
		}

		public async Task Send(List<NotificationDelivery> notificationDeliveries)
		{
			if (notificationDeliveries.Count <= 0)
				return;

			var transport = notificationDeliveries[0].NotificationTransport;
			var notificationType = notificationDeliveries[0].Notification.GetNotificationType();
			foreach (var delivery in notificationDeliveries)
			{
				if (delivery.NotificationTransportId != transport.Id)
					throw new Exception("NotificationSender.SendAsync(List<NotificationDelivery>): all deliveries should be for one transport");
				if (delivery.Notification.GetNotificationType() != notificationType)
					throw new Exception("NotificationSender.SendAsync(List<NotificationDelivery>): all deliveries should be for one notification type");
			}

			metricSender.SendCount($"send_notification.{notificationType}", notificationDeliveries.Count);

			if (transport is MailNotificationTransport || transport is TelegramNotificationTransport)
				await Send((dynamic)transport, notificationDeliveries);
		}
	}
}