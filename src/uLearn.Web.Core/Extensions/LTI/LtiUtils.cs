using System.Collections.Specialized;
using System.Data;
using Database;
using Database.Models;
using Database.Repos;
using LtiLibrary.Core.Common;
using LtiLibrary.NetCore.Lti.v1;
using LtiLibrary.Core.OAuth;
using LtiLibrary.Core.Outcomes.v1;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Ulearn.Core.Courses.Slides;
using uLearn.Web.Core.Controllers;
using Vostok.Logging.Abstractions;

namespace uLearn.Web.Core.Extensions.LTI;

public static class LtiUtils
{
	private static ILog log => LogProvider.Get().ForContext(typeof(LtiUtils));

	public static async Task SubmitScore(
		ILtiRequestsRepo ltiRequestsRepo,
		ILtiConsumersRepo consumersRepo,
		IVisitsRepo visitsRepo,
		string courseId,
		Slide slide,
		string userId,
		Visit visit = null)
	{
		var ltiRequest = await ltiRequestsRepo.Find(courseId, userId, slide.Id);
		if (ltiRequest == null)
			throw new Exception("LtiRequest for user '" + userId + "' not found");

		var ltiRequestJson = JsonConvert.DeserializeObject<Ulearn.Core.Model.LtiRequest>(ltiRequest);

		var consumerSecret = (await consumersRepo.Find(ltiRequestJson.ConsumerKey)).Secret;

		var score = visit?.Score ?? await visitsRepo.GetScore(courseId, slide.Id, userId);

		log.Info($"Надо отправить результаты слайда {slide.Id} пользователя {userId} по LTI. Нашёл LtiRequest: {ltiRequest}"); //jsonSerialize of lti request
		UriBuilder uri;
		try
		{
			uri = new UriBuilder(ltiRequestJson.LisOutcomeServiceUrl);
		}
		catch (Exception e)
		{
			log.Error(e, $"Неверный адрес отправки результатов по LTI: {ltiRequestJson.LisOutcomeServiceUrl}");
			throw;
		}

		if (uri.Host == "localhost")
		{
			uri.Host = "192.168.33.10";
			uri.Port = 80;
			uri.Scheme = "http";
		}

		var maxScore = ControllerUtils.GetMaxScoreForUsersSlide(slide, true, false, false);
		var outputScore = score / (double)maxScore;
		log.Info($"Отправляю результаты на {ltiRequestJson.LisOutcomeServiceUrl}: {score} из {maxScore} ({outputScore})");

		/* Sometimes score is bigger then slide's MaxScore, i.e. in case of manual checking */
		if (score > maxScore)
			outputScore = 1;
		var result = OutcomesClient.PostScore(uri.ToString(), ltiRequestJson.ConsumerKey, consumerSecret,
			ltiRequestJson.LisResultSourcedId, outputScore);

		if (!result.IsValid)
			throw new Exception(uri + "\r\n\r\n" + result.Message);
	}
}

public class LtiAuthentication
{
	private static ILog log => LogProvider.Get().ForContext(typeof(LtiAuthentication));

	private readonly UlearnDb db;
	private readonly UlearnUserManager userManager;
	private readonly AuthenticationManager authenticationManager;

	public LtiAuthentication(
		UlearnDb db,
		AuthenticationManager authenticationManager,
		UlearnUserManager userManager
	)
	{
		this.db = db;
		this.userManager = userManager;
		this.authenticationManager = authenticationManager;
	}

	public async Task<string> Authenticate(HttpContext context, LtiRequest ltiRequest)
	{
		// Make sure the request is not being replayed
		var timeout = TimeSpan.FromMinutes(5);
		var oauthTimestampAbsolute = OAuthConstants.Epoch.AddSeconds(ltiRequest.Timestamp);
		if (DateTime.UtcNow - oauthTimestampAbsolute > timeout)
		{
			log.Error($"Неправильное время у LTI-запроса: {ltiRequest.Timestamp}, сейчас {DateTime.UtcNow}, прошло больше 5 минут");
			throw new LtiException("Expired " + OAuthConstants.TimestampParameter);
		}

		var consumer = await db.Consumers.SingleOrDefaultAsync(c => c.Key == ltiRequest.ConsumerKey);
		if (consumer == null)
			throw new LtiException("Invalid " + OAuthConstants.ConsumerKeyParameter + " " + ltiRequest.ConsumerKey);

		/* Substitute http(s) scheme with real scheme from header */
		var uriBuilder = new UriBuilder(ltiRequest.Url)
		{
			Scheme = context.Request.GetRealScheme(),
			Port = context.Request.GetRealPort()
		};

		ltiRequest.Url = uriBuilder.Uri;

		var oldParams = new NameValueCollection();
		foreach (var ltiRequestParameter in ltiRequest.Parameters)
			oldParams.Add(ltiRequestParameter.Key, ltiRequestParameter.Value);

		var signature = ltiRequest.GenerateSignature(consumer.Secret);
		if (!signature.Equals(ltiRequest.Signature))
			throw new LtiException("Invalid " + OAuthConstants.SignatureParameter);

		return await Internal_Authenticate(context, ltiRequest);
		// If we made it this far the request is valid
	}

