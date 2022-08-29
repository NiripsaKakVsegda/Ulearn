using Serilog;
using Serilog.Context;
using Serilog.Events;
using Ulearn.Core.Logging;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Serilog;
using Web.Api.Configuration;

namespace uLearn.Web.Core.Utils;

public static class UlearnLogger
{
	public static void ConfigureLogging(WebConfiguration configuration)
	{
		var log = LoggerSetup
			.Setup(configuration.HostLog, configuration.GraphiteServiceName, false);
		Log.Logger = new LoggerConfiguration()
			.Enrich.FromLogContext()
			.WriteTo
			.Sink(new VostokSink(log))
			.CreateLogger();
		LogProvider.Configure(log);
	}
}

public class LogUserNameAndIpMiddleware
{
	private readonly RequestDelegate next;

	public LogUserNameAndIpMiddleware(RequestDelegate next)
	{
		this.next = next;
	}

	public Task Invoke(HttpContext context)
	{
		//Get username  
		var username = context.User.Identity.IsAuthenticated ? context.User.Identity.Name : "anonymous";
		LogContext.PushProperty("User", username);

		//Get remote real IP address  
		const string xRealIpHeaderName = "X-Real-IP";
		var ip = context.Request.Headers[xRealIpHeaderName].ToString();
		LogContext.PushProperty("IP", !string.IsNullOrWhiteSpace(ip) ? ip : "unknown");

		return next(context);
	}
}