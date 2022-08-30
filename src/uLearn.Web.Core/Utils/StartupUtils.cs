using Database;
using Telegram.Bot;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses.Manager;
using Vostok.Logging.Abstractions;
using Web.Api.Configuration;

namespace uLearn.Web.Core.Utils;

public class StartupUtils
{
	private static ILog log => LogProvider.Get().ForContext(typeof(StartupUtils));

	
	public static void InitializeAllUtils(IApplicationBuilder app, WebConfiguration configuration)
	{
		var courseManager = app.ApplicationServices.GetService<IWebCourseManager>();

		InitTelegramBot(configuration);
		InitializeCourses(courseManager);
	}

	public static void InitTelegramBot(WebConfiguration configuration)
	{
		var botToken = ApplicationConfiguration.Read<UlearnConfiguration>().Telegram.BotToken ?? "";
		if (string.IsNullOrEmpty(botToken))
			return;

		var telegramBot = new TelegramBotClient(botToken);
		var webhookSecret = configuration.OldWebConfig["ulearn.telegram.webhook.secret"] ?? "";
		var webhookDomain = configuration.OldWebConfig["ulearn.telegram.webhook.domain"] ?? "";
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

	private static void InitializeCourses(ICourseUpdater courseManager)
	{
		new UpdateCoursesWorker(courseManager).DoInitialCourseLoadAndRunCoursesUpdateInThreads();
	}
}