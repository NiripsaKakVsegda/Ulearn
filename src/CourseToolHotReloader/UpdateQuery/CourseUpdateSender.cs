using System.Threading.Tasks;
using CourseToolHotReloader.ApiClient;
using CourseToolHotReloader.Dtos;

namespace CourseToolHotReloader.UpdateQuery
{
	public interface ICourseUpdateSender
	{
		Task SendCourseUpdates();
		Task SendFullCourse();
	}

	public class CourseUpdateSender : ICourseUpdateSender
	{
		private readonly ICourseUpdateQuery courseUpdateQuery;
		private readonly IUlearnApiClient ulearnApiClient;
		private readonly IConfig config;

		public CourseUpdateSender(ICourseUpdateQuery courseUpdateQuery, IUlearnApiClient ulearnApiClient, IConfig config)
		{
			this.courseUpdateQuery = courseUpdateQuery;
			this.ulearnApiClient = ulearnApiClient;
			this.config = config;
		}

		public async Task SendCourseUpdates()
		{
			ConsoleWorker.WriteLineWithTime("Загружаем изменения на ulearn");

			var errors = await ulearnApiClient.SendCourseUpdates(config.Path, courseUpdateQuery.GetAllCourseUpdate(),
				courseUpdateQuery.GetAllDeletedFiles(), config.CourseId, config.ExcludeCriterias);

			if (errors.ErrorType != ErrorType.NoErrors)
			{
				ConsoleWorker.WriteErrorWithTime("Ошибка загрузки изменений. " + errors.Message);
				config.PreviousSendHasError = true;
			}
			else
			{
				ConsoleWorker.WriteLineWithTime("Изменения загружены без ошибок");
				config.PreviousSendHasError = false;
			}

			courseUpdateQuery.Clear();
		}

		public async Task SendFullCourse()
		{
			ConsoleWorker.WriteLineWithTime("Загружаем курс на ulearn");

			var errors = await ulearnApiClient.SendFullCourse(config.Path, config.CourseId, config.ExcludeCriterias);

			if (errors.ErrorType != ErrorType.NoErrors)
			{
				ConsoleWorker.WriteErrorWithTime("Ошибка загрузки курса. " + errors.Message);
				config.PreviousSendHasError = true;
			}
			else
			{
				ConsoleWorker.WriteLineWithTime("Курс загружен без ошибок");
				config.PreviousSendHasError = false;
			}
		}
	}
}