	/// <summary>
	/// Invoked after the LTI request has been authenticated so the application can sign in the application user.
	/// </summary>
	/// <param name="context">Contains information about the login session as well as the LTI request.</param>
	/// <param name="claims">Optional set of claims to add to the identity.</param>
	/// <returns>A <see cref="Task"/> representing the completed operation.</returns>
	private async Task<string> Internal_Authenticate(HttpContext context, LtiRequest ltiRequest)
	{
		log.Info($"LTI обрабатывает запрос на {context.Request.Path + context.Request.QueryString.Value}");

		var loginProvider = string.Join(":", "LTI", ltiRequest.ConsumerKey);
		var providerKey = ltiRequest.UserId;
		var ltiLogin = new UserLoginInfo(loginProvider, providerKey, "LTI");

		var identity = await GetIdentityForLtiLogin(context, ltiRequest, ltiLogin);

		if (identity == null)
			throw new Exception("Can\'t authenticate identity for LTI user");

		log.Info($"Аутенфицирую пользователя по identity: {identity.UserName}");
		
		await authenticationManager.LoginAsync(context, identity, false);
		return identity.Id;
	}

	private async Task<ApplicationUser> GetIdentityForLtiLogin(HttpContext context, LtiRequest ltiRequest, UserLoginInfo ltiLogin)
	{
		var ltiLoginUser = await userManager.FindByLoginAsync(ltiLogin.LoginProvider, ltiLogin.ProviderKey);
		if (ltiLoginUser != null)
		{
			log.Info($"Нашёл LTI-логин: провайдер {ltiLogin.LoginProvider}, идентификатор {ltiLogin.ProviderKey}, он принадлежит пользователю {ltiLoginUser.UserName} (Id = {ltiLoginUser.Id})");
			return ltiLoginUser;
		}

		log.Info($"Не нашёл LTI-логин: провайдер {ltiLogin.LoginProvider}, идентификатор {ltiLogin.ProviderKey}");

		if (context.User.Identity.IsAuthenticated)
		{
			var ulearnPrincipal = context.User;
			var ulearnUserFromPrincipal = await userManager.GetUserAsync(ulearnPrincipal);
			log.Info($"Пришёл LTI-запрос на аутенфикацию, пользователь уже аутенфицирован на ulearn: {ulearnPrincipal.Identity.Name}. Прикрепляю к пользователю LTI-логин");
			await userManager.AddLoginAsync(ulearnUserFromPrincipal, ltiLogin);
				
			return ulearnUserFromPrincipal;
		}

		var userName = GenerateUserName(ltiRequest);

		if (string.IsNullOrEmpty(userName))
			throw new Exception("Can't generate username");

		log.Info($"Сгенерировал имя пользователя для LTI-пользователя: {userName}, ищу пользователя по этому имени");

		var ulearnUser = await userManager.FindByNameAsync(userName);
		if (ulearnUser == null)
		{
			log.Info("Не нашёл пользователя с таким именем, создаю нового");
			ulearnUser = new ApplicationUser { UserName = userName };
			var result = await userManager.CreateAsync(ulearnUser);
			if (!result.Succeeded)
			{
				var errors = string.Join("\n\n", result.Errors);
				throw new Exception("Can't create user for LTI: " + errors);
			}
		}

		await userManager.AddLoginAsync(ulearnUser, ltiLogin);
			
		return ulearnUser;
	}

	/// <summary>
	/// Generate a valid application username using information from an LTI request. The default
	/// ASP.NET application using Microsoft Identity uses an email address as the username. This
	/// code will generate an "anonymous" email address if one is not supplied in the LTI request.
	/// </summary>
	/// <param name="ltiRequest">Contains information about the login session as the LTI request.</param>
	/// <returns>A <see cref="Task"/> representing the completed operation.</returns>
	private static string GenerateUserName(LtiRequest ltiRequest)
	{
		if (!string.IsNullOrEmpty(ltiRequest.LisPersonEmailPrimary))
			return ltiRequest.LisPersonEmailPrimary;

		var username = ltiRequest.UserId;
		if (string.IsNullOrEmpty(ltiRequest.ToolConsumerInstanceUrl)
			|| !Uri.TryCreate(ltiRequest.ToolConsumerInstanceUrl, UriKind.Absolute, out var url))
			return string.Concat(username, "@", ltiRequest.ConsumerKey);

		return string.Concat(username, "@", url.Host);
	}
}