using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Vostok.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ulearn.Common;

namespace GiftsGranter
{
	public class StaffClient
	{
		private static ILog Log => LogProvider.Get().ForContext(typeof(StaffClient));
		private readonly string clientAuth;
		private readonly string passportUri = "https://passport.skbkontur.ru/v3/connect/token";
		private string authToken;

		/// <param name="clientAuth">format: "clientId:clientSecret"</param>
		public StaffClient(string clientAuth)
		{
			this.clientAuth = clientAuth;
		}

		private JObject Get(string url)
		{
			var client = CreateHttpClient();
			return FuncUtils.TrySeveralTimesAsync(async () =>
			{
				var fullUrl = $"https://staff.skbkontur.ru/api/{url}";
				var response = await client.GetAsync(fullUrl).ConfigureAwait(false);
				return GetJsonResponse(response, fullUrl);
			}, 2).GetAwaiter().GetResult();
		}

		private HttpClient CreateHttpClient()
		{
			var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
			return client;
		}

		private JObject Post(string url, JObject jsonContent)
		{
			var client = CreateHttpClient();
			var content = new StringContent(JsonConvert.SerializeObject(jsonContent), Encoding.UTF8, "application/json");
			var response = client.PostAsync($"https://staff.skbkontur.ru/api/{url}", content).Result;
			return GetJsonResponse(response, url);
		}

		public JObject GetUser(string sid)
		{
			return Get($"users/{sid}");
		}

		public JObject GetUserGifts(int staffUserId)
		{
			return Get($"users/{staffUserId}/gifts");
		}

		public JObject GrantGift(int staffUserId, int score, CourseSettings courseSettings)
		{
			var gift = GiftsFactory.CreateGift(score, courseSettings);
			return Post($"feed/user_{staffUserId}", gift);
		}

		public string GetRefreshToken(string username, string password)
		{
			var content = new FormUrlEncodedContent(new Dictionary<string, string>
			{
				{ "grant_type", "password" },
				{ "username", username },
				{ "password", password },
				{ "scope", "profiles offline_access" },
			});
			var client = new HttpClient();
			AddClientAuthorizationHeader(client);
			var response = client.PostAsync(passportUri, content).Result;
			var responseJson = GetJsonResponse(response, passportUri);
			return responseJson["refresh_token"].Value<string>();
		}

		private JObject GetJsonResponse(HttpResponseMessage response, string url)
		{
			var result = response.Content.ReadAsStringAsync().Result;
			
			if (response.StatusCode == HttpStatusCode.OK)
				return JObject.Parse(result);
			
			Log.Error("Hint: If invalid_grant ask KONTUR\\pe to generate new refresh_token. It works 180 days.");
			throw new Exception($"Url: {url} StatusCode: {response.StatusCode}\nResult:\n{result}");
		}

		public void UseRefreshToken(string refreshToken)
		{
			var client = new HttpClient();
			var content = new FormUrlEncodedContent(new Dictionary<string, string>
			{
				{ "grant_type", "refresh_token" },
				{ "refresh_token", refreshToken }
			});
			AddClientAuthorizationHeader(client);
			var response = client.PostAsync(passportUri, content).Result;
			var responseJson = GetJsonResponse(response, passportUri);
			authToken = responseJson["access_token"].Value<string>();
		}

		private void AddClientAuthorizationHeader(HttpClient client)
		{
			var base64ClientAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientAuth));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64ClientAuth);
		}
	}
}