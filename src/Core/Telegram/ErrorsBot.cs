using System;
using System.Threading.Tasks;
using Vostok.Logging.Abstractions;
using Telegram.Bot.Types.Enums;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;
using MetricSender = Ulearn.Core.Metrics.MetricSender;

namespace Ulearn.Core.Telegram
{
	public class ErrorsBot : TelegramBot
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(ErrorsBot));
		private readonly MetricSender metricSender;

		public ErrorsBot()
		{
			var configuration = ApplicationConfiguration.Read<UlearnConfiguration>();
			channel = configuration.Telegram?.Errors?.Channel;
			var serviceName = configuration.GraphiteServiceName ?? System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToLower();
			metricSender = new MetricSender(serviceName);
		}

		public ErrorsBot(UlearnConfiguration configuration, MetricSender metricSender)
		{
			channel = configuration.Telegram?.Errors?.Channel;
			this.metricSender = metricSender;
		}

		public async Task PostToChannelAsync(string message, ParseMode parseMode = ParseMode.Default)
		{
			if (!IsBotEnabled)
				return;

			metricSender.SendCount("errors");
			log.Info($"Отправляю в телеграм-канал {channel} сообщение об ошибке:\n{message}");
			if (message.Length > MaxMessageSize)
			{
				log.Info($"Сообщение слишком длинное, отправлю только первые {MaxMessageSize} байтов");
				message = message.Substring(0, MaxMessageSize);
			}

			try
			{
				await telegramClient.SendTextMessageAsync(channel, message, parseMode: parseMode, disableWebPagePreview: true).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				/* Not error because it may cause recursive fails */
				log.Info(e, $"Не могу отправить сообщение в телеграм-канал {channel}");
			}
		}

		public void PostToChannel(string message, ParseMode parseMode = ParseMode.Default)
		{
			PostToChannelAsync(message, parseMode).Wait(5000);
		}

		public void PostToChannel(string errorId, Exception exception)
		{
			if (!IsBotEnabled)
				return;

			var elmahUrl = "https://ulearn.me/elmah/detail?id=" + errorId;

			var text = $"*Произошла ошибка {errorId.EscapeMarkdown()}*\n" +
						$"{exception.Message.EscapeMarkdown()}\n\n" +
						$"Подробности: {elmahUrl.EscapeMarkdown()}";

			PostToChannel(text, ParseMode.Markdown);
		}
	}
}