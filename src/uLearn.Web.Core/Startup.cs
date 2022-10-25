using System.Globalization;
using Database;
using Database.Di;
using Database.Models;
using ElmahCore;
using ElmahCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.FileProviders;
using uLearn.Web.Core.Controllers;
using uLearn.Web.Core.Utils;
using Serilog;
using uLearn.Web.Core.Authentication;
using uLearn.Web.Core.Authorization;
using uLearn.Web.Core.Extensions.LTI;
using uLearn.Web.Core.Start;
using uLearn.Web.Core.StartupConfigs;
using Vostok.Applications.AspNetCore;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Microsoft;
using Web.Api.Configuration;

namespace uLearn.Web.Core;

public class Startup : VostokAspNetCoreApplication
{
	private WebConfiguration configuration;
	private IHostEnvironment env;

	public override void Setup(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment hostingEnvironment)
	{
		builder
			.SetupWebHost(webHostBuilder =>
				webHostBuilder
					.UseKestrel(options => { options.Limits.MaxRequestBodySize = 160_000_000; })
					.ConfigureServices(s => ConfigureServices(s, hostingEnvironment))
					.UseEnvironment(hostingEnvironment.ApplicationIdentity.Environment)
					.Configure(ConfigureApp))
			.SetupLogging(s =>
			{
				s.LogRequests = true;
				s.LogResponses = true;
				s.LogRequestHeaders = false;
				s.LogResponseCompletion = true;
				s.LogResponseHeaders = false;
				s.LogQueryString = new LoggingCollectionSettings(_ => true);
			})
			.SetupGenericHost(host =>
				host.UseSerilog()
			)
			.SetupThrottling(b => b.DisableThrottling());
	}

	public void ConfigureServices(IServiceCollection services, IVostokHostingEnvironment hostingEnvironment)
	{
		configuration = hostingEnvironment.SecretConfigurationProvider.Get<WebConfiguration>(hostingEnvironment.SecretConfigurationSource);
		env = services.FirstOrDefault(s => s.ServiceType == typeof(IHostEnvironment)).ImplementationInstance as IHostEnvironment;

		services.AddScoped<WebConfiguration>(_ => configuration);

		services.AddLogging(builder => builder.AddVostok(hostingEnvironment.Log));

		services
			.AddDbContext<UlearnDb>(
				options => options
					.UseLazyLoadingProxies()
					.UseNpgsql(configuration.Database, o => o.SetPostgresVersion(13, 2))
			);

		var cookieKeyRingDirectory = new DirectoryInfo(Path.Combine(Ulearn.Core.Utils.GetAppPath(), configuration.Web.CookieKeyRingDirectory));
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

		//Allows to use Http.Action, see HtmlHelperViewExtensions.cs
		services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
		services.AddSingleton<IHttpContextAccessor, Extensions.HttpContextAccessor>();

		services.AddScoped<CertificateGenerator>();
		services.AddScoped<AdminController>();
		services.AddSingleton<IWebCourseManager, WebCourseManager>();

		services.AddSingleton<LtiAuthentication>();

		ConfigureAuthServices(services);

		services.AddElmah(options => new ElmahBuilder().Build(configuration, options));
		if (!string.IsNullOrEmpty(configuration.ElmahXmlLogPath))
			services.AddElmah<XmlFileErrorLog>(options => { options.LogPath = configuration.ElmahXmlLogPath; });

		services
			.AddMvc(options =>
			{
				options.EnableEndpointRouting = false;
				FilterConfig.RegisterGlobalFilters(options.Filters, env, configuration);
			})
			.AddControllersAsServices();
		
		var cultureInfo = new CultureInfo(configuration.Culture);
		CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
		CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
	}

	public void ConfigureAuthServices(IServiceCollection services)
	{
		/* Configure sharing cookies between application.
			See https://docs.microsoft.com/en-us/aspnet/core/security/cookie-sharing?tabs=aspnetcore2x for details */
		services
			.AddDataProtection()
			.PersistKeysToFileSystem(new DirectoryInfo(configuration.Web.CookieKeyRingDirectory))
			.SetApplicationName("ulearn");

		services.Configure<SecurityStampValidatorOptions>(options => { options.ValidationInterval = TimeSpan.FromSeconds(30); });

		services.AddCors();

		services.AddUlearnAuthentication(configuration);
		services.AddUlearnAuthorization(configuration);

		services.Configure<PasswordHasherOptions>(options => { options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2; });
	}

	public void ConfigureApp(IApplicationBuilder app)
	{
		var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

		app.UseSerilogRequestLogging(options => new SerilogBuilder().Build(configuration, options));

		if (env.IsDevelopment())
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
				.WithOrigins(configuration.Web.Cors.AllowOrigins)
				.AllowAnyMethod()
				.WithHeaders("Authorization", "Content-Type", "Json-Naming-Strategy")
				.WithExposedHeaders("Location")
				.AllowCredentials();
		});

		app.UseHttpsRedirection();
		app.UseStaticFiles();

		app.UseRouting();

		app.UseAuthentication();
		app.UseAuthorization();

		app.UseElmah();

		app.UseCookiePolicy(new CookiePolicyOptions
		{
			Secure = CookieSecurePolicy.Always
		});

		app.UseMvc(RouteConfig.RegisterRoutes);

		StartupUtils.InitializeAllUtils(app, configuration);
	}
}