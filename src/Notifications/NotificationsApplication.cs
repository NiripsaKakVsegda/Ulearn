using System;
using System.Threading;
using System.Threading.Tasks;
using Database;
using Database.Di;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ulearn.Common.Api;
using Ulearn.Core.Courses.Manager;
using Vostok.Logging.Abstractions;
using Ulearn.Core.Metrics;
using Vostok.Hosting.Abstractions;

namespace Notifications
{
	public class NotificationsApplication : BaseApplication
	{
		private ServiceKeepAliver keepAliver;
		private TimeSpan keepAliveInterval;
		private bool isOneTimeSend;

		private ILog log => LogProvider.Get().ForContext(typeof(NotificationsApplication));

		public NotificationsApplication(bool isOneTimeSend)
		{
			this.isOneTimeSend = isOneTimeSend;
		}

		public override async Task InitializeAsync(IVostokHostingEnvironment environment)
		{
			await base.InitializeAsync(environment);
			keepAliver = new ServiceKeepAliver(configuration.GraphiteServiceName);
			keepAliveInterval = TimeSpan.FromSeconds(configuration.KeepAliveInterval ?? 30);
		}

		public override async Task RunAsync(IVostokHostingEnvironment environment)
		{
			/* Pass first argument 'send' to send emails to addresses from `emails.txt` with content from `content.txt` (notifications daemon is not started in this case)*/
			if (isOneTimeSend)
			{
				var sender = serviceProvider.GetService<OneTimeEmailSender>();
				await sender!.SendEmails();
				return;
			}

			var notificationsConfiguration = serviceProvider.GetService<IOptions<NotificationsConfiguration>>();
			if (!notificationsConfiguration!.Value.Enabled)
			{
				Thread.Sleep(Timeout.Infinite);
				return;
			}

			MainLoop().Wait();
		}

		protected override void ConfigureServices(IServiceCollection services, IVostokHostingEnvironment hostingEnvironment)
		{
			base.ConfigureServices(services, hostingEnvironment);

			services.AddDbContext<UlearnDb>( // AddDbContextPool: DbContext Pooling does not dispose LazyLoader https://github.com/dotnet/efcore/issues/11308
				options => options
					.UseLazyLoadingProxies()
					.UseNpgsql(configuration.Database, o => o.SetPostgresVersion(13, 2))
			);

			services.Configure<NotificationsConfiguration>(options =>
				options.SetFrom(hostingEnvironment.SecretConfigurationProvider.Get<NotificationsConfiguration>(hostingEnvironment.SecretConfigurationSource)));
		}

		protected override void ConfigureDi(IServiceCollection services)
		{
			base.ConfigureDi(services);

			services.AddSingleton<OneTimeEmailSender>();
			services.AddSingleton<IEmailSender, KonturSpamEmailSender>();
			services.AddScoped<INotificationSender, NotificationSender>();
			services.AddSingleton<ITelegramSender, TelegramSender>();
			services.AddScoped<DeliveriesProcessor>();
			services.AddScoped(sp => new MetricSender(
				((IOptions<NotificationsConfiguration>)sp.GetService(typeof(IOptions<NotificationsConfiguration>)))!.Value.GraphiteServiceName));
			services.AddSingleton<UpdateCoursesWorker>();

			services.AddDatabaseServices(true);
		}

		public async Task MainLoop()
		{
			while (true)
			{
				try
				{
					using (var scope = serviceProvider.CreateScope())
					{
						var sender = scope.ServiceProvider.GetService<DeliveriesProcessor>();
						await sender!.CreateDeliveries().ConfigureAwait(false);
						await sender.SendDeliveries().ConfigureAwait(false);
					}
				}
				catch (Exception e)
				{
					log.Error(e, "Can\'t create deliveries or send them");
					log.Info("Waiting one second and repeat");
				}

				keepAliver.Ping(keepAliveInterval);
				await Task.Delay(TimeSpan.FromSeconds(30)).ConfigureAwait(false);
			}
		}
	}
}