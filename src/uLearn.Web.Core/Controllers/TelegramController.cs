using Database.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;
using Vostok.Logging.Abstractions;
using Web.Api.Configuration;

namespace uLearn.Web.Core.Controllers;

[AllowAnonymous]
public class TelegramController : JsonDataContractController
{
	private static ILog log => LogProvider.Get().ForContext(typeof(TelegramController));

	private readonly string webhookSecret;
	private readonly string protocol;
	private static TelegramBotClient telegramBot;

	private readonly INotificationsRepo notificationsRepo;

	public TelegramController(INotificationsRepo notificationsRepo, WebConfiguration configuration)
	{
		this.notificationsRepo = notificationsRepo;

		webhookSecret = configuration.OldWebConfig["ulearn.telegram.webhook.secret"] ?? "";
		var botToken = ApplicationConfiguration.Read<UlearnConfiguration>().Telegram.BotToken;
		if (botToken != null)
			telegramBot = new TelegramBotClient(botToken);

		protocol = Convert.ToBoolean(configuration.OldWebConfig["ulearn.requireHttps"] ?? "true") ? "https" : "http";
	}

	[HttpPost]
	public async Task<ActionResult> Webhook(string secret)
	{
		if (secret != webhookSecret)
		{
			log.Warn($"Пришёл запрос с неправильным секретом: «{secret}» вместо «{webhookSecret.MaskAsSecret()}»");
			return Forbid();
		}

		var update = await GetJsonObjectFromRequestBody<Update>();
		if (update == null)
			return BadRequest();

		// Fix for bug: Telegram.Bot.Client doesn't know about some new messages types
		if (update.Message == null)
			return Ok();

		switch (update.Type)
		{
			case UpdateType.Message:
				await OnMessage(update.Message);
				break;
		}

		return Ok();
	}

	private async Task<T> GetJsonObjectFromRequestBody<T>()
	{
		Request.Body.Position = 0;
		using var reader = new StreamReader(Request.Body);
		return (await reader.ReadToEndAsync()).DeserializeJson<T>();
	}

	public async Task OnMessage(Message message)
	{
		if (telegramBot == null)
			return;

		var text = message.Text;
		var chatId = message.Chat.Id;

		var reply = "";
		if (text == "/start" || text == "/connect")
		{
			var chatTitle = GetChatTitle(message.Chat);
			var secretHash = notificationsRepo.GetSecretHashForTelegramTransport(chatId, chatTitle, webhookSecret);
			var createTransportUrl = Url.Action("AddTelegram", "Account", new { chatId = chatId, chatTitle = chatTitle, hash = secretHash }, "https");

			reply = $"Чтобы получать уведомления от ulearn.me в телеграм, перейдите по ссылке {createTransportUrl}";
			if (text == "/start")
				reply = "Добро пожаловать к боту ulearn.me!\n\n" + reply;
		}
		else if (text == "/help")
		{
			reply = "Наш бот поддерживает следующие команды:\n\n" +
					"/connect — подключает получение уведомлений от ulearn.me в телеграм\n" +
					"/help — показывает это сообщение\n";
		}
		else
		{
			reply = "Не понимаю такой команды. Нажмите /help, чтобы узнать, что я могу.";
		}

		if (!string.IsNullOrEmpty(reply))
		{
			try
			{
				await telegramBot.SendTextMessageAsync(chatId, reply);
			}
			catch (ApiRequestException e) when (e.Message.Contains("bot was blocked by the user"))
			{
				log.Warn(e);
			}
		}
	}

	private static string GetChatTitle(Chat chat)
	{
		var chatTitle = "@" + chat.Username;
		if (string.IsNullOrEmpty(chatTitle))
			chatTitle = $"{chat.FirstName} {chat.LastName}";
		if (string.IsNullOrEmpty(chatTitle))
			chatTitle = chat.Title;
		return chatTitle;
	}
}