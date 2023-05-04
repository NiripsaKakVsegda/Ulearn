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

		options.OnError += (context, msg) =>
		{
			var password = msg.Form["password"];
			if (password != null)
				msg.Form.Set("password", "***hidden***");
			
			var confirmPassword = msg.Form["confirmPassword"];
			if (confirmPassword != null)
				msg.Form.Set("confirmPassword", "***hidden***");

			var token = msg.Form["__RequestVerificationToken"];
			if (token != null)
				msg.Form.Set("__RequestVerificationToken", "***hidden***");

			var cookie = msg.Cookies["ulearn.auth"];
			if (cookie != null)
				msg.Cookies.Set("ulearn.auth", "***hidden***");

			if (msg.ServerVariables["path"]!= null && msg.ServerVariables["path"].StartsWith("/Login"))
				msg.Body = null;

			return Task.CompletedTask;
		};

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
		var message = error.Message ?? error.Exception.Message;
		return ignorableForTelegramChannelSubstrings.Any(ignorableSubstring => message.Contains(ignorableSubstring));
	}
}