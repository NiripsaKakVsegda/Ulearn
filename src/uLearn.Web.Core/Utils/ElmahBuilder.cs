using ElmahCore;
using Ulearn.Common.Extensions;
using Ulearn.Core.Telegram;
using uLearn.Web.Core.Extensions;
using Vostok.Logging.Abstractions;
using Web.Api.Configuration;

namespace uLearn.Web.Core.Utils;

public class ElmahBuilder : IConfigBuilder<ElmahOptions>
{
	public void Build(WebConfiguration configuration, ElmahOptions options)
	{
		options.Notifiers.Add(new ElmahTelegram());
		options.OnPermissionCheck = context =>
		{
			var isAuthenticated = context.User.Identity.IsAuthenticated;
			var isSysAdmin = context.User.IsSystemAdministrator();

			if (!isSysAdmin && !isAuthenticated)
				context.Response.Redirect($"/login?returnUrl={context.Request.Path}");

			if (isAuthenticated && !isSysAdmin)
				context.Response.Redirect("/");

			return true;
		};
	}
}

public class ElmahTelegram : IErrorNotifierWithId
{
	private static ILog log => LogProvider.Get().ForContext(typeof(ElmahTelegram));
	
	private ErrorsBot errorsBot;

	public string Name { get; }

	public ElmahTelegram()
	{
		Name = "ElmahTelegram";
		errorsBot = new ErrorsBot();
	}

	public void Notify(string id, Error error)
	{
		log.Error(error.Exception, $"Произошла ошибка {id} (код {error.StatusCode}, подробности в Elmah):\n" +
									$"Query string: {error.QueryString.ToQueryString()}"
		);

		if (!IsErrorIgnoredForTelegramChannel(error))
			errorsBot.PostToChannel(id, error.Exception);
	}

	public void Notify(Error error)
	{
	}

	private static readonly List<string> ignorableForTelegramChannelSubstrings = new()
	{
		"The provided anti-forgery token was meant for user",
		"The required anti-forgery cookie \"__RequestVerificationToken\" is not present.",
		"A potentially dangerous Request.Path value was detected from the client"
	};

	private static bool IsErrorIgnoredForTelegramChannel(Error error)
	{
		var message = error.Exception.Message;
		return ignorableForTelegramChannelSubstrings.Any(ignorableSubstring => message.Contains(ignorableSubstring));
	}
}