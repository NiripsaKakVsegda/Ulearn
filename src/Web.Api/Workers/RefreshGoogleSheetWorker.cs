using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.Scheduled;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;
using System.Threading.Tasks;
using Database.Repos;
using Google;
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
			var scheduler = Scheduler.Periodical(TimeSpan.FromMinutes(1));
			builder.Schedule("RefreshGoogleSheets", scheduler, RefreshGoogleSheets);
		}

		private async Task RefreshGoogleSheets()
		{
			log.Info("RefreshGoogleSheets started");
			using (var scope = serviceScopeFactory.CreateScope())
			{
				var googleSheetExportTasksRepo = scope.ServiceProvider.GetService<IGoogleSheetExportTasksRepo>();
				var timeNow = DateTime.UtcNow;
				var tasks = (await googleSheetExportTasksRepo.GetAllTasks())
					.Where(t =>
						t.RefreshStartDate != null
						&& t.RefreshStartDate.Value <= timeNow
						&& t.RefreshEndDate != null
						&& t.RefreshEndDate.Value >= timeNow);
				foreach (var task in tasks)
				{
					if (task.LastUpdateDate != null && (timeNow - task.LastUpdateDate.Value).TotalMinutes < task.RefreshTimeInMinutes)
						continue;

					log.Info($"Start refreshing task {task.Id}");
					var courseStatisticsParams = new CourseStatisticsParams
					{
						CourseId = task.CourseId,
						ListId = task.ListId,
						GroupsIds = task.Groups.Select(g => g.GroupId.ToString()).ToList(),
					};

					string exceptionMessage = null;
					try
					{
						var sheet = await statisticModelUtils.GetFilledGoogleSheetModel(courseStatisticsParams, 3000, task.AuthorId, timeNow);
						var credentialsJson = configuration.GoogleAccessCredentials;
						var client = new GoogleApiClient(credentialsJson);
						client.FillSpreadSheet(task.SpreadsheetId, sheet);
					}
					catch (GoogleApiException e)
					{
						exceptionMessage = $"{e.Error.Code} {e.Error.Message}";
					}
					catch (Exception e)
					{
						exceptionMessage = e.Message;
					}

					if (exceptionMessage != null)
						log.Warn($"Error while filling spread sheed for task {task.Id}, error message {exceptionMessage}");

					await googleSheetExportTasksRepo.SaveTaskUploadResult(task, timeNow, exceptionMessage);

					log.Info($"Ended refreshing task {task.Id}");
				}
			}

			log.Info("RefreshGoogleSheets ended");
		}
	}
}