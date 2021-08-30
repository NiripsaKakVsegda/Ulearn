using System;
using System.Threading.Tasks;
using Database.Models;
using Microsoft.Extensions.Options;
using Vostok.Logging.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;
using Ulearn.Core.Metrics;

namespace Notifications
{
	public interface ITelegramSender
	{
		Task SendMessageAsync(long chatId, string html, TelegramButton button = null);
	}

	public class TelegramSender : ITelegramSender
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(TelegramSender));

		private readonly MetricSender metricSender;

		private readonly TelegramBotClient bot;

		public TelegramSender(MetricSender metricSender, IOptions<NotificationsConfiguration> options)
		{
			this.metricSender = metricSender;

			var botToken = options.Value.Telegram.BotToken;
			try
			{
				bot = new TelegramBotClient(botToken);
			}
			catch (Exception e)
			{
				log.Error(e, $"Can\'t initialize telegram bot with token \"{botToken.MaskAsSecret()}\"");
				throw;
			}

			log.Info($"Initialized telegram bot with token \"{botToken.MaskAsSecret()}\"");
		}

		public async Task SendMessageAsync(long chatId, string html, TelegramButton button = null)
		{
			metricSender.SendCount("send_to_telegram.try");
			metricSender.SendCount($"send_to_telegram.try.to.{chatId}");
			html = PrepareHtmlForTelegram(html);
			log.Info($"Try to send message to telegram chat {chatId}, html: {html.Replace("\n", @" \\ ")}" + (button != null ? $", button: {button}" : ""));

			InlineKeyboardMarkup replyMarkup = null;
			if (button != null)
				replyMarkup = new InlineKeyboardMarkup(new[] { InlineKeyboardButton.WithUrl(button.Text, button.Link) });

			try
			{
				await bot.SendTextMessageAsync(chatId, html, parseMode: ParseMode.Html, replyMarkup: replyMarkup);
			}
			catch (Exception e)
			{
				metricSender.SendCount("send_to_telegram.fail");
				metricSender.SendCount($"send_to_telegram.fail.to.{chatId}");

				var message = (e as ApiRequestException)?.Message;
				var isBotBlockedByUser = (message?.Contains("bot was blocked by the user") ?? false) || (message?.Contains("user is deactivated") ?? false);
				if (isBotBlockedByUser)
				{
					metricSender.SendCount("send_to_telegram.fail.blocked_by_user");
					metricSender.SendCount($"send_to_telegram.fail.blocked_by_user.to.{chatId}");
					log.Warn(e, $"Can\'t send message to telegram chat {chatId}");
				}
				else
					log.Error(e, $"Can\'t send message to telegram chat {chatId}");

				throw;
			}

			metricSender.SendCount("send_to_telegram.success");
			metricSender.SendCount($"send_to_telegram.success.to.{chatId}");
		}

		private static string PrepareHtmlForTelegram(string html)
		{
			html = html.Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n");

			/* https://core.telegram.org/bots/api#html-style
			 * All numerical HTML entities are supported.
			 * The API currently supports only the following named HTML entities: &lt;, &gt;, &amp; and &quot;. */
			html = html.Replace("&apos;", "'");

			return html;
		}
	}

	public class TelegramButton
	{
		public TelegramButton(NotificationButton notificationButton)
		{
			Link = notificationButton.Link;

			/* Telegram doesn't support links to localhost so replace domain */
			Link = Link.Replace("localhost", "replace.to.localhost");

			Link = Link.AddQueryParameter("utm_source", "telegram");

			Text = notificationButton.Text;
		}

		public string Link { get; private set; }
		public string Text { get; private set; }

		public override string ToString()
		{
			return $"TelegramButton{{{Text}: <{Link}>}}";
		}
	}
}