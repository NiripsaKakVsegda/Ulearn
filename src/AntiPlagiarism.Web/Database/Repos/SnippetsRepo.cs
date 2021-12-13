using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AntiPlagiarism.Api.Models;
using AntiPlagiarism.Web.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Logging.Abstractions;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using IsolationLevel = System.Data.IsolationLevel;

namespace AntiPlagiarism.Web.Database.Repos
{
	public interface ISnippetsRepo
	{
		Task<Snippet> AddSnippetAsync(int tokensCount, SnippetType type, int hash);
		Task<bool> IsSnippetExistsAsync(int tokensCount, SnippetType type, int hash);
		Task<SnippetOccurence> AddSnippetOccurenceAsync(Submission submission, Snippet snippet, int firstTokenIndex, int submissionInfluenceLimitInMonths);
		Task<List<SnippetOccurence>> GetSnippetsOccurrencesForSubmissionAsync(Submission submission, int maxCount, int authorsCountMinThreshold, int authorsCountMaxThreshold);
		Task<List<SnippetOccurence>> GetSnippetsOccurrencesForSubmissionAsync(Submission submission);
		Task<Dictionary<int, SnippetStatistics>> GetSnippetsStatisticsAsync(int clientId, Guid taskId, Language language, IEnumerable<int> snippetsIds);
		Task<List<SnippetOccurence>> GetSnippetsOccurrences(IEnumerable<int> snippetIds, Expression<Func<SnippetOccurence, bool>> filterFunction);
		Task<List<int>> GetSubmissionIdsWithSameSnippets(IEnumerable<int> snippetIds, Expression<Func<SnippetOccurence, bool>> filterFunction, int maxSubmissionsCount);
		Task RemoveSnippetsOccurrencesForTaskAsync(Guid taskId);
		Task<Snippet> GetOrAddSnippetAsync(Snippet snippet);
		Task UpdateOldSnippetsStatisticsAsync(DateTime from, DateTime to);
		Task<OldSubmissionsInfluenceBorder> GetOldSubmissionsInfluenceBorderAsync();
		Task SetOldSubmissionsInfluenceBorderAsync(DateTime? oldSnippetsInfluenceBorder);
	}

	public class SnippetsRepo : ISnippetsRepo
	{
		private readonly AntiPlagiarismDb db;
		private readonly IServiceScopeFactory serviceScopeFactory;
		private static ILog log => LogProvider.Get().ForContext(typeof(SnippetsRepo));

		public SnippetsRepo(AntiPlagiarismDb db, IServiceScopeFactory serviceScopeFactory)
		{
			this.db = db;
			this.serviceScopeFactory = serviceScopeFactory;
		}

		public async Task<Snippet> AddSnippetAsync(int tokensCount, SnippetType type, int hash)
		{
			var snippet = new Snippet
			{
				TokensCount = tokensCount,
				SnippetType = type,
				Hash = hash
			};
			db.Snippets.Add(snippet);
			await db.SaveChangesAsync().ConfigureAwait(false);
			return snippet;
		}

		public async Task<bool> IsSnippetExistsAsync(int tokensCount, SnippetType type, int hash)
		{
			return await db.Snippets.AnyAsync(s => s.TokensCount == tokensCount && s.SnippetType == type && s.Hash == hash);
		}

