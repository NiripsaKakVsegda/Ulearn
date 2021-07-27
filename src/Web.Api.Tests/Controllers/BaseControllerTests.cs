using System;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Vostok.Logging.Abstractions;
using Ulearn.Common;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api;
using Ulearn.Web.Api.Controllers;
using Vostok.Logging.Microsoft;
using Web.Api.Configuration;
using Z.EntityFramework.Plus;

namespace Web.Api.Tests.Controllers
{
	[TestFixture]
	public class BaseControllerTests
	{
		private WebApplication application;

		protected UlearnDb db;
		private static ILog log => LogProvider.Get().ForContext(typeof(BaseControllerTests));
		protected IServiceProvider serviceProvider;

		private readonly WebApiConfiguration fakeWebApiConfiguration = new WebApiConfiguration
		{
			Web = new UlearnWebConfiguration
			{
				Authentication = new AuthenticationConfiguration
				{
					Jwt = new JwtConfiguration
					{
						Audience = "ulearn.me",
						Issuer = "ulearn.me",
						IssuerSigningKey = "issuer-signing-key",
						LifeTimeHours = 24
					}
				},
				CookieKeyRingDirectory = "."
			}
		};

		public BaseControllerTests()
		{
			application = new WebApplication();
		}

		public async Task SetupTestInfrastructureAsync(Action<IServiceCollection> addServices = null)
		{
			var loggerFactory = new LoggerFactory().AddVostok(LogProvider.Get());
			db = CreateDbContext(loggerFactory);

			serviceProvider = ConfigureServices(addServices);

			var coursesDirectory = CourseManager.GetCoursesDirectory();
			if (coursesDirectory.Exists)
				coursesDirectory.Delete(true);

			await CreateInitialDataInDatabaseAsync().ConfigureAwait(false);
			await CreateTestUsersAsync().ConfigureAwait(false);

			/* Configuring Z.EntityFramework.Plus for working with In-Memory database
			   See https://entityframework-plus.net/batch-delete for details */
			BatchDeleteManager.InMemoryDbContextFactory = () => CreateDbContext(loggerFactory);

			/* Cache Manager is not working with In-Memory database.
			   See https://github.com/zzzprojects/EntityFramework-Plus/issues/391 for details. */
			QueryCacheManager.IsEnabled = false;
		}

		private async Task CreateInitialDataInDatabaseAsync()
		{
			var initialDataCreator = serviceProvider.GetService<InitialDataCreator>();
			await initialDataCreator.CreateRoles();
			await initialDataCreator.CreateUlearnBotUser();
			await initialDataCreator.AddExampleCourse();
		}

		private static UlearnDb CreateDbContext(ILoggerFactory loggerFactory)
		{
			var optionsBuilder = new DbContextOptionsBuilder<UlearnDb>();
			optionsBuilder.UseLazyLoadingProxies();
			optionsBuilder.UseInMemoryDatabase("ulearn_test_database" + Guid.NewGuid());
			if (loggerFactory != null)
				optionsBuilder.UseLoggerFactory(loggerFactory);

			return new UlearnDb(optionsBuilder.Options);
		}

		private IServiceProvider ConfigureServices(Action<IServiceCollection> addServices = null)
		{
			var services = new ServiceCollection();

			services.AddSingleton(db);
			services.AddLogging(builder => builder.AddVostok(LogProvider.Get()));
			application.ConfigureDi(services);
			application.ConfigureAuthServices(services, fakeWebApiConfiguration);
			application.ConfigureMvc(services);

			addServices?.Invoke(services);

			return services.BuildServiceProvider();
		}

		private async Task CreateTestUsersAsync(UlearnUserManager userManager)
		{
			var result = await userManager.CreateAsync(TestUsers.Admin, TestUsers.AdminPassword).ConfigureAwait(false);
			if (!result.Succeeded)
				throw new InvalidOperationException($"Can't create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
			TestUsers.Admin = await userManager.FindByNameAsync(TestUsers.Admin.UserName).ConfigureAwait(false);
			log.Info($"Created user {TestUsers.Admin.UserName} with password {TestUsers.AdminPassword}, id = {TestUsers.Admin.Id}");
			await userManager.AddToRoleAsync(TestUsers.Admin, LmsRoleType.SysAdmin.ToString()).ConfigureAwait(false);
			
			result = await userManager.CreateAsync(TestUsers.User, TestUsers.AdminPassword).ConfigureAwait(false);
			if (!result.Succeeded)
				throw new InvalidOperationException($"Can't create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
			TestUsers.User = await userManager.FindByNameAsync(TestUsers.User.UserName).ConfigureAwait(false);
			log.Info($"Created user {TestUsers.User.UserName} with password {TestUsers.AdminPassword}, id = {TestUsers.Admin.Id}");
		}

		private Task CreateTestUsersAsync()
		{
			var userManager = serviceProvider.GetService<UlearnUserManager>();
			return CreateTestUsersAsync(userManager);
		}

		protected async Task AuthenticateUserInControllerAsync(BaseController controller, ApplicationUser user)
		{
			var factory = serviceProvider.GetService<IUserClaimsPrincipalFactory<ApplicationUser>>();
			var admin = await factory.CreateAsync(user).ConfigureAwait(false);
			controller.ControllerContext.HttpContext.User = admin;
		}

		protected void ConfigureControllerForTests(BaseController controller)
		{
			var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
			var urlHelper = new Mock<IUrlHelper>();
			controller.Url = urlHelper.Object;
			controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
		}

		protected T GetController<T>() where T : BaseController
		{
			var controller = serviceProvider.GetService<T>();
			ConfigureControllerForTests(controller);
			return controller;
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

		public static ApplicationUser User = new ApplicationUser
		{
			UserName = "user",
			FirstName = "Simple",
			LastName = "User",
			Email = "user@ulearn.me",
			Gender = Gender.Female,
			Registered = DateTime.Now,
		};

		public const string AdminPassword = "AdminPassword123!";
	}
}