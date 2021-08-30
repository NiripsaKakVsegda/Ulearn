using Graphite.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ulearn.Core.Configuration;
using Ulearn.Core.Metrics;
using uLearn.Web;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(GraphiteMetricsPipeStart), "PreStart")]

namespace uLearn.Web
{
	public class GraphiteMetricsPipeStart
	{
		public static void PreStart()
		{
			var connectionString = ApplicationConfiguration.Read<UlearnConfiguration>().StatsdConnectionString;
			var isGraphiteSendingEnabled = !string.IsNullOrEmpty(connectionString);

			if (!isGraphiteSendingEnabled)
				return;

			// Make sure MetricsPipe handles BeginRequest and EndRequest
			DynamicModuleUtility.RegisterModule(typeof(MetricsPipeStartupModule));

			MetricsPipeStartupModule.Settings.ReportRequestTime = true;
			// The prefix is added in GraphiteConfiguration with ConfigurationManager.GetSection("graphite"). If you specify here, there will be duplication
			MetricsPipeStartupModule.Settings.RequestTimePrefix = MetricSender.BuildKey(null, "web", "request.time");
		}
	}
}