		public async Task<SnippetOccurence> AddSnippetOccurenceAsync(Submission submission, Snippet snippet, int firstTokenIndex, int submissionInfluenceLimitInMonths)
		{
			log.Info($"Сохраняю в базу информацию о сниппете {snippet} в решении #{submission.Id} в позиции {firstTokenIndex}");
			log.Info($"Ищу сниппет {snippet} в базе (или создаю новый)");
			var foundSnippet = await GetOrAddSnippetAsync(snippet).ConfigureAwait(false);
			log.Info($"Сниппет в базе имеет номер {foundSnippet.Id}");
			var snippetOccurence = new SnippetOccurence
			{
				SubmissionId = submission.Id,
				Snippet = foundSnippet,
				FirstTokenIndex = firstTokenIndex,
			};
			log.Info($"Добавляю в базу объект {snippetOccurence}");

			DisableAutoDetectChanges();

			/* ...and use non-async Add() here because of perfomance issues with async versions */
			db.SnippetsOccurences.Add(snippetOccurence);
			db.Entry(snippetOccurence).State = EntityState.Added;
			await db.SaveChangesAsync().ConfigureAwait(false);
			db.Entry(snippetOccurence).State = EntityState.Unchanged;

			EnableAutoDetectChanges();

			log.Info($"Добавил. Пересчитываю статистику сниппета (количество авторов, у которых он встречается)");
			var snippetStatistics = await GetOrAddSnippetStatisticsAsync(foundSnippet, submission.TaskId, submission.ClientId, submission.Language);

			DisableAutoDetectChanges();

			log.Info($"Старая статистика сниппета {foundSnippet}: {snippetStatistics}");
			/* Use non-async Add() here because of performance issues with async versions */
			var useSubmissionsFromDate = DateTime.Now.AddMonths(-submissionInfluenceLimitInMonths);
			snippetStatistics.AuthorsCount = await GetAuthorsCount(submission, foundSnippet, useSubmissionsFromDate);
			db.Entry(snippetStatistics).State = EntityState.Modified;
			log.Info($"Количество авторов, у которых встречается сниппет {foundSnippet} — {snippetStatistics.AuthorsCount}");
			await db.SaveChangesAsync().ConfigureAwait(false);
			db.Entry(snippetStatistics).State = EntityState.Unchanged;

			EnableAutoDetectChanges();

			log.Info($"Закончил сохранение в базу информации о сниппете {foundSnippet} в решении #{submission.Id} в позиции {firstTokenIndex}");
			return snippetOccurence;
		}

		private async Task<int> GetAuthorsCount(Submission submission, Snippet foundSnippet, DateTime useSubmissionsFromDate)
		{
			using (var transaction = db.Database.BeginTransaction(IsolationLevel.ReadUncommitted))
			{
				var result = await db.SnippetsOccurences
					.Where(o => o.SnippetId == foundSnippet.Id &&
						o.Submission.ClientId == submission.ClientId &&
						o.Submission.TaskId == submission.TaskId &&
						o.Submission.AddingTime > useSubmissionsFromDate)
					.Select(o => o.Submission.AuthorId)
					.Distinct()
					.CountAsync();
				await transaction.CommitAsync();
				return result;
			}
		}

		private async Task<SnippetStatistics> GetOrAddSnippetStatisticsAsync(Snippet snippet, Guid taskId, int clientId, Language language)
		{
			DisableAutoDetectChanges();
			try
			{
				var foundStatistics = await db.SnippetsStatistics.SingleOrDefaultAsync(
					s => s.SnippetId == snippet.Id &&
						s.TaskId == taskId &&
						s.ClientId == clientId &&
						s.Language == language
				).ConfigureAwait(false);
				if (foundStatistics != null)
					return foundStatistics;

				var addedStatistics = db.SnippetsStatistics.Add(new SnippetStatistics
				{
					SnippetId = snippet.Id,
					ClientId = clientId,
					TaskId = taskId,
					Language = language,
				});
				addedStatistics.State = EntityState.Added;

				try
				{
					await db.SaveChangesAsync().ConfigureAwait(false);
					addedStatistics.State = EntityState.Unchanged;
				}
				catch (DbUpdateException e)
				{
					log.Warn(e, $"Возникла ошибка при добавлении объекта SnippetStatistics для сниппета {snippet}. Но это ничего, просто найду себе такой же в базе");
					/* It's ok: this statistics already exists */
					foundStatistics = await db.SnippetsStatistics.AsNoTracking().SingleOrDefaultAsync(
						s => s.SnippetId == snippet.Id &&
							s.TaskId == taskId &&
							s.ClientId == clientId
					).ConfigureAwait(false);
					if (foundStatistics != null)
						return foundStatistics;

					throw;
				}

				return addedStatistics.Entity;
			}
			finally
			{
				EnableAutoDetectChanges();
			}
		}

		/* We stands with perfomance issue on EF Core: https://github.com/aspnet/EntityFrameworkCore/issues/11680
  		   So we decided to disable AutoDetectChangesEnabled temporary for some queries */
		private void DisableAutoDetectChanges()
		{
			log.Debug("Выключаю AutoDetectChangesEnabled");
			db.DisableAutoDetectChanges();
		}

		private void EnableAutoDetectChanges()
		{
			log.Debug("Включаю AutoDetectChangesEnabled обратно");
			db.EnableAutoDetectChanges();
		}

