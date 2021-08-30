using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntiPlagiarism.Api.Models.Parameters;
using AntiPlagiarism.Api.Models.Results;
using AntiPlagiarism.Web.CodeAnalyzing;
using AntiPlagiarism.Web.Configuration;
using AntiPlagiarism.Web.Database;
using AntiPlagiarism.Web.Database.Models;
using AntiPlagiarism.Web.Database.Repos;
using AntiPlagiarism.Web.Extensions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Vostok.Logging.Abstractions;
using Ulearn.Common;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Common.Extensions;

namespace AntiPlagiarism.Web.Controllers
{
	[Route("/api")]
	public class ApiController : BaseController
	{
		private readonly ISubmissionsRepo submissionsRepo;
		private readonly ISnippetsRepo snippetsRepo;
		private readonly ITasksRepo tasksRepo;
		private readonly IWorkQueueRepo workQueueRepo;
		private readonly IMostSimilarSubmissionsRepo mostSimilarSubmissionsRepo;
		private readonly IManualSuspicionLevelsRepo manualSuspicionLevelsRepo;
		private readonly PlagiarismDetector plagiarismDetector;
		private readonly CodeUnitsExtractor codeUnitsExtractor;
		private readonly IServiceScopeFactory serviceScopeFactory;
		private readonly NewSubmissionHandler newSubmissionHandler;
		private readonly AntiPlagiarismConfiguration configuration;
		private static ILog log => LogProvider.Get().ForContext(typeof(ApiController));

		public ApiController(
			AntiPlagiarismDb db,
			ISubmissionsRepo submissionsRepo, ISnippetsRepo snippetsRepo, ITasksRepo tasksRepo,
			IClientsRepo clientsRepo, IWorkQueueRepo workQueueRepo,
			IMostSimilarSubmissionsRepo mostSimilarSubmissionsRepo,
			IManualSuspicionLevelsRepo manualSuspicionLevelsRepo,
			PlagiarismDetector plagiarismDetector,
			CodeUnitsExtractor codeUnitsExtractor,
			IServiceScopeFactory serviceScopeFactory,
			NewSubmissionHandler newSubmissionHandler,
			IOptions<AntiPlagiarismConfiguration> configuration)
			: base(clientsRepo, db)
		{
			this.submissionsRepo = submissionsRepo;
			this.snippetsRepo = snippetsRepo;
			this.tasksRepo = tasksRepo;
			this.workQueueRepo = workQueueRepo;
			this.mostSimilarSubmissionsRepo = mostSimilarSubmissionsRepo;
			this.manualSuspicionLevelsRepo = manualSuspicionLevelsRepo;
			this.plagiarismDetector = plagiarismDetector;
			this.codeUnitsExtractor = codeUnitsExtractor;
			this.newSubmissionHandler = newSubmissionHandler;
			this.serviceScopeFactory = serviceScopeFactory;
			this.configuration = configuration.Value;
		}

		/// <summary>
		/// Зарегистрировать код студента в антиплагиате. Долгая операция, выполняется не сразу, ставится в очередь
		/// </summary>
		/// <remarks>
		/// token — Для авторизации. Для получения пишите на support@ulearn.me
		/// 
		/// additional_info — Json с любой дополнительной информацией, которую хочется сохранять вместе с submission; без изменений возвращается в ответах, см. класс SubmissionInfo
		///
		/// client_submission_id — Id посылки кода студентом в терминах клиентского кода. Любая строка
		/// </remarks>
		[HttpPost(Api.Urls.AddSubmission)]
		public async Task<ActionResult<AddSubmissionResponse>> AddSubmission([FromQuery]string token, AddSubmissionParameters parameters)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			if (parameters.Code.Length > configuration.AntiPlagiarism.MaxCodeLength)
				return BadRequest(new ErrorResponse($"Code is too long. Maximum length is {configuration.AntiPlagiarism.MaxCodeLength} bytes"));

			var tokensCount = GetTokensCount(parameters.Code, parameters.Language);
			var submission = await submissionsRepo.AddSubmissionAsync(
				client.Id,
				parameters.TaskId,
				parameters.AuthorId,
				parameters.Language,
				parameters.Code,
				tokensCount,
				parameters.AdditionalInfo,
				parameters.ClientSubmissionId
			).ConfigureAwait(false);

