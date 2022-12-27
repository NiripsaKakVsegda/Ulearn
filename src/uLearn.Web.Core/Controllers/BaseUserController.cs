using Database.Models;
using Database.Repos;
using Database.Repos.Users;
using Kontur.Spam.Client;
using Vostok.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Extensions;
using Ulearn.Core.Metrics;
using Web.Api.Configuration;

namespace uLearn.Web.Core.Controllers;

public class BaseUserController : BaseController
{
	private static ILog log => LogProvider.Get().ForContext(typeof(BaseUserController));

	protected UlearnUserManager userManager;
	protected readonly IUsersRepo usersRepo;

	protected readonly MetricSender metricSender;

	protected readonly string secretForHashes;

	protected readonly string spamChannelId;
	protected readonly SpamClient spamClient;
	protected readonly string spamTemplateId;

	public BaseUserController(UlearnUserManager userManager, IUsersRepo usersRepo, WebConfiguration configuration)
	{
		this.userManager = userManager;
		this.usersRepo = usersRepo;
		metricSender = new MetricSender(configuration.GraphiteServiceName);
		secretForHashes = configuration.OldWebConfig["ulearn.secretForHashes"] ?? "";

		var spamEndpoint = configuration.OldWebConfig["ulearn.spam.endpoint"] ?? "";
		var spamLogin = configuration.OldWebConfig["ulearn.spam.login"] ?? "";
		var spamPassword = configuration.OldWebConfig["ulearn.spam.password"] ?? "";

		spamChannelId = configuration.OldWebConfig["ulearn.spam.channels.emailConfirmations"] ?? "";

		spamTemplateId = configuration.OldWebConfig["ulearn.spam.templates.withButton"] ?? ""; //WebConfigurationManager.AppSettings["ulearn.spam.templates.withButton"] ?? "";

		try
		{
			spamClient = new SpamClient(new Uri(spamEndpoint), spamLogin, spamPassword);
		}
		catch (Exception e)
		{
			log.Error(e, $"Can\'t initialize Spam.API client to {spamEndpoint}, login {spamLogin}, password {spamPassword.MaskAsSecret()}");
			throw;
		}
	}

	protected string GetEmailConfirmationSignature(string email, string userId)
	{
		return $"{secretForHashes}email={email}userId={userId}{secretForHashes}".CalculateMd5();
	}

	protected async Task<bool> SendConfirmationEmail(ApplicationUser user)
	{
		metricSender.SendCount("email_confirmation.send_confirmation_email.try");
		var confirmationUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, email = user.Email, signature = GetEmailConfirmationSignature(user.Email, user.Id) }, "https");
		var subject = "Подтверждение адреса";

		var messageInfo = new MessageSentInfo
		{
			RecipientAddress = user.Email,
			RecipientName = user.VisibleName,
			Subject = subject,
			TemplateId = spamTemplateId,
			Variables = new Dictionary<string, object>
			{
				{ "title", subject },
				{ "content", $"<h2>Привет, {user.VisibleName.EscapeHtml()}!</h2><p>Подтверди адрес электронной почты, нажав на кнопку:</p>" },
				{ "text_content", $"Привет, {user.VisibleName}!\nПодтверди адрес электронной почты, нажав на кнопку:" },
				{ "button", true },
				{ "button_link", confirmationUrl },
				{ "button_text", "Подтвердить адрес" },
				{
					"content_after_button",
					"<p>Подтвердив адрес, ты сможешь восстановить доступ к своему аккаунту " +
					"в любой момент, а также получать уведомления об ответах на свои комментарии и других важных событиях</p>" +
					"<p>Мы не подпиcываем ни на какую периодическую рассылку, " +
					"а все уведомления можно выключить в профиле.</p><p>" +
					"Если ссылка для подтверждения почты не работает, просто скопируй адрес " +
					$"и вставь его в адресную строку браузера: <a href=\"{confirmationUrl}\">{confirmationUrl}</a></p>"
				}
			}
		};

		try
		{
			await spamClient.SentMessageAsync(spamChannelId, messageInfo).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			log.Error(e, $"Не могу отправить письмо для подтверждения адреса на {user.Email}");
			return false;
		}

		metricSender.SendCount("email_confirmation.send_confirmation_email.success");

		await usersRepo.UpdateLastConfirmationEmailTime(user).ConfigureAwait(false);
		return true;
	}

	/* User can not set email which are used (and confirmed) by other user already */
	protected async Task<bool> CanUserSetThisEmail(ApplicationUser user, string email)
	{
		var users = await usersRepo.FindUsersByConfirmedEmail(email);
		return users.All(u => u.Id == user.Id);
	}

	protected async Task<bool> CanNewUserSetThisEmail(string email)
	{
		var users = await usersRepo.FindUsersByConfirmedEmail(email);
		return !users.Any();
	}
}