		public async Task<Snippet> GetOrAddSnippetAsync(Snippet snippet)
		{
			DisableAutoDetectChanges();

			try
			{
				var foundSnippet = await db.Snippets.SingleOrDefaultAsync(
					s => s.SnippetType == snippet.SnippetType
						&& s.TokensCount == snippet.TokensCount
						&& s.Hash == snippet.Hash
				).ConfigureAwait(false);

				if (foundSnippet != null)
					return foundSnippet;

				db.Snippets.Add(snippet);
				db.Entry(snippet).State = EntityState.Added;
				try
				{
					await db.SaveChangesAsync().ConfigureAwait(false);
					db.Entry(snippet).State = EntityState.Unchanged;
					return snippet;
				}
				catch (DbUpdateException e)
				{
					log.Warn(e, $"Возникла ошибка при добавлении сниппета {snippet}. Но это ничего, просто найду себе такой же в базе");
					/* It's ok: this snippet already exists */
					foundSnippet = await db.Snippets.AsNoTracking().SingleOrDefaultAsync(
						s => s.SnippetType == snippet.SnippetType
							&& s.TokensCount == snippet.TokensCount
							&& s.Hash == snippet.Hash
					).ConfigureAwait(false);
					if (foundSnippet != null)
						return foundSnippet;

					throw;
				}
			}
			finally
			{
				EnableAutoDetectChanges();
			}
		}

		public async Task<List<SnippetOccurence>> GetSnippetsOccurrencesForSubmissionAsync(Submission submission, int maxCount, int authorsCountMinThreshold, int authorsCountMaxThreshold)
		{
			if (maxCount == 0)
				return await db.SnippetsOccurences.Where(o => o.SubmissionId == submission.Id).ToListAsync().ConfigureAwait(false);

			var selectedSnippetsStatistics = db.SnippetsStatistics.Where(s => s.TaskId == submission.TaskId && s.Language == submission.Language && s.ClientId == submission.ClientId);
			return (await db.SnippetsOccurences.Include(o => o.Snippet)
					.Join(selectedSnippetsStatistics, o => o.SnippetId, s => s.SnippetId, (occurence, statistics) => new { occurence, statistics })
					.Where(o => o.occurence.SubmissionId == submission.Id && o.statistics.AuthorsCount >= authorsCountMinThreshold && o.statistics.AuthorsCount <= authorsCountMaxThreshold)
					.OrderBy(o => o.statistics.AuthorsCount)
					.Take(maxCount)
					.ToListAsync()
					.ConfigureAwait(false))
				.Select(o => o.occurence)
				.ToList();
		}

		public async Task<List<SnippetOccurence>> GetSnippetsOccurrencesForSubmissionAsync(Submission submission)
		{
			return await GetSnippetsOccurrencesForSubmissionAsync(submission, 0, 0, int.MaxValue);
		}

		public async Task<Dictionary<int, SnippetStatistics>> GetSnippetsStatisticsAsync(int clientId, Guid taskId, Language language, IEnumerable<int> snippetsIds)
		{
			return await db.SnippetsStatistics
				.Where(s => s.ClientId == clientId && s.TaskId == taskId && s.Language == language && snippetsIds.Contains(s.SnippetId))
				.ToDictionaryAsync(s => s.SnippetId);
		}

		public async Task<List<SnippetOccurence>> GetSnippetsOccurrences(IEnumerable<int> snippetIds, Expression<Func<SnippetOccurence, bool>> filterFunction)
		{
			return await InternalGetSnippetsOccurrences(snippetIds, filterFunction).ToListAsync();
		}

		public async Task<List<int>> GetSubmissionIdsWithSameSnippets(IEnumerable<int> snippetIds, Expression<Func<SnippetOccurence, bool>> filterFunction, int maxSubmissionsCount)
		{
			List<int> submissionIdsForOccurences;
			using (var transaction = db.Database.BeginTransaction(IsolationLevel.ReadUncommitted))
			{
				// Если сделать GroupBy, то Where выполняется после Join, иначе до. Убрал GroupBy, т.к. Join большого числа строк приводил к тормозам.
				submissionIdsForOccurences = await db.SnippetsOccurences
					.Where(o => snippetIds.Contains(o.SnippetId))
					.Where(filterFunction)
					.Select(o => o.SubmissionId)
					.ToListAsync();
				await transaction.CommitAsync();
			}
			var submissionsWithSnippetsCount = submissionIdsForOccurences
				.GroupBy(id => id)
				.Select(g => new {g.Key, Value = g.Count()})
				.ToList();

			if (!submissionsWithSnippetsCount.Any())
				return new List<int>();

			/* Sort by occurrences count descendants */
			submissionsWithSnippetsCount.Sort((first, second) => second.Value.CompareTo(first.Value));
			var maxOccurrencesCount = submissionsWithSnippetsCount[0].Value;

			/* Take at least maxSubmissionsCount submissions, but also take submissions while it's occurencesCount is greater than 0.9 * maxOccurencesCount */
			return submissionsWithSnippetsCount.TakeWhile((kvp, index) => index < maxSubmissionsCount || kvp.Value >= 0.9 * maxOccurrencesCount).Select(kvp => kvp.Key).ToList();
		}