			log.Info(
				"Добавлено новое решение {submissionId} по задаче {taskId}, автор {authorId}, язык {language}, доп. информация {additionalInfo}",
				submission.Id,
				parameters.TaskId,
				parameters.AuthorId,
				parameters.Language,
				parameters.AdditionalInfo
			);

			await workQueueRepo.Add(QueueIds.NewSubmissionsQueue, submission.Id.ToString()).ConfigureAwait(false);

			return new AddSubmissionResponse
			{
				SubmissionId = submission.Id,
			};
		}

		private int GetTokensCount(string code, Language language)
		{
			var codeUnits = codeUnitsExtractor.Extract(code, language);
			return codeUnits.Select(u => u.Tokens.Count).Sum();
		}

		[HttpPost(Api.Urls.RebuildSnippetsForTask)]
		public async Task<IActionResult> RebuildSnippetsForTask([FromQuery] RebuildSnippetsForTaskParameters parameters)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			await snippetsRepo.RemoveSnippetsOccurrencesForTaskAsync(parameters.TaskId).ConfigureAwait(false);
			var submissions = await submissionsRepo.GetSubmissionsByTaskAsync(client.Id, parameters.TaskId, parameters.Language)
				.ConfigureAwait(false);
			foreach (var submission in submissions)
			{
				await submissionsRepo.UpdateSubmissionTokensCountAsync(submission, GetTokensCount(submission.ProgramText, submission.Language));
				await newSubmissionHandler.ExtractSnippetsFromSubmissionAsync(submission).ConfigureAwait(false);
			}

			await newSubmissionHandler.CalculateTaskStatisticsParametersAsync(client.Id, parameters.TaskId, parameters.Language).ConfigureAwait(false);

