using Serilog;
using Ulearn.Core.Configuration;
using uLearn.Web.Core.Utils;
using Web.Api.Configuration;

namespace uLearn.Web.Core;

public static class Program
{
	public static async Task Main(string[] args)
	{
		var configuration = ApplicationConfiguration.Read<WebConfiguration>();
		UlearnLogger.ConfigureLogging(configuration);
		
		await Host
			.CreateDefaultBuilder(args)
			.UseSerilog()
			.ConfigureWebHostDefaults(
				webBuilder => webBuilder
					.UseStartup<Startup>())
			.Build()
			.RunAsync();
	}
}