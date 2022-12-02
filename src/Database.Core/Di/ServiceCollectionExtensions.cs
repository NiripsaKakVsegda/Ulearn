using Database.Models;
using Database.Repos;
using Database.Repos.Comments;
using Database.Repos.Flashcards;
using Database.Repos.Groups;
using Database.Repos.SystemAccessesRepo;
using Database.Repos.Users;
using Database.Repos.Users.Search;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Ulearn.Core.Courses.Manager;

namespace Database.Di;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddDatabaseServices(this IServiceCollection services, bool withSlaveCourseManager)
	{
		if (withSlaveCourseManager)
		{
			services.AddSingleton(SlaveCourseManager.CourseStorageInstance);
			services.AddSingleton<ISlaveCourseManager, SlaveCourseManager>();
			services.AddSingleton<ICourseUpdater>(x => x.GetRequiredService<ISlaveCourseManager>());
		}

		services.AddScoped<UlearnUserManager>();
		services.AddScoped<InitialDataCreator>();

		services.AddIdentity<ApplicationUser, IdentityRole>(options =>
			{
				options.Password.RequireDigit = false;
				options.Password.RequiredLength = 6;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireUppercase = false;
				options.Password.RequireLowercase = false;
			})
			.AddEntityFrameworkStores<UlearnDb>()
			.AddDefaultTokenProviders();

		/* DI for database repos: */

		/* Groups */
		services.AddScoped<IGroupsRepo, GroupsRepo>();
		services.AddScoped<IGroupMembersRepo, GroupMembersRepo>();
		services.AddScoped<IGroupAccessesRepo, GroupAccessesRepo>();
		services.AddScoped<IManualCheckingsForOldSolutionsAdder, ManualCheckingsForOldSolutionsAdder>();
		services.AddScoped<IGroupsCreatorAndCopier, GroupsCreatorAndCopier>();
		services.AddScoped<IAdditionalScoresRepo, AdditionalScoresRepo>();
		services.AddScoped<IGroupsArchiver, GroupsArchiver>();

		/* Comments */
		services.AddScoped<ICommentsRepo, CommentsRepo>();
		services.AddScoped<ICommentLikesRepo, CommentLikesRepo>();
		services.AddScoped<ICommentPoliciesRepo, CommentPoliciesRepo>();

		/* Users */
		services.AddScoped<IUsersRepo, UsersRepo>();
		services.AddScoped<IUserSearcher, UserSearcher>();
		services.AddScoped<IAccessRestrictor, AccessRestrictor>();
		services.AddScoped<IFilter, FilterByCourseRole>();
		services.AddScoped<IFilter, FilterByLmsRole>();
		services.AddScoped<ISearcher, SearcherByUserId>();
		services.AddScoped<ISearcher, SearcherByNames>();
		services.AddScoped<ISearcher, SearcherByLogin>();
		services.AddScoped<ISearcher, SearcherByEmail>();
		services.AddScoped<ISearcher, SearcherBySocialLogin>();

		/*Flashcards*/
		services.AddScoped<IUsersFlashcardsVisitsRepo, UsersFlashcardsVisitsRepo>();
		services.AddScoped<IUserFlashcardsUnlockingRepo, UserFlashcardsUnlockingRepo>();

		/* Others */
		services.AddScoped<ICourseRolesRepo, CourseRolesRepo>();
		services.AddScoped<ICoursesRepo, CoursesRepo>();
		services.AddScoped<ITempCoursesRepo, TempCoursesRepo>();
		services.AddScoped<ISlideCheckingsRepo, SlideCheckingsRepo>();
		services.AddScoped<IUserSolutionsRepo, UserSolutionsRepo>();
		services.AddScoped<IUserQuizzesRepo, UserQuizzesRepo>();
		services.AddScoped<IVisitsRepo, VisitsRepo>();
		services.AddScoped<ITextsRepo, TextsRepo>();
		services.AddScoped<INotificationsRepo, NotificationsRepo>();
		services.AddScoped<IFeedRepo, FeedRepo>();
		services.AddScoped<ISystemAccessesRepo, SystemAccessesRepo>();
		services.AddScoped<IUnitsRepo, UnitsRepo>();
		services.AddScoped<ILtiConsumersRepo, LtiConsumersRepo>();
		services.AddScoped<ILtiRequestsRepo, LtiRequestsRepo>();
		services.AddScoped<IXQueueRepo, XQueueRepo>();
		services.AddScoped<IStyleErrorsRepo, StyleErrorsRepo>();
		services.AddScoped<IWorkQueueRepo, WorkQueueRepo>();
		services.AddScoped<ICertificatesRepo, CertificatesRepo>();
		services.AddScoped<IRestoreRequestRepo, RestoreRequestRepo>();
		services.AddScoped<IStepikRepo, StepikRepo>();
		services.AddScoped<IAcceptedSolutionsRepo, AcceptedSolutionsRepo>();
		services.AddScoped<IGoogleSheetExportTasksRepo, GoogleSheetExportTasksRepo>();
		services.AddScoped<IFavouriteReviewsRepo, FavouriteReviewsRepo>();
		services.AddScoped<IAdditionalContentPublicationsRepo, AdditionalContentPublicationsRepo>();
		services.AddScoped<IDeadLinesRepo, DeadLinesRepo>();
		services.AddScoped<ISelfCheckupsRepo, SelfCheckupsRepo>();

		return services;
	}
}