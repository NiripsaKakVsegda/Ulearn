using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NHttp;
using Vostok.Logging.Abstractions;
using Ulearn.Common.Extensions;

namespace Stepik.Api.Tests
{
	public class StepikApiTests
	{
		private const int authorizationCodeRetrievedServerPort = 7777;
		private Thread serverThread;
		private string authorizationCode;

		protected StepikApiClient client;

		private string GetAuthorizationCodeUrl(string clientId, string redirectUri)
		{
			var parameters = new NameValueCollection
			{
				["client_id"] = clientId,
				["redirect_uri"] = redirectUri,
				["response_mode"] = "form_post",
				["response_type"] = "code",
				["scope"] = "read write",
				["state"] = "TestState"
			};

			var builder = new UriBuilder("https://stepik.org/oauth2/authorize")
			{
				Query = parameters.ToQueryString()
			};
			return builder.ToString();
		}

		private void StartAuthorizationCodeRetrieverServer()
		{
			var server = new AuthorizationCodeRetrieverServer(authorizationCodeRetrievedServerPort);
			server.OnAuthorizationCodeReceived += (sender, token) => authorizationCode = token;

			serverThread = new Thread(() => server.Start());
			serverThread.Start();
		}

		protected string GetAuthorizationCodeFromStepik(string clientId)
		{
			authorizationCode = null;
			StartAuthorizationCodeRetrieverServer();

			var redirectUrl = GetRedirectUri();
			var authorizationUrl = GetAuthorizationCodeUrl(clientId, redirectUrl);
			System.Diagnostics.Process.Start(authorizationUrl);

			while (true)
			{
				if (!string.IsNullOrEmpty(authorizationCode))
					break;
				Thread.Sleep(TimeSpan.FromSeconds(1));
			}

			serverThread.Abort();

			return authorizationCode;
		}

		protected static string GetRedirectUri()
		{
			return $"http://localhost:{authorizationCodeRetrievedServerPort}/";
		}

		protected async Task InitializeClient()
		{
			if (string.IsNullOrEmpty(client?.AccessToken))
			{
				var clientId = ConfigurationManager.AppSettings["stepik.clientId"];
				var clientSecret = ConfigurationManager.AppSettings["stepik.clientSecret"];

				var authorizationCode = GetAuthorizationCodeFromStepik(clientId);

				var options = new StepikApiOptions
				{
					ClientId = clientId,
					ClientSecret = clientSecret,
					AuthorizationCode = authorizationCode,
					RedirectUri = GetRedirectUri()
				};
				client = new StepikApiClient(options);

				await client.RetrieveAccessTokenFromAuthorizationCode();
			}
		}
	}

	public class AuthorizationCodeRetrieverServer
	{
		private readonly HttpServer server;
		private static ILog log => LogProvider.Get().ForContext(typeof(AuthorizationCodeRetrieverServer));

		public EventHandler<string> OnAuthorizationCodeReceived;

		public AuthorizationCodeRetrieverServer(int port)
		{
			server = new HttpServer
			{
				EndPoint = new IPEndPoint(IPAddress.Loopback, port),
			};
		}

		public void Start()
		{
			try
			{
				server.RequestReceived += OnHttpRequest;
				server.UnhandledException += OnException;
				server.Start();
			}
			catch (Exception e)
			{
				log.Error(e, $"Some error occurred while AuthorizationCodeRetrieverServer is starting: {e.Message}");
			}
		}

		private void OnHttpRequest(object sender, HttpRequestEventArgs context)
		{
			var error = context.Request.QueryString["error"];
			if (!string.IsNullOrEmpty(error))
			{
				log.Error($"Error on authorization code retrieving: {error}. Try again");
				return;
			}

			var code = context.Request.QueryString["code"];
			if (string.IsNullOrEmpty(code))
			{
				log.Error("Empty authorization code received. Try again");
				return;
			}

			OnAuthorizationCodeReceived(this, code);

			var response = Encoding.UTF8.GetBytes(
				$"Authorization code: \"{WebUtility.HtmlEncode(code)}\"<br>" +
				"Теперь эту вкладку можно закрыть"
			);

			context.Response.ContentType = "text/html";
			context.Response.OutputStream.Write(response, 0, response.Length);
			context.Response.OutputStream.Close();
		}

		private void OnException(object sender, HttpExceptionEventArgs e)
		{
			e.Handled = true;
			var buffer = Encoding.UTF8.GetBytes(e.Exception.ToString());
			e.Response.ContentType = "text/plain";
			e.Response.OutputStream.Write(buffer, 0, buffer.Length);
		}
	}
}