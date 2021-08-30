using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Vostok.Logging.Abstractions;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using XQueue.Models;

namespace XQueue
{
	public class XQueueClient
	{
		private const string loginUrl = "xqueue/login/";
		private const string getSubmissionUrl = "xqueue/get_submission/";
		private const string putResultUrl = "xqueue/put_result/";

		private static ILog log => LogProvider.Get().ForContext(typeof(XQueueClient));

		private readonly string username;
		private readonly string password;
		private readonly HttpClient client;

		public XQueueClient(string baseUrl, string username, string password)
		{
			this.username = username;
			this.password = password;

			client = new HttpClient(new HttpClientHandler
			{
				AllowAutoRedirect = true,
				UseCookies = true,
				CookieContainer = new CookieContainer(),
			})
			{
				BaseAddress = new Uri(baseUrl),
				Timeout = TimeSpan.FromMinutes(1)
			};
		}

		public XQueueClient(string baseUrl)
			: this(baseUrl, "", "")
		{
		}

		public async Task<bool> Login()
		{
			return await FuncUtils.TrySeveralTimesAsync(TryLogin, 5, () => Task.Delay(TimeSpan.FromSeconds(1)));
		}

		private async Task<bool> TryLogin()
		{
			if (string.IsNullOrEmpty(username))
				return true;

			log.Debug($"Try to login at {loginUrl} with username {username} and password {password.MaskAsSecret()}");

			var loginData = new Dictionary<string, string>
			{
				{ "username", username },
				{ "password", password }
			};

			var response = await client.PostAsync(loginUrl, new FormUrlEncodedContent(loginData));
			if (!response.IsSuccessStatusCode)
			{
				log.Info($"I tried to login in xqueue {loginUrl} with username \"{username}\" and password \"{password.MaskAsSecret()}\"");
				log.Warn($"Unexpected response status code for login: {response.StatusCode}");
				throw new Exception($"Unexpected response status code for login: {response.StatusCode}");
			}

			return response.StatusCode == HttpStatusCode.OK;
		}

		public async Task<XQueueSubmission> GetSubmission(string queueName)
		{
			var getSubmissionsUrl = new Uri(client.BaseAddress, getSubmissionUrl);
			var uriBuilder = new UriBuilder(getSubmissionsUrl);
			var urlParams = HttpUtility.ParseQueryString(getSubmissionsUrl.Query);
			urlParams.Add("queue_name", queueName);
			urlParams.Add("block", "true");
			uriBuilder.Query = urlParams.ToString();

			HttpResponseMessage response;
			try
			{
				response = await client.GetAsync(uriBuilder.ToString());
			}
			catch (Exception e)
			{
				log.Warn( e, $"Can't get submission from xqueue {queueName}: {e.Message}");
				return null;
			}

			if (response.StatusCode == HttpStatusCode.Moved || response.StatusCode == HttpStatusCode.Found)
			{
				log.Warn("Redirected to the login page. Try to authorize again");
				await Login();
				return null;
			}

			if (response.IsSuccessStatusCode)
			{
				try
				{
					var content = await response.Content.ReadAsStringAsync();
					var parsedResponse = content.DeserializeJson<XQueueResponse>();
					if (parsedResponse.ReturnCode != 0)
						return null;
					return parsedResponse.Content.DeserializeJson<XQueueSubmission>();
				}
				catch (Exception e)
				{
					log.Error(e, $"Can\'t parse answer from xqueue: {e.Message}");
					return null;
				}
			}

			return null;
		}

		public async Task<bool> PutResult(XQueueResult result)
		{
			return await FuncUtils.TrySeveralTimesAsync(() => TryPutResult(result), 5, () => Task.Delay(TimeSpan.FromMilliseconds(1)));
		}

		private async Task<bool> TryPutResult(XQueueResult result)
		{
			var content = JsonConvert.SerializeObject(result);
			log.Info($"Try to put submission checking result into xqueue. Url: {putResultUrl}, content: {content}");
			var formContent = new Dictionary<string, string> { { "xqueue_header", result.header }, { "xqueue_body", result.body } };
			var response = await client.PostAsync(putResultUrl, new FormUrlEncodedContent(formContent));

			if (response.StatusCode == HttpStatusCode.Moved || response.StatusCode == HttpStatusCode.Found)
			{
				log.Warn("Redirected to the login page. Try to authorize again");
				await Login();
				throw new Exception("Redirected to the login page. Try to authorize again");
			}

			if (!response.IsSuccessStatusCode)
			{
				log.Warn($"Unexpected response status code while putting results to xqueue: {response.StatusCode}");
				throw new Exception($"Unexpected response status code while putting results to xqueue: {response.StatusCode}");
			}

			var responseContent = await response.Content.ReadAsStringAsync();
			log.Info($"XQueue returned following content: {responseContent}");
			var parsedResponse = responseContent.DeserializeJson<XQueueResponse>();
			return parsedResponse.ReturnCode == 0;
		}
	}
}