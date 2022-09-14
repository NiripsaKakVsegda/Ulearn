using Serilog;
using Ulearn.Common.Api;
using Ulearn.Core.Configuration;
using uLearn.Web.Core.Utils;
using Vostok.Hosting;
using Web.Api.Configuration;

namespace uLearn.Web.Core;

public static class Program
{
	public static async Task Main(string[] args)
	{
		var application = new Startup();
		var setupBuilder = new EnvironmentSetupBuilder("web", args);
		var hostSettings = new VostokHostSettings(application, setupBuilder.EnvironmentSetup);
		var host = new VostokHost(hostSettings);
		await host.WithConsoleCancellation().RunAsync();
		
		// var configuration = ApplicationConfiguration.Read<WebConfiguration>();
		// UlearnLogger.ConfigureLogging(configuration);
		//
		// await Host
		// 	.CreateDefaultBuilder(args)
		// 	.UseSerilog()
		// 	.ConfigureWebHostDefaults(
		// 		webBuilder => webBuilder
		// 			.UseKestrel(options =>
		// 			{
		// 				options.Limits.MaxRequestBodySize = 160_000_000;
		// 			})
		// 			.UseStartup<Startup>()
		// 			//.UseIISIntegration()
		// 	)
		// 	.Build()
		// 	.RunAsync();
	}
}