		private IQueryable<SnippetOccurence> InternalGetSnippetsOccurrences(IEnumerable<int> snippetIds, Expression<Func<SnippetOccurence, bool>> filterFunction)
		{
			return db.SnippetsOccurences
				.Include(o => o.Submission)
				.Where(o => snippetIds.Contains(o.SnippetId))
				.Where(filterFunction);
		}

		public async Task RemoveSnippetsOccurrencesForTaskAsync(Guid taskId)
		{
			db.SnippetsOccurences.RemoveRange(
				await db.SnippetsOccurences
					.Include(o => o.Submission)
					.Where(o => o.Submission.TaskId == taskId)
					.ToListAsync().ConfigureAwait(false)
			);
		}

		public async Task<List<Snippet>> GetSnippetsForSubmission(Submission submission, int maxCount, int authorsCountThreshold)
		{
			return (await GetSnippetsOccurrencesForSubmissionAsync(submission, maxCount, 0, authorsCountThreshold))
				.Select(o => o.Snippet).ToList();
		}

		public async Task UpdateOldSnippetsStatisticsAsync(DateTime from, DateTime to)
		{
			log.Info($"Start update old snippets statistics from {from.ToSortable()} to {to.ToSortable()}");
			var tasks = await db.Submissions
				.Where(s => s.AddingTime > from && s.AddingTime < to)
				.Select(s => new {s.ClientId, s.TaskId})
				.Distinct()
				.ToListAsync();
			log.Info($"Need update snippets statistics for {tasks.Count} tasks from {from.ToSortable()} to {to.ToSortable()}");

			foreach (var task in tasks)
			{
				var clientId = task.ClientId;
				var taskId = task.TaskId;
				await FuncUtils.TrySeveralTimesAsync(
					async () => await UpdateOldSnippetsStatisticsForTask(from, to, taskId, clientId, serviceScopeFactory),
					3);
			}
			log.Info($"Updated snippets statistics for {tasks.Count} tasks from {from.ToSortable()} to {to.ToSortable()}");
		}

		/// from может быть больше to. Тогда в authorsCount учтутся посылки начиная со времени to.
		/// Операция безопасна, т.к. таблица ЫnippetsStatistics полностью основана на данных других таблиц.
		/// Считает правильные значения, а не вычитает, так что можно запускать несколько раз.
		private static async Task UpdateOldSnippetsStatisticsForTask(DateTime from, DateTime to, Guid taskId, int clientId,
			IServiceScopeFactory serviceScopeFactory)
		{
			using (var scope = serviceScopeFactory.CreateScope())
			{
				var db = scope.ServiceProvider.GetService<AntiPlagiarismDb>();
				try
				{
					db.DisableAutoDetectChanges();
					var minDate = from > to ? to : from;
					var maxDate = from > to ? from : to;
					var snippetIdsInNonInfluencingSubmissions = db.SnippetsOccurences
						.Where(s => s.Submission.AddingTime > minDate && s.Submission.AddingTime <= maxDate
							&& s.Submission.TaskId == taskId && s.Submission.ClientId == clientId)
						.Select(ss => ss.SnippetId)
						.Distinct()
						.OrderBy(s => s);
					var snippetIdsInNonInfluencingSubmissionsList = await snippetIdsInNonInfluencingSubmissions.ToListAsync();
					var snippetsStatistics = await db.SnippetsStatistics
						.Where(s =>
							s.TaskId == taskId && s.ClientId == clientId
							&& snippetIdsInNonInfluencingSubmissions.Contains(s.SnippetId))
						.ToListAsync();
					log.Info($"Need update {snippetIdsInNonInfluencingSubmissionsList.Count} snippets statistics for task {taskId} from {from.ToSortable()} to {to.ToSortable()}");
					if (snippetIdsInNonInfluencingSubmissionsList.Count == 0)
						return;

					const int maxBatchSize = 20000;
					for (var i = 0; i < snippetIdsInNonInfluencingSubmissionsList.Count / maxBatchSize + (snippetIdsInNonInfluencingSubmissionsList.Count % maxBatchSize > 0 ? 1 : 0); i++)
					{
						var snippetIdsPart = snippetIdsInNonInfluencingSubmissionsList
							.Skip(i * maxBatchSize)
							.Take(Math.Min(maxBatchSize, snippetIdsInNonInfluencingSubmissionsList.Count - i * maxBatchSize))
							.ToHashSet();
						var snippetsStatisticsPart = snippetsStatistics
							.Where(ss => snippetIdsPart.Contains(ss.SnippetId))
							.ToList();
						await UpdateOldSnippetsStatisticsForTaskBatch(@from, to, taskId, clientId, db,
							snippetIdsInNonInfluencingSubmissions.Skip(i * maxBatchSize).Take(snippetIdsPart.Count),
							snippetsStatisticsPart,
							$"{i * maxBatchSize + snippetIdsPart.Count}/{snippetIdsInNonInfluencingSubmissionsList.Count}");
					}
				}
				finally
				{
					db.EnableAutoDetectChanges();
				}
			}
		}

