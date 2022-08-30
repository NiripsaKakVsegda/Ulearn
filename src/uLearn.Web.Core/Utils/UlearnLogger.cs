using Serilog;
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