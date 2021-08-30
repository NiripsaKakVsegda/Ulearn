using System;
using System.Configuration;
using System.Threading.Tasks;
using Vostok.Logging.Abstractions;
using Telegram.Bot.Types.Enums;
using Ulearn.Core.Telegram;

namespace GiftsGranter
{
	public class GiftsTelegramBot : TelegramBot
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(GiftsTelegramBot));

		public GiftsTelegramBot()
		{
			channel = ConfigurationManager.AppSettings["ulearn.telegram.gifts.channel"];
		}

		public async Task PostToChannelAsync(string message, ParseMode parseMode = ParseMode.Default)
		{
			if (!IsBotEnabled)
				return;

			log.Info($"Отправляю в телеграм-канал {channel} сообщение:\n{message}");
			try
			{
				await telegramClient.SendTextMessageAsync(channel, message, parseMode: parseMode, disableWebPagePreview: true).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				log.Error(e, $"Не могу отправить сообщение в телеграм-канал {channel}");
			}
		}

		public void PostToChannel(string message, ParseMode parseMode = ParseMode.Default)
		{
			PostToChannelAsync(message, parseMode).Wait();
		}
	}
}