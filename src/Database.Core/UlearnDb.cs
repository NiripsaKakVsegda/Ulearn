using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Database.Models;
using Database.Models.Quizzes;
using Database.Models.Comments;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql.Logging;
using Ulearn.Core.Configuration;

namespace Database
{
	public class UlearnDb : IdentityDbContext<ApplicationUser>
	{
		static UlearnDb()
		{
			var configuration = ApplicationConfiguration.Read<UlearnConfiguration>();
			if (configuration.HostLog != null)
			{
				NpgsqlLogManager.Provider = new UlearnDbLoggingProvider();
				NpgsqlLogManager.IsParameterLoggingEnabled = true;
			}

			AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
		}

		public UlearnDb(DbContextOptions<UlearnDb> options)
			: base(options)
		{
		}

		public void MigrateToLatestVersion()
		{
			Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
			Database.Migrate();
			Database.SetCommandTimeout(TimeSpan.FromSeconds(30));
		}

		public Task CreateInitialDataAsync(InitialDataCreator creator)
		{
			return creator.CreateAllAsync();
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.HasCollation("case_insensitive", locale: "und@colStrength=secondary", provider: "icu", deterministic: false);
			modelBuilder.UseDefaultColumnCollation("case_insensitive");

			modelBuilder.Entity<FavouriteReview>()
				.Property(u => u.Text)
				.UseCollation("default");
			// По Names будет осуществляться поиск по регулярном выражению. Такой поиск работает только с deterministic collation
			modelBuilder.Entity<ApplicationUser>()
				.Property(u => u.Names)
				.UseCollation("default");
			modelBuilder.Entity<ApplicationUser>()
				.Property(u => u.Names)
				.HasComputedColumnSql(@"lower(immutable_concat_ws(' ', nullif(""UserName"", ''), nullif(""FirstName"",''), nullif(""LastName"",''), nullif(""FirstName"",'')))", stored: true);
			// Индекс с триграммами. Ускоряет поиск в том числе по регулярному выражению, если в нем есть кусочки простого текста хотя бы на 3 символа.
			modelBuilder.Entity<ApplicationUser>()
				.HasIndex(p => p.Names)
				.HasMethod("gin")
				.HasOperators("gin_trgm_ops");

			/* IdentityUser.Id is guid in ASP.NET Core, so we can limit it by 64 chars.
			   If we will not do it, foreign keys to AspNetUsers.Id will fail in ASP.NET Core
			 */
			modelBuilder.Entity<ApplicationUser>(b => { b.Property(u => u.Id).HasMaxLength(64); });

			/* Customize the ASP.NET Identity model and override the defaults if needed.
			 * See https://docs.microsoft.com/en-us/aspnet/core/migration/1x-to-2x/identity-2x#add-identityuser-poco-navigation-properties
			 * for details */

			modelBuilder.Entity<ApplicationUser>()
				.HasMany(e => e.Claims)
				.WithOne()
				.HasForeignKey(e => e.UserId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<ApplicationUser>()
				.HasMany(e => e.Logins)
				.WithOne()
				.HasForeignKey(e => e.UserId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<ApplicationUser>()
				.HasMany(e => e.Roles)
				.WithOne()
				.HasForeignKey(e => e.UserId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);

			/* "If you don't want to expose a DbSet for one or more entities in the hierarchy, you can use the Fluent API to ensure they are included in the model".
			 * See https://stackoverflow.com/questions/37398141/ef7-migrations-the-corresponding-clr-type-for-entity-type-is-not-instantiab for details
			 */
			modelBuilder.Entity<MailNotificationTransport>();
			modelBuilder.Entity<TelegramNotificationTransport>();
			modelBuilder.Entity<FeedNotificationTransport>();

			var notificationClasses = GetNonAbstractSubclasses(typeof(Notification));
			foreach (var notificationClass in notificationClasses)
				modelBuilder.Entity(notificationClass);

			/* For backward compatibility with EF 6.0 */
			modelBuilder.Entity<ReceivedCommentToCodeReviewNotification>().Property(n => n.CommentId).HasColumnName("CommentId");
			modelBuilder.Entity<NewCommentNotification>().Property(n => n.CommentId).HasColumnName("CommentId1");
			modelBuilder.Entity<NewCommentForInstructorsOnlyNotification>().Property(n => n.CommentId).HasColumnName("CommentId1");
			modelBuilder.Entity<NewCommentFromYourGroupStudentNotification>().Property(n => n.CommentId).HasColumnName("CommentId1");
			modelBuilder.Entity<RepliedToYourCommentNotification>().Property(n => n.CommentId).HasColumnName("CommentId1");
			modelBuilder.Entity<LikedYourCommentNotification>().Property(n => n.CommentId).HasColumnName("CommentId1");
			modelBuilder.Entity<CreatedGroupNotification>().Property(n => n.GroupId).HasColumnName("GroupId1");
			modelBuilder.Entity<SystemMessageNotification>().Property(n => n.Text).HasColumnName("Text");
			modelBuilder.Entity<InstructorMessageNotification>().Property(n => n.Text).HasColumnName("Text1");
			modelBuilder.Entity<GroupMembersHaveBeenRemovedNotification>().Property(n => n.GroupId).HasColumnName("GroupId");
			modelBuilder.Entity<GroupMembersHaveBeenRemovedNotification>().Property(n => n.UserDescriptions).HasColumnName("UserDescriptions");
			modelBuilder.Entity<GroupMembersHaveBeenRemovedNotification>().Property(n => n.UserIds).HasColumnName("UserIds");
			modelBuilder.Entity<GroupMembersHaveBeenAddedNotification>().Property(n => n.GroupId).HasColumnName("GroupId");
			modelBuilder.Entity<GroupMembersHaveBeenAddedNotification>().Property(n => n.UserDescriptions).HasColumnName("UserDescriptions");
			modelBuilder.Entity<GroupMembersHaveBeenAddedNotification>().Property(n => n.UserIds).HasColumnName("UserIds");
			modelBuilder.Entity<PassedManualQuizCheckingNotification>().Property(n => n.CheckingId).HasColumnName("PassedManualQuizCheckingNotification_CheckingId");
			modelBuilder.Entity<JoinedToYourGroupNotification>().Property(n => n.GroupId).HasColumnName("JoinedToYourGroupNotification_GroupId");
			modelBuilder.Entity<RevokedAccessToGroupNotification>().Property(n => n.AccessId).HasColumnName("RevokedAccessToGroupNotification_AccessId");
			modelBuilder.Entity<GroupIsArchivedNotification>().Property(n => n.GroupId).HasColumnName("GroupIsArchivedNotification_GroupId");
			modelBuilder.Entity<UploadedPackageNotification>().Property(n => n.CourseVersionId).HasColumnName("UploadedPackageNotification_CourseVersionId");
			modelBuilder.Entity<NotUploadedPackageNotification>().Property(n => n.CommitHash).HasColumnName("NotUploadedPackageNotification_CommitHash");
			modelBuilder.Entity<NotUploadedPackageNotification>().Property(n => n.RepoUrl).HasColumnName("NotUploadedPackageNotification_RepoUrl");

			modelBuilder.Entity<CommentLike>()
				.HasOne(x => x.Comment)
				.WithMany(x => x.Likes)
				.HasForeignKey(x => x.CommentId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<GroupMember>()
				.HasOne(m => m.Group)
				.WithMany(g => g.Members)
				.HasForeignKey(m => m.GroupId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Like>()
				.HasOne(l => l.Submission)
				.WithMany(s => s.Likes)
				.HasForeignKey(l => l.SubmissionId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<NotificationDelivery>()
				.HasOne(d => d.Notification)
				.WithMany(n => n.Deliveries)
				.HasForeignKey(d => d.NotificationId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<ExerciseCodeReview>()
				.HasOne(s => s.ExerciseChecking)
				.WithMany(c => c.Reviews)
				.HasForeignKey(p => p.ExerciseCheckingId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<UserQuizSubmission>()
				.HasOne(s => s.AutomaticChecking)
				.WithOne(c => c.Submission)
				.HasForeignKey<AutomaticQuizChecking>(p => p.Id)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<UserQuizSubmission>()
				.HasOne(s => s.ManualChecking)
				.WithOne(c => c.Submission)
				.HasForeignKey<ManualQuizChecking>(p => p.Id)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<UserExerciseSubmission>()
				.HasOne(s => s.ManualChecking)
				.WithOne(c => c.Submission)
				.HasForeignKey<ManualExerciseChecking>(p => p.Id)
				.OnDelete(DeleteBehavior.Restrict);

			SetDeleteBehavior<CourseRole, ApplicationUser>(modelBuilder, r => r.User, r => r.UserId, DeleteBehavior.Cascade);
			SetDeleteBehavior<ReceivedCommentToCodeReviewNotification, ExerciseCodeReviewComment>(modelBuilder, c => c.Comment, c => c.CommentId, DeleteBehavior.Cascade);

			SetDeleteBehavior<ExerciseCodeReview, ApplicationUser>(modelBuilder, c => c.Author, c => c.AuthorId);

			SetDeleteBehavior<UserExerciseSubmission, ApplicationUser>(modelBuilder, c => c.User, c => c.UserId);
			SetDeleteBehavior<ManualExerciseChecking, ApplicationUser>(modelBuilder, c => c.User, c => c.UserId);

			SetDeleteBehavior<Certificate, ApplicationUser>(modelBuilder, c => c.User, c => c.UserId);
			SetDeleteBehavior<Certificate, ApplicationUser>(modelBuilder, c => c.Instructor, c => c.InstructorId);

			SetDeleteBehavior<AdditionalScore, ApplicationUser>(modelBuilder, c => c.User, c => c.UserId);
			SetDeleteBehavior<AdditionalScore, ApplicationUser>(modelBuilder, c => c.Instructor, c => c.InstructorId);

			SetDeleteBehavior<GraderClient, ApplicationUser>(modelBuilder, c => c.User, c => c.UserId);

			SetDeleteBehavior<ReceivedAdditionalScoreNotification, AdditionalScore>(modelBuilder, c => c.Score, c => c.ScoreId, DeleteBehavior.Cascade);

			SetDeleteBehavior<XQueueWatcher, ApplicationUser>(modelBuilder, c => c.User, c => c.UserId);

			SetDeleteBehavior<StepikExportProcess, ApplicationUser>(modelBuilder, c => c.Owner, c => c.OwnerId);

			SetDeleteBehavior<NotificationTransport, ApplicationUser>(modelBuilder, c => c.User, c => c.UserId);

			SetDeleteBehavior<GroupAccess, ApplicationUser>(modelBuilder, c => c.User, c => c.UserId, DeleteBehavior.Cascade);
			SetDeleteBehavior<GroupAccess, ApplicationUser>(modelBuilder, c => c.GrantedBy, c => c.GrantedById, DeleteBehavior.Cascade);

			SetDeleteBehavior<LabelOnGroup, SingleGroup>(modelBuilder, c => c.Group, c => c.GroupId);
			SetDeleteBehavior<GroupLabel, ApplicationUser>(modelBuilder, c => c.Owner, c => c.OwnerId);
			SetDeleteBehavior<LabelOnGroup, GroupLabel>(modelBuilder, c => c.Label, c => c.LabelId);

			SetDeleteBehavior<SystemAccess, ApplicationUser>(modelBuilder, c => c.GrantedBy, c => c.GrantedById);

			modelBuilder.Entity<ExerciseAttemptedUsersCount>()
				.ToView(ExerciseAttemptedUsersCount.ViewName)
				.HasNoKey();

			modelBuilder.Entity<ExerciseUsersWithRightAnswerCount>()
				.ToView(ExerciseUsersWithRightAnswerCount.ViewName)
				.HasNoKey();

			modelBuilder.Entity<DeadLine>(builder =>
			{
				if (!Database.IsNpgsql())
				{
					builder.Property(p => p.UserIds)
						.HasConversion(
							v => string.Join("'", v),
							v => v
								.Split(',', StringSplitOptions.RemoveEmptyEntries)
								.Select(Guid.Parse)
								.ToList());
				}
			});

			modelBuilder.Entity<GroupBase>()
				.HasDiscriminator(x => x.GroupType)
				.HasValue<SingleGroup>(GroupType.SingleGroup)
				.HasValue<SuperGroup>(GroupType.SuperGroup);
		}

		private static void SetDeleteBehavior<T1, T2>(ModelBuilder modelBuilder, Expression<Func<T1, T2>> oneWay, Expression<Func<T1, object>> secondWay, DeleteBehavior deleteBehavior = DeleteBehavior.Restrict)
			where T1 : class
			where T2 : class
		{
			modelBuilder.Entity<T1>()
				.HasOne(oneWay)
				.WithMany()
				.HasForeignKey(secondWay)
				.OnDelete(deleteBehavior);
		}

		private static List<Type> GetNonAbstractSubclasses(Type type)
		{
			return type.Assembly.GetTypes().Where(t => t.IsSubclassOf(type) && !t.IsAbstract && t != type).ToList();
		}

		public override int SaveChanges()
		{
			ValidateChanges();
			return base.SaveChanges();
		}

		public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new())
		{
			ValidateChanges();
			return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}

		private void ValidateChanges()
		{
			var entities = from e in ChangeTracker.Entries()
				where e.State == EntityState.Added
					|| e.State == EntityState.Modified
				select e.Entity;
			foreach (var entity in entities)
			{
				var validationContext = new ValidationContext(entity);
				Validator.ValidateObject(entity, validationContext);
			}
		}

		public DbSet<UserQuestion> UserQuestions { get; set; }
		public DbSet<SlideRate> SlideRates { get; set; }
		public DbSet<Visit> Visits { get; set; }
		public DbSet<LastVisit> LastVisits { get; set; }
		public DbSet<SlideHint> Hints { get; set; }
		public DbSet<Like> SolutionLikes { get; set; }
		public DbSet<UserQuizAnswer> UserQuizAnswers { get; set; }
		public DbSet<UnitAppearance> UnitAppearances { get; set; }
		public DbSet<TextBlob> Texts { get; set; }
		public DbSet<LtiConsumer> Consumers { get; set; }
		public DbSet<LtiSlideRequest> LtiRequests { get; set; }
		public DbSet<RestoreRequest> RestoreRequests { get; set; }
		public DbSet<CourseRole> CourseRoles { get; set; }

		public DbSet<Comment> Comments { get; set; }
		public DbSet<CommentLike> CommentLikes { get; set; }
		public DbSet<CommentsPolicy> CommentsPolicies { get; set; }

		public DbSet<CourseVersion> CourseVersions { get; set; }
		public DbSet<CourseVersionFile> CourseVersionFiles { get; set; }

		public DbSet<CourseGit> CourseGitRepos { get; set; }

		public DbSet<ManualExerciseChecking> ManualExerciseCheckings { get; set; }
		public DbSet<AutomaticExerciseChecking> AutomaticExerciseCheckings { get; set; }
		public DbSet<ManualQuizChecking> ManualQuizCheckings { get; set; }
		public DbSet<AutomaticQuizChecking> AutomaticQuizCheckings { get; set; }
		public DbSet<UserExerciseSubmission> UserExerciseSubmissions { get; set; }
		public DbSet<UserQuizSubmission> UserQuizSubmissions { get; set; }
		public DbSet<ExerciseCodeReview> ExerciseCodeReviews { get; set; }
		public DbSet<ExerciseCodeReviewComment> ExerciseCodeReviewComments { get; set; }

		public DbSet<GroupBase> Groups { get; set; }
		public DbSet<SingleGroup> SingleGroups { get; set; }
		public DbSet<SuperGroup> SuperGroups { get; set; }
		
		public DbSet<GroupMember> GroupMembers { get; set; }
		public DbSet<GroupLabel> GroupLabels { get; set; }
		public DbSet<LabelOnGroup> LabelsOnGroups { get; set; }
		public DbSet<GroupAccess> GroupAccesses { get; set; }

		public DbSet<CertificateTemplate> CertificateTemplates { get; set; }
		public DbSet<CertificateTemplateArchive> CertificateTemplateArchives { get; set; }
		public DbSet<Certificate> Certificates { get; set; }

		public DbSet<AdditionalScore> AdditionalScores { get; set; }
		public DbSet<EnabledAdditionalScoringGroup> EnabledAdditionalScoringGroups { get; set; }

		public DbSet<GraderClient> GraderClients { get; set; }
		public DbSet<ExerciseSolutionByGrader> ExerciseSolutionsByGrader { get; set; }

		public DbSet<NotificationTransport> NotificationTransports { get; set; }
		public DbSet<NotificationTransportSettings> NotificationTransportSettings { get; set; }
		public DbSet<NotificationDelivery> NotificationDeliveries { get; set; }
		public DbSet<Notification> Notifications { get; set; }

		// Без этого не работает Include в GetFeedNotificationDeliveriesAsync
		public DbSet<AbstractCommentNotification> AbstractCommentNotification { get; set; }
		public DbSet<AbstractCommentNotification> CourseExportedToStepikNotification { get; set; }
		public DbSet<AbstractCommentNotification> ReceivedCommentToCodeReviewNotification { get; set; }
		public DbSet<AbstractCommentNotification> PassedManualExerciseCheckingNotification { get; set; }
		public DbSet<AbstractCommentNotification> UploadedPackageNotification { get; set; }
		public DbSet<AbstractCommentNotification> PublishedPackageNotification { get; set; }
		public DbSet<AbstractCommentNotification> CreatedGroupNotification { get; set; }

		public DbSet<XQueueWatcher> XQueueWatchers { get; set; }
		public DbSet<XQueueExerciseSubmission> XQueueExerciseSubmissions { get; set; }

		public DbSet<FeedViewTimestamp> FeedViewTimestamps { get; set; }

		public DbSet<StepikAccessToken> StepikAccessTokens { get; set; }
		public DbSet<StepikExportProcess> StepikExportProcesses { get; set; }
		public DbSet<StepikExportSlideAndStepMap> StepikExportSlideAndStepMaps { get; set; }

		public DbSet<CourseAccess> CourseAccesses { get; set; }
		public DbSet<SystemAccess> SystemAccesses { get; set; }

		public DbSet<UserFlashcardsVisit> UserFlashcardsVisits { get; set; }
		public DbSet<UserGeneratedFlashcard> UserGeneratedFlashcards { get; set; }

		public DbSet<TempCourse> TempCourses { get; set; }

		public DbSet<TempCourseError> TempCourseErrors { get; set; }

		public DbSet<StyleErrorSettings> StyleErrorSettings { get; set; }

		public DbSet<WorkQueueItem> WorkQueueItems { get; set; }

		public DbSet<AcceptedSolutionsPromote> AcceptedSolutionsPromotes { get; set; }

		public DbSet<ExerciseAttemptedUsersCount> ExerciseAttemptedUsersCounts { get; set; }
		public DbSet<ExerciseUsersWithRightAnswerCount> ExerciseUsersWithRightAnswerCounts { get; set; }

		public DbSet<GoogleSheetExportTask> GoogleSheetExportTasks { get; set; }

		public DbSet<GoogleSheetExportTaskGroup> GoogleSheetExportTaskGroups { get; set; }

		public DbSet<FavouriteReview> FavouriteReviews { get; set; }
		public DbSet<FavouriteReviewByUser> FavouriteReviewsByUsers { get; set; }
		public DbSet<AdditionalContentPublication> AdditionalContentPublications { get; set; }
		public DbSet<DeadLine> DeadLines { get; set; }

		public DbSet<SelfCheckup> SelfCheckups { get; set; }
	}
}