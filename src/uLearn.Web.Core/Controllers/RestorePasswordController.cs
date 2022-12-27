using Database;
using Database.Models;
using Database.Repos;
using Kontur.Spam.Client;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;
using Ulearn.Core.Metrics;
using uLearn.Web.Core.Models;
using Vostok.Logging.Abstractions;
using Web.Api.Configuration;
using Message = uLearn.Web.Core.Models.Message;

namespace uLearn.Web.Core.Controllers;

public class RestorePasswordController : Controller
{
	private static ILog log => LogProvider.Get().ForContext(typeof(RestorePasswordController));
	private readonly IRestoreRequestRepo requestRepo;
	private readonly UlearnUserManager userManager;
	private readonly UlearnDb db;
	private readonly MetricSender metricSender;
	private readonly AuthenticationManager authenticationManager;

	private readonly string spamChannelId;
	private readonly SpamClient spamClient;

	public RestorePasswordController(UlearnDb db, WebConfiguration configuration, IRestoreRequestRepo requestRepo, UlearnUserManager userManager, AuthenticationManager authenticationManager)
	{
		this.db = db;
		this.requestRepo = requestRepo;
		this.userManager = userManager;
		this.authenticationManager = authenticationManager;
		metricSender = new MetricSender(ApplicationConfiguration.Read<UlearnConfiguration>().GraphiteServiceName);

		var spamEndpoint = configuration.OldWebConfig["ulearn.spam.endpoint"] ?? "";
		var spamLogin = configuration.OldWebConfig["ulearn.spam.login"] ?? "ulearn";
		var spamPassword = configuration.OldWebConfig["ulearn.spam.password"] ?? "";
		spamChannelId = configuration.OldWebConfig["ulearn.spam.channels.passwords"] ?? "";

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

	public ActionResult Index()
	{
		return View(new RestorePasswordModel());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public async Task<ActionResult> Index(string username)
	{
		if (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(username))
		{
			return View(new RestorePasswordModel
			{
				Messages = new List<Message>
				{
					new($"Пользователь \"{username}\" не найден")
				}
			});
		}

		metricSender.SendCount("restore_password.try");
		var users = await FindUsers(username);
		var answer = new RestorePasswordModel
		{
			UserName = username
		};

		if (!users.Any())
		{
			answer.Messages.Add(new Message($"Пользователь {username} не найден"));
			return View(answer);
		}

		metricSender.SendCount("restore_password.found_users");

		foreach (var user in users)
		{
			if (string.IsNullOrWhiteSpace(user.Email))
			{
				answer.Messages.Add(new Message($"У пользователя {user.UserName} не указана электронная почта"));
				continue;
			}

			var requestId = await requestRepo.CreateRequest(user.Id);

			if (requestId == null)
			{
				answer.Messages.Add(new Message($"Слишком частые запросы для пользователя {user.UserName}. Попробуйте ещё раз через несколько минут"));
				continue;
			}

			try
			{
				await SendRestorePasswordEmail(requestId, user);
			}
			catch (Exception _)
			{
				answer.Messages.Add(user.EmailConfirmed
					? new Message($"При отправке письма произошла ошибка. Обратитесь в службу поддержки support@ulearn.me") 
					: new Message($"При отправке письма произошла ошибка. Ваша почта {user.Email} - не подтверждена, возможно вы ошиблись в её написании. Обратитесь на support@ulearn.me, укажите ваше ФИО и почту в письме."));

				return View(answer);
			}

			answer.Messages.Add(new Message($"Письмо с инструкцией по восстановлению пароля для пользователя {user.UserName} отправлено вам на почту", false));
		}

		return View(answer);
	}

	private async Task<List<ApplicationUser>> FindUsers(string info)
	{
		var user = await userManager.FindByNameAsync(info);
		if (user != null)
			return new List<ApplicationUser> { user };
		return await db.Users.Where(u => u.Email == info && !u.IsDeleted).ToListAsync();
	}

	private async Task SendRestorePasswordEmail(string requestId, ApplicationUser user)
	{
		var url = Url.Action("SetNewPassword", "RestorePassword", new { requestId }, "https");

		var subject = "Восстановление пароля от ulearn.me";
		var textBody = "Чтобы изменить пароль к аккаунту " + user.UserName + ", перейдите по ссылке: " + url + ".";
		var htmlBody = "Чтобы изменить пароль к аккаунту " + user.UserName.EscapeHtml() + ", перейдите по ссылке: <a href=\"" + url + "\">" + url + "</a>.";
		var messageInfo = new MessageSentInfo
		{
			RecipientAddress = user.Email,
			Subject = subject,
			Text = textBody,
			Html = htmlBody
		};

		log.Info($"Пытаюсь отправить емэйл на {user.Email} с темой «{subject}», text: {textBody.Replace("\n", @" \\ ")}");
		try
		{
			await spamClient.SentMessageAsync(spamChannelId, messageInfo);
		}
		catch (Exception e)
		{
			log.Error(e, $"Не смог отправить емэйл через Spam.API на {user.Email} с темой «{subject}»");
			throw;
		}

		metricSender.SendCount("restore_password.send_email");
	}

	public ActionResult SetNewPassword(string requestId)
	{
		if (!requestRepo.ContainsRequest(requestId))
			return View(new SetNewPasswordModel
			{
				Errors = new[] { "Запрос не найден" }
			});

		return View(new SetNewPasswordModel
		{
			RequestId = requestId,
			Errors = new string[0]
		});
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[HandleHttpAntiForgeryException]
	public async Task<ActionResult> SetNewPassword(SetNewPasswordModel model)
	{
		var answer = new SetNewPasswordModel
		{
			RequestId = model.RequestId
		};
		if (!ModelState.IsValid)
		{
			answer.Errors = ModelState.Values.SelectMany(state => state.Errors.Select(error => error.ErrorMessage)).ToArray();
			return View(answer);
		}

		var userId = requestRepo.FindUserId(model.RequestId);
		if (userId == null)
		{
			answer.Errors = new[] { "Запрос не найден" };
			answer.RequestId = null;
			return View(answer);
		}

		var user = await userManager.FindByIdAsync(userId);
		var result = await userManager.RemovePasswordAsync(user);
		if (!result.Succeeded)
		{
			answer.Errors = result.Errors.Select(e => e.ToString()).ToArray();
			return View(answer);
		}

		result = await userManager.AddPasswordAsync(user, model.NewPassword);
		if (!result.Succeeded)
		{
			answer.Errors = result.Errors.Select(e => e.ToString()).ToArray();
			return View(answer);
		}

		metricSender.SendCount("restore_password.set_new_password");

		await requestRepo.DeleteRequest(model.RequestId);

		if (user == null)
		{
			answer.Errors = new[] { "Пользователь был удалён администраторами" };
			return View(answer);
		}

		await authenticationManager.LoginAsync(HttpContext, user, false);

		return RedirectToAction("Index", "Home");
	}
}