using Database;
using Database.Di;
using Database.Models;
using ElmahCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ulearn.Core.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.FileProviders;
using uLearn.Web.Core.Controllers;
using uLearn.Web.Core.Utils;
using Vostok.Logging.Abstractions;
using Serilog;
using uLearn.Web.Core.Authentication;
using uLearn.Web.Core.Authorization;
using uLearn.Web.Core.Start;
using Web.Api.Configuration;

namespace uLearn.Web.Core;

public class Startup
{
	private static ILog log => LogProvider.Get().ForContext(typeof(Startup));

	public WebConfiguration Configuration { get; }

	private IHostEnvironment HostingEnvironment { get; }

	public Startup(IHostEnvironment env)
	{
		HostingEnvironment = env;
		Configuration = ApplicationConfiguration.Read<WebConfiguration>();
	}

	public void ConfigureServices(IServiceCollection services)
	{
		services
			.AddDbContext<UlearnDb>(
				options => options
					.UseLazyLoadingProxies()
					.UseNpgsql(Configuration.Database, o => o.SetPostgresVersion(14, 4))
			);

		var cookieKeyRingDirectory = new DirectoryInfo(Path.Combine(Ulearn.Core.Utils.GetAppPath(), Configuration.Web.CookieKeyRingDirectory));
		services
			.AddDataProtection()
			.PersistKeysToFileSystem(cookieKeyRingDirectory)
			.SetApplicationName("ulearn");

		services.AddControllersWithViews();
		services.AddRazorPages();
		services.AddDatabaseServices(true);

		services
			.AddScoped<AuthenticationManager>();
		services
			.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, UserClaimsPrincipalFactory<ApplicationUser>>();
		services
			.AddScoped(_ => Configuration);

		//Allows to use Http.Action, see HtmlHelperViewExtensions.cs
		services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
		services.AddSingleton<IHttpContextAccessor, Extensions.HttpContextAccessor>();

		services.AddScoped<CertificateGenerator>();
		services.AddScoped<AdminController>();
		services.AddSingleton<IWebCourseManager, WebCourseManager>();

		ConfigureAuthServices(services);

		services.AddElmah(options => new ElmahBuilder().Build(Configuration, options));

		services
			.AddMvc(options =>
			{
				options.EnableEndpointRouting = false;
				FilterConfig.RegisterGlobalFilters(options.Filters, HostingEnvironment, Configuration);
			})
			.AddControllersAsServices();
	}

	public void ConfigureAuthServices(IServiceCollection services)
	{
		/* Configure sharing cookies between application.
			See https://docs.microsoft.com/en-us/aspnet/core/security/cookie-sharing?tabs=aspnetcore2x for details */
		services
			.AddDataProtection()
			.PersistKeysToFileSystem(new DirectoryInfo(Configuration.Web.CookieKeyRingDirectory))
			.SetApplicationName("ulearn");

		services.Configure<SecurityStampValidatorOptions>(options => { options.ValidationInterval = TimeSpan.FromSeconds(30); });

		services.AddCors();

		services.AddUlearnAuthentication(Configuration);
		services.AddUlearnAuthorization(Configuration);

		services.Configure<PasswordHasherOptions>(options => { options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2; });
	}

	public void Configure(IApplicationBuilder app)
	{
		app.UseSerilogRequestLogging(options => new SerilogBuilder().Build(Configuration, options));

		if (HostingEnvironment.IsDevelopment())
		{
			app.UseMigrationsEndPoint();
		}
		else
		{
			app.UseHsts();
		}

		app.UseCors(builder =>
		{
			builder
				.WithOrigins(Configuration.Web.Cors.AllowOrigins)
				.AllowAnyMethod()
				.WithHeaders("Authorization", "Content-Type", "Json-Naming-Strategy")
				.WithExposedHeaders("Location")
				.AllowCredentials();
		});

		app.UseHttpsRedirection();
		app.UseStaticFiles();
		app.UseStaticFiles(new StaticFileOptions
		{
			FileProvider = new PhysicalFileProvider(
				Path.Combine(HostingEnvironment.ContentRootPath, "Content")),
			RequestPath = "/content"
		});

		app.UseRouting();

		app.UseAuthentication();
		app.UseAuthorization();

		app.UseElmah();

		app.UseCookiePolicy(new CookiePolicyOptions
		{
			Secure = CookieSecurePolicy.Always
		});

		// app.UseLtiAuthentication();

		app.UseMvc(RouteConfig.RegisterRoutes);

		StartupUtils.InitializeAllUtils(app, Configuration);
	}
}