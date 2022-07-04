using System;
using System.Web.Configuration;
using System.Web.Hosting;
using Vostok.Logging.Abstractions;
using Microsoft.Owin;
using Owin;
using Telegram.Bot;
using uLearn.Web;
using Ulearn.Core;
using Ulearn.Core.Configuration;

[assembly: OwinStartup(typeof(Startup))]
namespace uLearn.Web
{
	public partial class Startup
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(Startup));

		public void Configuration(IAppBuilder app)
		{
			/* TODO (andgein): remove this hack */
			Utils.WebApplicationPhysicalPath = HostingEnvironment.ApplicationPhysicalPath;

			ConfigureAuth(app);
			InitTelegramBot();
		}

		public void InitTelegramBot()
		{
			var botToken = ApplicationConfiguration.Read<UlearnConfiguration>().Telegram.BotToken ?? "";
			if (string.IsNullOrEmpty(botToken))
				return;

			var telegramBot = new TelegramBotClient(botToken);
			var webhookSecret = WebConfigurationManager.AppSettings["ulearn.telegram.webhook.secret"] ?? "";
			var webhookDomain = WebConfigurationManager.AppSettings["ulearn.telegram.webhook.domain"] ?? "";
			var webhookUrl = $"https://{webhookDomain}/Telegram/Webhook?secret={webhookSecret}";
			try
			{
				telegramBot.SetWebhookAsync(webhookUrl).Wait();
			}
			catch (Exception ex)
			{
				log.Error(ex);
			}
		}
	}
}