		private static async Task UpdateOldSnippetsStatisticsForTaskBatch(DateTime @from, DateTime to, Guid taskId, int clientId,
			AntiPlagiarismDb db, IQueryable<int> snippetIdsInNonInfluencingSubmissions,
			List<SnippetStatistics> snippetsStatistics, string snippetsCountLog)
		{
			var snippet2AuthorsCount = (await db.SnippetsOccurences
					.Where(s =>
						s.Submission.AddingTime > to
						&& s.Submission.TaskId == taskId && s.Submission.ClientId == clientId
						&& snippetIdsInNonInfluencingSubmissions.Contains(s.SnippetId))
					.Select(s => new { s.SnippetId, s.Submission.AuthorId })
					.Distinct()
					.GroupBy(o => new { o.SnippetId })
					.Select(g => new { g.Key.SnippetId, AuthorsCount = g.Count() })
					.ToListAsync())
				.ToDictionary(p => p.SnippetId, p => p.AuthorsCount);
			log.Info($"Authors counted for {snippetsCountLog} snippets for task {taskId} from {@from.ToSortable()} to {to.ToSortable()}");

			await WriteNewAuthorsCounts(@from, to, taskId, db, snippetsStatistics, snippet2AuthorsCount);
		}

		private static async Task WriteNewAuthorsCounts(DateTime @from, DateTime to, Guid taskId, AntiPlagiarismDb db,
			List<SnippetStatistics> snippetsStatistics, Dictionary<int, int> snippet2AuthorsCount)
		{
			foreach (var snippetStatistics in snippetsStatistics)
			{
				// snippet2AuthorsCount не содержит ключа, если с момента to не было посылок c этим сниппетом
				snippetStatistics.AuthorsCount = snippet2AuthorsCount.GetValueOrDefault(snippetStatistics.SnippetId);
				db.Entry(snippetStatistics).State = EntityState.Modified;
			}

			await db.SaveChangesAsync();
			foreach (var snippetStatistics in snippetsStatistics)
				db.Entry(snippetStatistics).State = EntityState.Unchanged;
			log.Info($"New authors count are written for {snippetsStatistics.Count} snippets for task {taskId} from {@from.ToSortable()} to {to.ToSortable()}");
		}

		public async Task<OldSubmissionsInfluenceBorder> GetOldSubmissionsInfluenceBorderAsync()
		{
			return await db.OldSubmissionsInfluenceBorder.FirstOrDefaultAsync();
		}

		public async Task SetOldSubmissionsInfluenceBorderAsync(DateTime? oldSubmissionsInfluenceBorder)
		{
			var border = await db.OldSubmissionsInfluenceBorder.FirstOrDefaultAsync();
			if (border != null)
				border.Date = oldSubmissionsInfluenceBorder;
			else
				db.OldSubmissionsInfluenceBorder.Add(new OldSubmissionsInfluenceBorder { Date = oldSubmissionsInfluenceBorder });
			await db.SaveChangesAsync();
		}
	}

	internal static class DbErrors
	{
		/* Cannot insert duplicate key row in object, see https://msdn.microsoft.com/en-us/library/cc645728.aspx */
		public const int CanNotInsertDuplicateKeyRowInObject = 2601;
	}
}