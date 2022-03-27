using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.Scheduled;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;
using System.Threading.Tasks;
using Database.Repos;
using Microsoft.Extensions.Options;
using Ulearn.Core.Configuration;
using Ulearn.Core.GoogleSheet;
using Ulearn.Web.Api.Models.Parameters.Analytics;
using Ulearn.Web.Api.Utils;
using Web.Api.Configuration;

namespace Ulearn.Web.Api.Workers
{
	public class RefreshGoogleSheetWorker : VostokScheduledApplication
	{
		private readonly IServiceScopeFactory serviceScopeFactory;
		private static ILog log => LogProvider.Get().ForContext(typeof(RefreshGoogleSheetWorker));
		private readonly UlearnConfiguration configuration;
		private readonly StatisticModelUtils statisticModelUtils;

		public RefreshGoogleSheetWorker(IServiceScopeFactory serviceScopeFactory, IOptions<WebApiConfiguration> options,
			StatisticModelUtils statisticModelUtils)
		{
			this.serviceScopeFactory = serviceScopeFactory;
			configuration = options.Value;
			this.statisticModelUtils = statisticModelUtils;
		}

		public override void Setup(IScheduledActionsBuilder builder, IVostokHostingEnvironment environment)
		{
			var scheduler = Scheduler.Multi(Scheduler.Periodical(TimeSpan.FromMinutes(1)), Scheduler.OnDemand(out var refreshGoogleSheets));
			builder.Schedule("RefreshGoogleSheets", scheduler, RefreshGoogleSheets);
			refreshGoogleSheets();
		}

		private async Task RefreshGoogleSheets()
		{
			log.Info("RefreshGoogleSheets");
			using (var scope = serviceScopeFactory.CreateScope())
			{
				var googleSheetExportTasksRepo = scope.ServiceProvider.GetService<IGoogleSheetExportTasksRepo>();
				var timeNow = DateTime.UtcNow;
				var tasks = (await googleSheetExportTasksRepo.GetAllTasks())
					.Where(t =>
						t.RefreshStartDate != null
						&& t.RefreshStartDate >= timeNow
						&& t.RefreshEndDate != null
						&& t.RefreshEndDate <= timeNow);
				foreach (var task in tasks)
				{
					var deltaTime = timeNow - task.RefreshStartDate.Value;
					//Добавить refreshTime в таблицу in minutes в таску, времени должно быть >= RefreshTimeInMinutes
					if ((int)deltaTime.TotalMinutes % task.RefreshTimeInMinutes == 0)
					{
						log.Info($"Start refreshing task {task.Id}");
						var courseStatisticsParams = new CourseStatisticsParams
						{
							CourseId = task.CourseId,
							ListId = task.ListId,
							GroupsIds = task.Groups.Select(g => g.GroupId.ToString()).ToList(),
						};

						try
						{
							var sheet = await statisticModelUtils.GetFilledGoogleSheetModel(courseStatisticsParams, 3000, task.AuthorId);
							var credentialsJson = configuration.GoogleAccessCredentials;
							var client = new GoogleApiClient(credentialsJson);
							client.FillSpreadSheet(task.SpreadsheetId, sheet);
						}
						catch (Exception e)
						{
							//TODO обновлять дату последней выгрузки, добавить поле "последняя ошибка"?
							log.Warn($"Error while filling spread sheed for task {task.Id}");
						}

						log.Info($"Refreshed task {task.Id}");
					}
				}
			}

			log.Info("End RefreshGoogleSheets");
		}
	}
}