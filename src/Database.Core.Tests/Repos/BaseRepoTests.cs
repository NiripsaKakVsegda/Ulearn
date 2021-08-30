using System;
using System.Linq;
using System.Threading.Tasks;
using Database.Di;
using Database.Models;
using Database.Repos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Vostok.Logging.Abstractions;
using Ulearn.Common;
using Vostok.Logging.Microsoft;
using Z.EntityFramework.Plus;

namespace Database.Core.Tests.Repos
{
	public class BaseRepoTests
	{
		protected UlearnDb db;
		protected IServiceProvider serviceProvider;
		protected UlearnUserManager userManager;
		private static ILog log => LogProvider.Get().ForContext(typeof(BaseRepoTests));

		[SetUp]
		public virtual void SetUp()
		{
			var loggerFactory = new LoggerFactory().AddVostok(LogProvider.Get());
			db = CreateDbContext(loggerFactory);

			serviceProvider = ConfigureServices();

			userManager = serviceProvider.GetService<UlearnUserManager>();
			CreateInitialDataInDatabaseAsync().GetAwaiter().GetResult();
			CreateTestUsersAsync().GetAwaiter().GetResult();

			/* Configuring Z.EntityFramework.Plus for working with In-Memory database
			   See https://entityframework-plus.net/batch-delete for details. */
			BatchDeleteManager.InMemoryDbContextFactory = () => CreateDbContext(loggerFactory);

			/* Cache Manager is not working with In-Memory database.
			   See https://github.com/zzzprojects/EntityFramework-Plus/issues/391 for details. */
			QueryCacheManager.IsEnabled = false;
		}

		private IServiceProvider ConfigureServices()
		{
			var services = new ServiceCollection();

			services.AddSingleton(db);
			services.AddLogging(builder => builder.AddVostok(LogProvider.Get()));
			services.AddDatabaseServices(true);
			services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<UlearnDb>();

			return services.BuildServiceProvider();
		}

		protected static UlearnDb CreateDbContext(ILoggerFactory loggerFactory)
		{
			var optionsBuilder = new DbContextOptionsBuilder<UlearnDb>();
			optionsBuilder.UseLazyLoadingProxies();
			optionsBuilder.UseInMemoryDatabase("ulearn_test_database" + Guid.NewGuid());
			if (loggerFactory != null)
				optionsBuilder.UseLoggerFactory(loggerFactory);

			return new UlearnDb(optionsBuilder.Options);
		}

		protected async Task<ApplicationUser> CreateUserAsync(string userName, string password = null)
		{
			if (password == null)
				password = StringUtils.GenerateSecureAlphanumericString(10);

			var user = new ApplicationUser
			{
				UserName = userName,
				FirstName = userName,
				LastName = userName,
				Email = $"{userName}@test.ru"
			};
			var result = await userManager.CreateAsync(user, password).ConfigureAwait(false);
			if (!result.Succeeded)
				throw new InvalidOperationException($"Can't create user {userName} with password {password}:\n{string.Join("\n", result.Errors.Select(e => e.Description))}");

			log.Info($"User {userName} with password {password} successfully created");

			return await userManager.FindByNameAsync(userName).ConfigureAwait(false);
		}

		private async Task CreateInitialDataInDatabaseAsync()
		{
			var initialDataCreator = serviceProvider.GetService<InitialDataCreator>();
			await initialDataCreator.CreateRoles().ConfigureAwait(false);
		}

		private async Task CreateTestUsersAsync()
		{
			var result = await userManager.CreateAsync(TestUsers.Admin, TestUsers.AdminPassword).ConfigureAwait(false);
			if (!result.Succeeded)
				throw new InvalidOperationException($"Can't create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

			TestUsers.Admin = await userManager.FindByNameAsync(TestUsers.Admin.UserName).ConfigureAwait(false);
			log.Info($"Created user {TestUsers.Admin.UserName} with password {TestUsers.AdminPassword}, id = {TestUsers.Admin.Id}");
			await userManager.AddToRoleAsync(TestUsers.Admin, LmsRoleType.SysAdmin.ToString()).ConfigureAwait(false);
		}
	}

	public static class TestUsers
	{
		public static ApplicationUser Admin = new ApplicationUser
		{
			UserName = "admin",
			FirstName = "Super",
			LastName = "Administrator",
			Email = "admin@ulearn.me",
			Gender = Gender.Male,
			Registered = DateTime.Now,
		};

		public const string AdminPassword = "AdminPassword123!";
	}
}