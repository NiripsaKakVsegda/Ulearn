using Database.Models;
using Database.Repos;
using LtiLibrary.Core.Outcomes.v1;
using Newtonsoft.Json;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Model;
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
		
		var ltiRequestJson = JsonConvert.DeserializeObject<LtiRequest>(ltiRequest);
		
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