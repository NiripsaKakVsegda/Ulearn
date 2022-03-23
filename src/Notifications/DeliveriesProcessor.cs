using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos;
using Ulearn.Core.Courses.Manager;
using Vostok.Logging.Abstractions;

namespace Notifications
{
	public class DeliveriesProcessor
	{
		private readonly INotificationsRepo notificationsRepo;
		private readonly INotificationSender notificationSender;
		private readonly ICourseStorage courseStorage;

		private ILog log => LogProvider.Get().ForContext(typeof(NotificationsApplication));

		public DeliveriesProcessor(INotificationsRepo notificationsRepo, INotificationSender notificationSender,
			ICourseStorage courseStorage)
		{
			this.notificationsRepo = notificationsRepo;
			this.notificationSender = notificationSender;
			this.courseStorage = courseStorage;
		}

		public async Task CreateDeliveries()
		{
			await notificationsRepo.CreateDeliveries();
		}

		public async Task SendDeliveries()
		{
			var deliveries = await notificationsRepo.GetDeliveriesForSendingNow();

			var deliveryGroups = deliveries.GroupBy(d => Tuple.Create(d.NotificationTransportId, d.Notification.GetNotificationType()));
			foreach (var deliveryKvp in deliveryGroups)
			{
				var deliveriesForTransport = deliveryKvp.ToList();
				if (deliveriesForTransport.Count <= 0)
					continue;

				await SendDeliveriesOfOneType(deliveriesForTransport);
			}
		}

		private async Task SendDeliveriesOfOneType(List<NotificationDelivery> deliveries)
		{
			if (deliveries.Count == 0)
				return;

			if (deliveries.Count == 1)
			{
				var delivery = deliveries[0];
				var course = courseStorage.FindCourse(delivery.Notification.CourseId);
				if (course == null)
				{
					log.Warn($"Can't find course {delivery.Notification.CourseId}");
					await notificationsRepo.MarkDeliveryAsFailed(delivery);
					return;
				}

				try
				{
					await notificationSender.Send(delivery);
					await notificationsRepo.MarkDeliveryAsSent(delivery.Id);
				}
				catch (Exception e)
				{
					log.Warn(e, $"Can\'t send notification {delivery.NotificationId} to {delivery.NotificationTransport}. Will try later");
					await notificationsRepo.MarkDeliveryAsFailed(delivery);
				}
			}
			else
			{
				try
				{
					await notificationSender.Send(deliveries);
					await notificationsRepo.MarkDeliveriesAsSent(deliveries.Select(d => d.Id).ToList());
				}
				catch (Exception e)
				{
					log.Warn(e, $"Can\'t send multiple notifications [{string.Join(", ", deliveries.Select(d => d.NotificationId))}] to {deliveries[0].NotificationTransport}. Will try later");
					await notificationsRepo.MarkDeliveriesAsFailed(deliveries);
				}
			}
		}
	}
}