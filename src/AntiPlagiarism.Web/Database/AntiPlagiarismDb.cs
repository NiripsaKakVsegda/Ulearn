using AntiPlagiarism.Web.Database.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql.Logging;
using Ulearn.Core.Configuration;

namespace AntiPlagiarism.Web.Database
{
	public class AntiPlagiarismDb : DbContext
	{
		public static readonly string DefaultSchema = "antiplagiarism";

		static AntiPlagiarismDb()
		{
			var configuration = ApplicationConfiguration.Read<UlearnConfiguration>();
			if (configuration.HostLog != null)
			{
				NpgsqlLogManager.Provider = new AntiPlagiarismDbLoggingProvider();
				NpgsqlLogManager.IsParameterLoggingEnabled = true;
			}
		}

		public AntiPlagiarismDb(DbContextOptions<AntiPlagiarismDb> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasDefaultSchema(DefaultSchema);

			modelBuilder.Entity<MostSimilarSubmission>()
				.HasOne(e => e.SimilarSubmission)
				.WithMany()
				.HasForeignKey(e => e.SimilarSubmissionId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Restrict); // Introducing FOREIGN KEY constraint may cause cycles or multiple cascade paths, потому что две ссылки на одну таблицу

			modelBuilder.Entity<TaskStatisticsSourceData>()
				.HasOne(e => e.Submission2)
				.WithMany()
				.HasForeignKey(e => e.Submission2Id)
				.IsRequired()
				.OnDelete(DeleteBehavior.Restrict); // Introducing FOREIGN KEY constraint may cause cycles or multiple cascade paths, потому что две ссылки на одну таблицу

			modelBuilder.Entity<Client>()
				.HasIndex(c => c.Token)
				.IsUnique();
			modelBuilder.Entity<Client>()
				.HasIndex(c => new { c.Token, c.IsEnabled });

			var snippetOccurenceEntityBuilder = modelBuilder.Entity<SnippetOccurence>();
			snippetOccurenceEntityBuilder.HasIndex(c => new { c.SubmissionId, c.FirstTokenIndex });
			snippetOccurenceEntityBuilder.HasIndex(c => new { c.SubmissionId, c.SnippetId });
			snippetOccurenceEntityBuilder.HasIndex(c => new { c.SnippetId, c.SubmissionId });

			modelBuilder.Entity<Snippet>()
				.HasIndex(c => new { c.TokensCount, c.SnippetType, c.Hash })
				.IsUnique();

			modelBuilder.Entity<SnippetStatistics>()
				.HasIndex(c => new { c.SnippetId, c.TaskId, c.Language, c.ClientId })
				.IsUnique();

			var submissionEntityBuilder = modelBuilder.Entity<Submission>();
			submissionEntityBuilder.HasIndex(c => new { c.ClientId, c.TaskId, c.Language });
			submissionEntityBuilder.HasIndex(c => new { c.ClientId, c.TaskId, c.Language, c.AuthorId });
			submissionEntityBuilder.HasIndex(c => new { c.ClientId, c.TaskId, c.AddingTime, c.Language, c.AuthorId });
			submissionEntityBuilder.HasIndex(c => new { c.ClientId, c.ClientSubmissionId });
			submissionEntityBuilder.HasIndex(c => new { c.AddingTime });

			modelBuilder.Entity<WorkQueueItem>()
				.HasIndex(c => new { c.QueueId, c.TakeAfterTime })
				.IsUnique(false);

			modelBuilder.Entity<MostSimilarSubmission>()
				.HasIndex(c => new { c.Timestamp })
				.IsUnique(false);

			modelBuilder.Entity<TaskStatisticsParameters>()
				.HasKey(p => new { p.TaskId, p.Language });
			
			modelBuilder.Entity<ManualSuspicionLevels>()
				.HasKey(p => new { p.TaskId, p.Language });
		}

		public void MigrateToLatestVersion()
		{
			Database.Migrate();
		}

		/* We stands with perfomance issue on EF Core: https://github.com/aspnet/EntityFrameworkCore/issues/11680
  		   So we decided to disable AutoDetectChangesEnabled temporary for some queries */
		public void DisableAutoDetectChanges()
		{
			ChangeTracker.AutoDetectChangesEnabled = false;
		}

		public void EnableAutoDetectChanges()
		{
			ChangeTracker.AutoDetectChangesEnabled = true;
		}

		public DbSet<Client> Clients { get; set; }
		public DbSet<Submission> Submissions { get; set; }
		public DbSet<Code> Codes { get; set; }
		public DbSet<Snippet> Snippets { get; set; }
		public DbSet<SnippetStatistics> SnippetsStatistics { get; set; }
		public DbSet<SnippetOccurence> SnippetsOccurences { get; set; }
		public DbSet<TaskStatisticsParameters> TasksStatisticsParameters { get; set; }
		public DbSet<WorkQueueItem> WorkQueueItems { get; set; }
		public DbSet<TaskStatisticsSourceData> TaskStatisticsSourceData { get; set; }
		public DbSet<MostSimilarSubmission> MostSimilarSubmissions { get; set; }
		public DbSet<ManualSuspicionLevels> ManualSuspicionLevels { get; set; }
		public DbSet<OldSubmissionsInfluenceBorder> OldSubmissionsInfluenceBorder { get; set; }
	}
}