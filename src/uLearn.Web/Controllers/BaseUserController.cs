using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;
using Database.DataContexts;
using Database.Models;
using Kontur.Spam.Client;
using Vostok.Logging.Abstractions;
using Microsoft.AspNet.Identity;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;
using Ulearn.Core.Metrics;

namespace uLearn.Web.Controllers
{
	public class BaseUserController : BaseController
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(BaseUserController));

		protected readonly ULearnDb db;
		protected UserManager<ApplicationUser> userManager;
		protected readonly UsersRepo usersRepo;

		protected readonly MetricSender metricSender;

		protected readonly string secretForHashes;

		protected readonly string spamChannelId;
		protected readonly SpamClient spamClient;
		protected readonly string spamTemplateId;

		protected BaseUserController(ULearnDb db)
		{
			this.db = db;
			userManager = new ULearnUserManager(db);
			usersRepo = new UsersRepo(db);
			metricSender = new MetricSender(ApplicationConfiguration.Read<UlearnConfiguration>().GraphiteServiceName);

			secretForHashes = WebConfigurationManager.AppSettings["ulearn.secretForHashes"] ?? "";

			var spamEndpoint = WebConfigurationManager.AppSettings["ulearn.spam.endpoint"] ?? "";
			var spamLogin = WebConfigurationManager.AppSettings["ulearn.spam.login"] ?? "ulearn";
			var spamPassword = WebConfigurationManager.AppSettings["ulearn.spam.password"] ?? "";
			spamChannelId = WebConfigurationManager.AppSettings["ulearn.spam.channels.emailConfirmations"] ?? "";
			spamTemplateId = WebConfigurationManager.AppSettings["ulearn.spam.templates.withButton"] ?? "";

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

		protected BaseUserController()
			: this(new ULearnDb())
		{
		}

		protected string GetEmailConfirmationSignature(string email, string userId)
		{
			return $"{secretForHashes}email={email}userId={userId}{secretForHashes}".CalculateMd5();
		}

		protected async Task<bool> SendConfirmationEmail(ApplicationUser user)
		{
			metricSender.SendCount("email_confirmation.send_confirmation_email.try");
			var confirmationUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, email = user.Email, signature = GetEmailConfirmationSignature(user.Email,user.Id) }, "https");
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
		protected bool CanUserSetThisEmail(ApplicationUser user, string email)
		{
			var users = usersRepo.FindUsersByConfirmedEmail(email);
			return users.All(u => u.Id == user.Id);
		}

		protected bool CanNewUserSetThisEmail(string email)
		{
			var users = usersRepo.FindUsersByConfirmedEmail(email);
			return !users.Any();
		}
	}
}