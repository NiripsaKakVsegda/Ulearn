using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntiPlagiarism.Web.CodeAnalyzing;
using AntiPlagiarism.Web.Configuration;
using AntiPlagiarism.Web.Database.Models;
using AntiPlagiarism.Web.Database.Repos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Vostok.Logging.Abstractions;
using Ulearn.Common;

namespace AntiPlagiarism.UpdateDb
{
	public class AntiPlagiarismSnippetsUpdater
	{
		private readonly SubmissionSnippetsExtractor submissionSnippetsExtractor;
		private readonly CodeUnitsExtractor codeUnitsExtractor;
		private readonly IServiceScopeFactory serviceScopeFactory;
		private readonly AntiPlagiarismConfiguration configuration;
		private static ILog log => LogProvider.Get().ForContext(typeof(AntiPlagiarismSnippetsUpdater));

		public AntiPlagiarismSnippetsUpdater(
			SubmissionSnippetsExtractor submissionSnippetsExtractor,
			CodeUnitsExtractor codeUnitsExtractor,
			IServiceScopeFactory serviceScopeFactory,
			IOptions<AntiPlagiarismConfiguration> configuration)
		{
			this.submissionSnippetsExtractor = submissionSnippetsExtractor;
			this.codeUnitsExtractor = codeUnitsExtractor;
			this.serviceScopeFactory = serviceScopeFactory;
			this.configuration = configuration.Value;
		}

		public async Task UpdateAsync(int startFromIndex = 0, bool updateOnlyTokensCount = false)
		{
			log.Info("Начинаю обновлять информацию о сниппетах в базе данных");

			const int maxSubmissionsCount = 1000;
			while (true)
			{
				int lastSubmissionId;
				using (var scope = serviceScopeFactory.CreateScope())
				{
					/* Re-create submissions repo each time for preventing memory leaks */
					var submissionsRepo = scope.ServiceProvider.GetService<ISubmissionsRepo>();

					var submissions = await submissionsRepo.GetSubmissionsAsync(startFromIndex, maxSubmissionsCount);
					if (submissions.Count == 0)
						break;

					var snippetsRepo = scope.ServiceProvider.GetService<ISnippetsRepo>();

					var firstSubmissionId = submissions.First().Id;
					lastSubmissionId = submissions.Last().Id;
					log.Info($"Получил {submissions.Count} следующих решений из базы данных. Идентификаторы решений от {firstSubmissionId} до {lastSubmissionId}");

					foreach (var submission in submissions)
					{
						await submissionsRepo.UpdateSubmissionTokensCountAsync(submission, GetTokensCount(submission.ProgramText, submission.Language));
						if (updateOnlyTokensCount)
							continue;

						try
						{
							await UpdateSnippetsFromSubmissionAsync(snippetsRepo, submission).ConfigureAwait(false);
						}
						catch (Exception e)
						{
							log.Error(e, $"Ошибка при обновлении списка сниппетов решения #{submission.Id}. Продолжаю работу со следующего решения");
						}
					}
				}

				startFromIndex = lastSubmissionId + 1;

				log.Info("Запускаю сборку мусора");
				log.Info($"Потребление памяти до сборки мусора: {GC.GetTotalMemory(false) / 1024}Кб. GC's Gen0: {GC.CollectionCount(0)} Gen1: {GC.CollectionCount(1)} Gen2: {GC.CollectionCount(2)}");
				GC.Collect();
				log.Info($"Потребление памяти после сборки мусора: {GC.GetTotalMemory(false) / 1024}Кб. GC's Gen0: {GC.CollectionCount(0)} Gen1: {GC.CollectionCount(1)} Gen2: {GC.CollectionCount(2)}");
			}

			log.Info("AntiPlagiarismSnippetsUpdater закончил свою работу");
		}

		private async Task UpdateSnippetsFromSubmissionAsync(ISnippetsRepo snippetsRepo, Submission submission)
		{
			var occurences = new HashSet<Tuple<int, int>>(
				(await snippetsRepo.GetSnippetsOccurrencesForSubmissionAsync(submission).ConfigureAwait(false))
				.Select(o => Tuple.Create(o.SnippetId, o.FirstTokenIndex))
			);

			foreach (var (firstTokenIndex, snippet) in submissionSnippetsExtractor.ExtractSnippetsFromSubmission(submission))
			{
				var foundSnippet = await snippetsRepo.GetOrAddSnippetAsync(snippet);
				if (!occurences.Contains(Tuple.Create(foundSnippet.Id, firstTokenIndex)))
				{
					log.Info($"Информация о сниппете #{foundSnippet.Id} в решении #{submission.Id} не найдена, добавляю");
					try
					{
						await snippetsRepo.AddSnippetOccurenceAsync(submission, foundSnippet, firstTokenIndex, configuration.AntiPlagiarism.SubmissionInfluenceLimitInMonths);
					}
					catch (Exception e)
					{
						log.Error(e, $"Ошибка при добавлении сниппета #{foundSnippet.Id} в решении #{submission.Id}");
					}
				}
			}
		}

		private int GetTokensCount(string code, Language language)
		{
			var codeUnits = codeUnitsExtractor.Extract(code, language);
			return codeUnits.Select(u => u.Tokens.Count).Sum();
		}
	}
}