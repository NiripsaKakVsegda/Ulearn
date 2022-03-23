using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Ulearn.Core.Configuration;

namespace uLearn.Web.Helpers
{
	public class TelegramHelper
	{
		private static TelegramBotClient telegramBot;

		private static TelegramBotClient TelegramBot
		{
			get
			{
				if (telegramBot != null)
					return telegramBot;

				var botToken = ApplicationConfiguration.Read<UlearnConfiguration>().Telegram.BotToken;
				return telegramBot = new TelegramBotClient(botToken);
			}
		}

		public static async Task SendHtmlMessage(long chatId, string message)
		{
			await TelegramBot.SendTextMessageAsync(chatId, message, parseMode: ParseMode.Html);
		}
	}
}