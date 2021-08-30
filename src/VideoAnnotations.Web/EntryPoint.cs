using System.Threading.Tasks;
using Ulearn.Common.Api;
using Vostok.Hosting;

namespace Ulearn.VideoAnnotations.Web
{
	public static class EntryPoint
	{
		public static async Task Main(string[] args)
		{
			var application = new WebApplication();
			var setupBuilder = new EnvironmentSetupBuilder("videoannotations-web", args);
			var hostSettings = new VostokHostSettings(application, setupBuilder.EnvironmentSetup);
			var host = new VostokHost(hostSettings);
			await host.WithConsoleCancellation().RunAsync();
		}
	}
}