			return Json(new RebuildSnippetsForTaskResponse
			{
				SubmissionsIds = submissions.Select(s => s.Id).ToList(),
			});
		}

		[HttpPost(Api.Urls.RecalculateTaskStatistics)]
		public async Task<IActionResult> RecalculateTaskStatistics([FromQuery] RecalculateTaskStatisticsParameters parameters)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var taskIds = await tasksRepo.GetTaskIds().ConfigureAwait(false);

			if (parameters.FromTaskId.HasValue && parameters.TaskId.HasValue)
				return BadRequest(new ErrorResponse("You should pass from_task_id or task_id, not both"));
			if (parameters.FromTaskId.HasValue)
				taskIds = taskIds.Skip(taskIds.FindIndex(taskId => taskId == parameters.FromTaskId)).ToList();
			if (parameters.TaskId.HasValue)
				taskIds = taskIds.Where(t => t == parameters.TaskId.Value).ToList();

			var weights = new Dictionary<Guid, List<double>>();
			foreach (var (index, taskId) in taskIds.Enumerate(start: 1))
			{
				weights[taskId] = await newSubmissionHandler.CalculateTaskStatisticsParametersAsync(client.Id, taskId, parameters.Language).ConfigureAwait(false);
				weights[taskId].Sort();

				log.Info($"RecalculateTaskStatistics: обработано {index.PluralizeInRussian(RussianPluralizationOptions.Tasks)} из {taskIds.Count}");

				GC.Collect();
			}

			return Json(new RecalculateTaskStatisticsResponse
			{
				TaskIds = taskIds,
				Weights = weights,
				Language = parameters.Language
			});
		}

		[HttpGet(Api.Urls.GetSubmissionPlagiarisms)]
		public async Task<IActionResult> GetSubmissionPlagiarisms([FromQuery] GetSubmissionPlagiarismsParameters parameters)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var submission = await submissionsRepo.FindSubmissionByIdAsync(parameters.SubmissionId).ConfigureAwait(false);
			if (submission == null || submission.ClientId != client.Id)
				return NotFound(new ErrorResponse("Invalid submission id"));

			var suspicionLevels = await GetSuspicionLevelsAsync(submission.TaskId, submission.Language).ConfigureAwait(false);

			var result = new GetSubmissionPlagiarismsResponse
			{
				SubmissionInfo = submission.GetSubmissionInfoForApi(),
				Plagiarisms = await plagiarismDetector.GetPlagiarismsAsync(submission, suspicionLevels, configuration.AntiPlagiarism.SubmissionInfluenceLimitInMonths).ConfigureAwait(false),
				TokensPositions = plagiarismDetector.GetNeededTokensPositions(submission.ProgramText, submission.Language),
				SuspicionLevels = suspicionLevels,
				AnalyzedCodeUnits = GetAnalyzedCodeUnits(submission),
			};

			return Json(result);
		}

		[HttpGet(Api.Urls.GetAuthorPlagiarisms)]
		public async Task<IActionResult> GetAuthorPlagiarisms([FromQuery] GetAuthorPlagiarismsParameters parameters)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var maxLastSubmissionsCount = configuration.AntiPlagiarism.Actions.GetAuthorPlagiarisms.MaxLastSubmissionsCount;
			if (parameters.LastSubmissionsCount <= 0 || parameters.LastSubmissionsCount > maxLastSubmissionsCount)
				return BadRequest(new ErrorResponse($"Invalid last_submissions_count. This value should be at least 1 and at most {maxLastSubmissionsCount}"));

			var suspicionLevels = await GetSuspicionLevelsAsync(parameters.TaskId, parameters.Language).ConfigureAwait(false);

			var submissions = await submissionsRepo.GetSubmissionsByAuthorAndTaskAsync(client.Id, parameters.AuthorId, parameters.TaskId, parameters.Language, parameters.LastSubmissionsCount).ConfigureAwait(false);
			var result = new GetAuthorPlagiarismsResponse
			{
				SuspicionLevels = suspicionLevels,
			};
			foreach (var submission in submissions)
			{
				using (var scope = serviceScopeFactory.CreateScope())
				{
					/* Create internal plagiarismDetector for preventing memory leaks */
					var internalPlagiarismDetector = scope.ServiceProvider.GetService<PlagiarismDetector>();

					result.ResearchedSubmissions.Add(new ResearchedSubmission
					{
						SubmissionInfo = submission.GetSubmissionInfoForApi(),
						Plagiarisms = await internalPlagiarismDetector.GetPlagiarismsAsync(submission, suspicionLevels, configuration.AntiPlagiarism.SubmissionInfluenceLimitInMonths).ConfigureAwait(false),
						TokensPositions = internalPlagiarismDetector.GetNeededTokensPositions(submission.ProgramText, submission.Language),
						AnalyzedCodeUnits = GetAnalyzedCodeUnits(submission),
					});
				}
			}

			return Json(result);
		}

		// TODO
		private List<AnalyzedCodeUnit> GetAnalyzedCodeUnits(Submission submission)
		{
			var codeUnits = codeUnitsExtractor.Extract(submission.ProgramText, submission.Language);
			return codeUnits.Select(
				u => new AnalyzedCodeUnit
				{
					Name = u.Path.ToString(),
					FirstTokenIndex = u.FirstTokenIndex,
					TokensCount = u.Tokens.Count,
				}).ToList();
		}

		[ItemNotNull]
		private async Task<SuspicionLevels> GetSuspicionLevelsAsync(Guid taskId, Language language)
		{
			var taskStatisticsParameters = await tasksRepo.FindTaskStatisticsParametersAsync(taskId, language).ConfigureAwait(false);
			var manualSuspicionLevels = await manualSuspicionLevelsRepo.GetManualSuspicionLevelsAsync(taskId, language);

			var automaticFaintSuspicion = configuration.AntiPlagiarism.StatisticsAnalyzing.MaxFaintSuspicionLevel;
			var automaticStrongSuspicion = configuration.AntiPlagiarism.StatisticsAnalyzing.MaxStrongSuspicionLevel;
			if (taskStatisticsParameters != null)
				(automaticFaintSuspicion, automaticStrongSuspicion) = GetAutomaticSuspicionLevels(taskStatisticsParameters);

			return new SuspicionLevels
			{
				AutomaticFaintSuspicion = automaticFaintSuspicion,
				AutomaticStrongSuspicion = automaticStrongSuspicion,
				ManualFaintSuspicion = manualSuspicionLevels?.FaintSuspicion,
				ManualStrongSuspicion = manualSuspicionLevels?.StrongSuspicion,
				FaintSuspicion = manualSuspicionLevels?.FaintSuspicion ?? automaticFaintSuspicion,
				StrongSuspicion = manualSuspicionLevels?.StrongSuspicion ?? automaticStrongSuspicion,
			};
		}

		private (double automaticFaintSuspicion, double automaticStrongSuspicion) GetAutomaticSuspicionLevels(TaskStatisticsParameters taskStatisticsParameters)
		{
			var faintSuspicionCoefficient = configuration.AntiPlagiarism.StatisticsAnalyzing.FaintSuspicionCoefficient;
			var strongSuspicionCoefficient = configuration.AntiPlagiarism.StatisticsAnalyzing.StrongSuspicionCoefficient;
			var minFaintSuspicionLevel = configuration.AntiPlagiarism.StatisticsAnalyzing.MinFaintSuspicionLevel;
			var minStrongSuspicionLevel = configuration.AntiPlagiarism.StatisticsAnalyzing.MinStrongSuspicionLevel;
			var maxFaintSuspicionLevel = configuration.AntiPlagiarism.StatisticsAnalyzing.MaxFaintSuspicionLevel;
			var maxStrongSuspicionLevel = configuration.AntiPlagiarism.StatisticsAnalyzing.MaxStrongSuspicionLevel;

			var (faintSuspicion, strongSuspicion)
				= StatisticsParametersFinder.GetSuspicionLevels(taskStatisticsParameters.Mean, taskStatisticsParameters.Deviation, faintSuspicionCoefficient, strongSuspicionCoefficient);

			var automaticFaintSuspicion = GetSuspicionLevelWithThreshold(faintSuspicion, minFaintSuspicionLevel, maxFaintSuspicionLevel);
			var automaticStrongSuspicion = GetSuspicionLevelWithThreshold(strongSuspicion, minStrongSuspicionLevel, maxStrongSuspicionLevel);
			return (automaticFaintSuspicion, automaticStrongSuspicion);
		}

		private static double GetSuspicionLevelWithThreshold(double value, double minValue, double maxValue)
		{
			if (minValue > maxValue)
				throw new ArgumentException("minValue should be less than maxValue");
			if (value < minValue)
				return minValue;
			if (value > maxValue)
				return maxValue;
			return value;
		}

		[HttpGet(Api.Urls.GetMostSimilarSubmissions)]
		public async Task<IActionResult> GetMostSimilarSubmissions([FromQuery] GetMostSimilarSubmissionsParameters parameters)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var mostSimilarSubmissions = await mostSimilarSubmissionsRepo.GetMostSimilarSubmissionsByTaskAsync(client.Id, parameters.TaskId, parameters.Language).ConfigureAwait(false);
			var result = new GetMostSimilarSubmissionsResponse
			{
				MostSimilarSubmissions = mostSimilarSubmissions,
			};
			return Json(result);
		}

		[HttpGet(Api.Urls.GetSuspicionLevels)]
		public async Task<IActionResult> GetSuspicionLevelsAsync([FromQuery] GetSuspicionLevelsParameters parameters)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var suspicionLevels = await GetSuspicionLevelsAsync(parameters.TaskId, parameters.Language).ConfigureAwait(false);

			var result = new GetSuspicionLevelsResponse { SuspicionLevels = suspicionLevels };
			return Json(result);
		}

		[HttpPost(Api.Urls.SetSuspicionLevels)]
		public async Task<IActionResult> SetSuspicionLevelsAsync([FromQuery]string token, SetSuspicionLevelsParameters parameters)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			await manualSuspicionLevelsRepo.SetManualSuspicionLevelsAsync(new ManualSuspicionLevels
			{
				TaskId = parameters.TaskId,
				Language = parameters.Language,
				FaintSuspicion = parameters.FaintSuspicion,
				StrongSuspicion = parameters.StrongSuspicion,
				Timestamp = DateTime.Now
			});

			var suspicionLevels = await GetSuspicionLevelsAsync(parameters.TaskId, parameters.Language);

			var result = new GetSuspicionLevelsResponse { SuspicionLevels = suspicionLevels };
			return Json(result);
		}
	}
}