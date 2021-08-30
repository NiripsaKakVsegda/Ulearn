using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Database.Models;
using Database.Repos;
using Database.Repos.Users;
using Vostok.Logging.Abstractions;
using Ulearn.Common.Extensions;
using Ulearn.Core.Extensions;

namespace Notifications
{
	public class OneTimeEmailSender
	{
		private const string configFilename = "sender.xml";

		private readonly IEmailSender emailSender;
		private readonly IUsersRepo usersRepo;
		private readonly INotificationsRepo notificationsRepo;

		private static ILog log => LogProvider.Get().ForContext(typeof(OneTimeEmailSender));

		public OneTimeEmailSender(IEmailSender emailSender, IUsersRepo usersRepo, INotificationsRepo notificationsRepo)
		{
			this.emailSender = emailSender;
			this.usersRepo = usersRepo;
			this.notificationsRepo = notificationsRepo;
		}

		public async Task SendEmails()
		{
			log.Info($"Loading config from {configFilename} (see example in {configFilename}.example)");
			OneTimeEmailSenderConfig config;
			using (var stream = new StreamReader(configFilename))
				config = (OneTimeEmailSenderConfig)new XmlSerializer(typeof(OneTimeEmailSenderConfig)).Deserialize(stream);

			log.Info($"Loaded config from {configFilename}:");
			log.Info(config.XmlSerialize());

			/* Get text from html by stripping HTML tags if text is not defined */
			if (string.IsNullOrEmpty(config.Text))
				config.Text = config.Html.StripHtmlTags();
			config.Text = config.Text.RemoveCommonNesting().Trim();

			var emails = config.Emails.ToImmutableHashSet();
			var email2User = (await usersRepo.FindUsersByConfirmedEmails(emails)).ToDictSafe(u => u.Email, u => u);

			foreach (var email in emails)
			{
				if (!email2User.TryGetValue(email, out var user))
				{
					log.Warn($"User with confirmed email not found for {email}");
					continue;
				}

				var mailTransport = await notificationsRepo.FindUsersNotificationTransport<MailNotificationTransport>(user.Id);
				if (mailTransport == null)
				{
					log.Warn($"Mail transport not enabled for {email}");
					continue;
				}

				var settings = await notificationsRepo.GetNotificationTransportsSettings(config.CourseId, NotificationType.SystemMessage, new List<int> { mailTransport.Id });
				const bool isEnabledByDefault = true;
				var mailSettings = settings[mailTransport.Id];
				if (mailSettings == null && !isEnabledByDefault || mailSettings != null && !settings[mailTransport.Id].IsEnabled)
				{
					log.Warn($"SystemMessage for mail transport for {config.CourseId} not enabled for {email}");
					continue;
				}

				log.Info($"Send email to {email}");
				var button = config.Button == null ? null : new EmailButton(config.Button.Link, config.Button.Text);
				await emailSender.SendEmailAsync(email, config.Subject, config.Text, config.Html, button).ConfigureAwait(false);
			}
		}
	}

	[XmlRoot("sender")]
	public class OneTimeEmailSenderConfig
	{
		[XmlElement("email")]
		public List<string> Emails { get; set; }

		[XmlElement("courseId")]
		public string CourseId { get; set; }

		[XmlElement("subject")]
		public string Subject { get; set; }

		[XmlElement("text")]
		public string Text { get; set; }

		[XmlElement("html")]
		public string Html { get; set; }

		[XmlElement("button")]
		public ButtonConfig Button { get; set; }
	}

	public class ButtonConfig
	{
		[XmlElement("link")]
		public string Link { get; set; }

		[XmlElement("text")]
		public string Text { get; set; }
	}
}