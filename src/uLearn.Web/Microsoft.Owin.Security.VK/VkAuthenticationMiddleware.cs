using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;
using uLearn.Web.Microsoft.Owin.Security.VK.Provider;

namespace uLearn.Web.Microsoft.Owin.Security.VK
{
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
		Justification = "Middleware is not disposable.")]
	public class VkAuthenticationMiddleware : AuthenticationMiddleware<VkAuthenticationOptions>
	{
		private readonly ILogger _logger;
		private readonly HttpClient _httpClient;

		public VkAuthenticationMiddleware(
			OwinMiddleware next,
			IAppBuilder app,
			VkAuthenticationOptions options)
			: base(next, options)
		{
			_logger = app.CreateLogger<VkAuthenticationMiddleware>();

			if (Options.Provider == null)
			{
				Options.Provider = new VkAuthenticationProvider();
			}

			if (Options.StateDataFormat == null)
			{
				var dataProtector = app.CreateDataProtector(
					typeof(VkAuthenticationMiddleware).FullName,
					Options.AuthenticationType, "v1");
				Options.StateDataFormat = new PropertiesDataFormat(dataProtector);
			}

			_httpClient = new HttpClient(ResolveHttpMessageHandler(Options));
			_httpClient.Timeout = Options.BackchannelTimeout;
			_httpClient.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected override AuthenticationHandler<VkAuthenticationOptions> CreateHandler()
		{
			return new VkAuthenticationHandler(_httpClient, _logger);
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
			Justification = "Managed by caller")]
		private static HttpMessageHandler ResolveHttpMessageHandler(VkAuthenticationOptions options)
		{
			HttpMessageHandler handler = options.BackchannelHttpHandler ?? new WebRequestHandler();

			// If they provided a validator, apply it or fail.
			if (options.BackchannelCertificateValidator != null)
			{
				// Set the cert validate callback
				WebRequestHandler webRequestHandler = handler as WebRequestHandler;
				if (webRequestHandler == null)
				{
					throw new InvalidOperationException(Resources.Exception_ValidatorHandlerMismatch);
				}

				webRequestHandler.ServerCertificateValidationCallback = options.BackchannelCertificateValidator.Validate;
			}

			return handler;
		}
	}
}