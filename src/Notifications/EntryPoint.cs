using System.Threading.Tasks;
using Ulearn.Common.Api;
using Ulearn.Core.Courses.Manager;
using Vostok.Applications.Scheduled;
using Vostok.Hosting;
using Vostok.Hosting.Abstractions.Composite;

namespace Notifications
{
	public class EntryPoint
	{
		public static async Task Main(string[] args)
		{
			var isOneTimeSend = args.Length > 0 && args[0] == "send";
			var mainApplication = new NotificationsApplication(isOneTimeSend);
			var compositeApplication = new CompositeApplication(builder => builder
				.AddApplication(mainApplication)
				.AddScheduled(mainApplication.SetupScheduled<UpdateCoursesWorker>));
			var setupBuilder = new EnvironmentSetupBuilder("notifications", args);
			var hostSettings = new VostokHostSettings(compositeApplication, setupBuilder.EnvironmentSetup);
			var host = new VostokHost(hostSettings);
			await host.WithConsoleCancellation().RunAsync();
		}
	}
}