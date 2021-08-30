using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;
using Kontur.Spam.Client;
using Microsoft.Extensions.Options;
using Vostok.Logging.Abstractions;
using Ulearn.Common.Extensions;
using Ulearn.Core.Metrics;

namespace Notifications
{
	public interface IEmailSender
	{
		Task SendEmailAsync(string to, string subject, string textContent = null, string htmlContent = null, EmailButton button = null, string textContentAfterButton = null, string htmlContentAfterButton = null);
	}

	public class KonturSpamEmailSender : IEmailSender
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(KonturSpamEmailSender));

		private readonly MetricSender metricSender;

		private readonly ISpamClient client;
		private readonly string channelId;
		private readonly string templateId;

		public KonturSpamEmailSender(MetricSender metricSender, IOptions<NotificationsConfiguration> options)
		{
			this.metricSender = metricSender;

			var config = options.Value;
			var spamEndpoint = config.Spam.Endpoint ?? "";
			var spamLogin = config.Spam.Login ?? "ulearn";
			var spamPassword = config.Spam.Password ?? "";
			channelId = config.Spam.Channels.Notifications ?? "";
			templateId = config.Spam.Templates.WithButton;

			try
			{
				client = new SpamClient(new Uri(spamEndpoint), spamLogin, spamPassword);
			}
			catch (Exception e)
			{
				log.Error(e, $"Can\'t initialize Spam.API client to {spamEndpoint}, login {spamLogin}, password {spamPassword.MaskAsSecret()}");
				throw;
			}

			log.Info($"Initialized Spam.API client to {spamEndpoint}, login {spamLogin}, password {spamPassword.MaskAsSecret()}");
			log.Info($"Using channel '{channelId}'");
		}

		public async Task SendEmailAsync(string to, string subject, string textContent = null, string htmlContent = null, EmailButton button = null, string textContentAfterButton = null, string htmlContentAfterButton = null)
		{
			var recipientEmailMetricName = to.ToLower().Replace(".", "_");

			metricSender.SendCount("send_email.try");
			metricSender.SendCount($"send_email.try.to.{recipientEmailMetricName}");

			var messageInfo = new MessageSentInfo
			{
				RecipientAddress = to,
				Subject = subject,
			};
			if (!string.IsNullOrEmpty(templateId))
			{
				messageInfo.TemplateId = templateId;
				messageInfo.Variables = new Dictionary<string, object>
				{
					{ "title", subject },
					{ "content", htmlContent },
					{ "text_content", textContent },
					{ "button", button != null },
					{ "button_link", button?.Link },
					{ "button_text", button?.Text },
					{ "content_after_button", htmlContentAfterButton },
					{ "text_content_after_button", textContentAfterButton },
				};
			}
			else
			{
				messageInfo.Html = htmlContent;
				messageInfo.Text = textContent;
			}

			log.Info($"Try to send message to {to} with subject {subject}, text: {textContent?.Replace("\n", @" \\ ")}");
			try
			{
				await client.SentMessageAsync(channelId, messageInfo);
			}
			catch (Exception e)
			{
				log.Warn(e, $"Can\'t send message via Spam.API to {to} with subject {subject}");
				metricSender.SendCount("send_email.fail");
				metricSender.SendCount($"send_email.fail.to.{recipientEmailMetricName}");
				throw;
			}

			metricSender.SendCount("send_email.success");
			metricSender.SendCount($"send_email.success.to.{recipientEmailMetricName}");
		}
	}

	public class EmailButton
	{
		public EmailButton(string link, string text)
		{
			Link = link;
			Link = Link.AddQueryParameter("utm_source", "email");

			Text = text;
		}

		public EmailButton(NotificationButton notificationButton)
			: this(notificationButton.Link, notificationButton.Text)
		{
		}

		public string Link { get; private set; }

		public string Text { get; private set; }
	}
}