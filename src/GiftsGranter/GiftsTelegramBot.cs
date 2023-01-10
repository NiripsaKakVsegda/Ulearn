using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
			var configuration =  new ConfigurationBuilder()
				.AddJsonFile("appsettings.json");
            
			var config = configuration.Build();
			channel = config
				.GetSection("ulearn.telegram.gifts.channel")
				.Get<string>(); //ConfigurationManager.AppSettings["ulearn.telegram.gifts.channel"];
		}

		public async Task PostToChannelAsync(string message, ParseMode parseMode = ParseMode.Default)
		{
			if (!IsBotEnabled || channel == null)
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
	}
}