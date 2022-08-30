using System.Security.Claims;
using ApprovalUtilities.Utilities;
using Castle.Core.Internal;
using Database;
using Database.Di;
using Database.Models;
using Database.Repos;
using Database.Repos.Users;
using ElmahCore;
using ElmahCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Ulearn.Core.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.FileProviders;
using Telegram.Bot;
using Ulearn.Core.Courses.Manager;
using uLearn.Web.Core.Controllers;
using uLearn.Web.Core.Utils;
using Vostok.Logging.Abstractions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Rewrite;
using Serilog;
using Ulearn.Common.Extensions;
using Ulearn.Core.Telegram;
using uLearn.Web.Core.Extensions;
using uLearn.Web.Core.External.Kontur;
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
		services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
		services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
		services.AddScoped<CertificateGenerator>();
		services.AddScoped<AdminController>();
		services.AddSingleton<IWebCourseManager, WebCourseManager>();

		ConfigureAuthServices(services);
		
		services.AddElmah(options =>
		{
			options.Notifiers.Add(new ElmahTelegram());
			options.OnPermissionCheck = context =>
			{
				var isAuthenticated = context.User.Identity.IsAuthenticated;
				var isSysAdmin = context.User.IsSystemAdministrator();

				if (!isSysAdmin && !isAuthenticated)
					context.Response.Redirect($"/login?returnUrl={context.Request.Path}");

				if (isAuthenticated && !isSysAdmin)
					context.Response.Redirect("/");

				return true;
			};
		});

		services
			.AddMvc(options =>
			{
				options.EnableEndpointRouting = false;
				FilterConfig.RegisterGlobalFilters(options.Filters, HostingEnvironment, Configuration);
			})
			.AddControllersAsServices();
	}

	public class SysAdminClaimsTransformation : IClaimsTransformation
	{
		private readonly IUsersRepo usersRepo;

		public SysAdminClaimsTransformation(IUsersRepo usersRepo)
		{
			this.usersRepo = usersRepo;
		}

		public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
		{
			var claimsIdentity = new ClaimsIdentity();
			if (await usersRepo.IsSystemAdministrator(principal.GetUserId()))
				claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, LmsRoleType.SysAdmin.ToString()));

			principal.AddIdentity(claimsIdentity);
			return await Task.FromResult(principal);
		}
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

		services.AddTransient<IClaimsTransformation, SysAdminClaimsTransformation>();

		services.ConfigureApplicationCookie(options =>
		{
			options.Cookie.Name = Configuration.Web.CookieName;
			options.ExpireTimeSpan = TimeSpan.FromDays(14);
			options.Cookie.Domain = Configuration.Web.CookieDomain;
			options.Cookie.SameSite = SameSiteMode.Lax;
			options.LoginPath = "/login";
		});

		services
			.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = "Identity.Application";
				options.DefaultScheme = "Identity.Application";
				options.DefaultChallengeScheme = "Identity.Application";
			})
			.AddCookie(options =>
			{
				options.LoginPath = "/login";
				options.LogoutPath = "/account/logout";
			})
			.AddVkontakte("Вконтакте", "Вконтакте", options =>
			{
				var vkAppId = Configuration.OldWebConfig["oauth.vk.appId"];
				var vkAppSecret = Configuration.OldWebConfig["oauth.vk.appSecret"];

				options.ClientId = vkAppId;
				options.ClientSecret = vkAppSecret;

				options.Fields.Add("sex");
				options.Fields.Add("photo_50");

				options.Events.OnCreatingTicket = context =>
				{
					var user = context.User;

					if (user.TryGetProperty("name", out var name))
						context.Identity.AddClaim(new Claim(ClaimTypes.GivenName, name.GetString()));

					if (user.TryGetProperty("surname", out var surname))
						context.Identity.AddClaim(new Claim(ClaimTypes.Surname, surname.GetString()));

					if (user.TryGetProperty("photo", out var photo))
						context.Identity.AddClaim(new Claim("AvatarUrl", photo.GetString()));

					if (user.TryGetProperty("email", out var email))
						context.Identity.AddClaim(new Claim(ClaimTypes.Email, email.GetString()));

					if (user.TryGetProperty("sex", out var sex))
						context.Identity.AddClaim(new Claim(ClaimTypes.Gender, sex.GetInt32().ToString()));

					return Task.CompletedTask;
				};
			})
			.AddGoogle(options =>
			{
				var clientId = Configuration.OldWebConfig["oauth.google.clientId"];
				var clientSecret = Configuration.OldWebConfig["oauth.google.clientSecret"];
				options.ClientId = clientId;
				options.ClientSecret = clientSecret;

				options.Scope.Add("profile");
				options.Events.OnCreatingTicket = context =>
				{
					var user = context.User;

					if (user.TryGetProperty("given_name", out var name))
						context.Identity.AddClaim(new Claim(ClaimTypes.GivenName, name.GetString()));

					if (user.TryGetProperty("family_name", out var surname))
						context.Identity.AddClaim(new Claim(ClaimTypes.Surname, surname.GetString()));

					if (user.TryGetProperty("email", out var email))
						context.Identity.AddClaim(new Claim(ClaimTypes.Email, email.GetString()));

					if (user.TryGetProperty("picture", out var picture))
						context.Identity.AddClaim(new Claim("AvatarUrl", picture.GetString()));

					return Task.CompletedTask;
				};
			});

		services
			.AddAuthentication(options =>
			{
				options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
			})
			.AddOpenIdConnect("Контур.Паспорт", "Контур.Паспорт", options =>
			{
				var passportAppId = Configuration.OldWebConfig["oicd.konturPassport.clientId"];
				var passportAppSecret = Configuration.OldWebConfig["oicd.konturPassport.clientSecret"];
				var passportAuthority = Configuration.OldWebConfig["oicd.konturPassport.authority"];
				var returnUrl = Configuration.OldWebConfig["oicd.konturPassport.returnUrl"];
		
				options.Authority = passportAuthority;
				options.ClientId = passportAppId;
				options.ClientSecret = passportAppSecret;
				options.CallbackPath = returnUrl;
		
				options.Scope.AddAll(new[] { "openid", "profile", "email" });
		
				options.Events.OnTicketReceived = context =>
				{
					var user = context.Principal;
					var userClaims = user.Claims.ToList();
					const string xmlSchemaForStringType = "http://www.w3.org/2001/XMLSchema#string";
		
					log.Info($"Received follow user claims from Kontur.Passport server: {string.Join(", ", userClaims.Select(c => c.Type + ": " + c.Value))}");
		
					var login = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
					var sid = userClaims.FirstOrDefault(c => c.Type == KonturPassportConstants.SidClaimType)?.Value;
					var email = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
					var avatarUrl = userClaims.FirstOrDefault(c => c.Type == KonturPassportConstants.AvatarUrlClaimType)?.Value;
					var realNameParts = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value.Split(' ');
		
					var identity = new ClaimsIdentity();
					if (sid != null)
						identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, sid, xmlSchemaForStringType));
					if (email != null)
						identity.AddClaim(new Claim(ClaimTypes.Email, email, xmlSchemaForStringType));
					if (avatarUrl != null)
						identity.AddClaim(new Claim("AvatarUrl", avatarUrl, xmlSchemaForStringType));
					if (realNameParts is { Length: > 0 })
					{
						/* Suppose that Гейн Андрей Александрович is Surname (Гейн), GivenName (Андрей) and other. So we splitted name from Kontur.Passport into parts */
						identity.AddClaim(new Claim(ClaimTypes.Surname, realNameParts[0], xmlSchemaForStringType));
						if (realNameParts.Length > 1)
							identity.AddClaim(new Claim(ClaimTypes.GivenName, realNameParts[1], xmlSchemaForStringType));
					}
		
					/* Replace name from Kontur\andgein to andgein */
					if (login != null && login.Contains('\\'))
						login = login[(login.IndexOf('\\') + 1)..];
		
					if (login != null)
					{
						identity.AddClaim(new Claim(ClaimTypes.Name, login, xmlSchemaForStringType));
						identity.AddClaim(new Claim("KonturLogin", sid, xmlSchemaForStringType));
					}
		
					user.AddIdentity(identity);
		
					return Task.CompletedTask;
				};
			});

		services.AddScoped<IAuthorizationHandler, CourseRoleAuthorizationHandler>();
		services.AddScoped<IAuthorizationHandler, CourseAccessAuthorizationHandler>();

		services.AddAuthorization(options =>
		{
			options.AddPolicy("Students", policy =>
			{
				policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
				policy.AddAuthenticationSchemes("Identity.Application");
				policy.RequireAuthenticatedUser();
			});
			options.AddPolicy("Instructors", policy =>
			{
				policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
				policy.AddAuthenticationSchemes("Identity.Application");
				policy.RequireAuthenticatedUser();
				policy.Requirements.Add(new CourseRoleRequirement(CourseRoleType.Instructor));
			});
			options.AddPolicy("CourseAdmins", policy =>
			{
				policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
				policy.AddAuthenticationSchemes("Identity.Application");
				policy.RequireAuthenticatedUser();
				policy.Requirements.Add(new CourseRoleRequirement(CourseRoleType.CourseAdmin));
			});
			options.AddPolicy("SysAdmins", policy =>
			{
				policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
				policy.AddAuthenticationSchemes("Identity.Application");
				policy.RequireAuthenticatedUser();
				policy.RequireRole(new List<string> { LmsRoleType.SysAdmin.GetDisplayName() });
			});
		
			foreach (var courseAccessType in Enum.GetValues(typeof(CourseAccessType)).Cast<CourseAccessType>())
			{
				var policyName = courseAccessType.GetAuthorizationPolicyName();
				options.AddPolicy(policyName, policy =>
				{
					policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
					policy.AddAuthenticationSchemes("Identity.Application");
					policy.RequireAuthenticatedUser();
					policy.Requirements.Add(new CourseAccessRequirement(courseAccessType));
				});
			}
		});
		
		services.Configure<PasswordHasherOptions>(options => { options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2; });
	}

	public class ElmahTelegram : IErrorNotifierWithId
	{
		private ErrorsBot errorsBot;

		public string Name { get; }

		public ElmahTelegram()
		{
			Name = "ElmahTelegram";
			errorsBot = new ErrorsBot();
		}

		public void Notify(string id, Error error)
		{
			log.Error(error.Exception, $"Произошла ошибка {id} (код {error.StatusCode}, подробности в Elmah):\n" +
										$"Query string: {error.QueryString.ToQueryString()}"
			);

			if (!IsErrorIgnoredForTelegramChannel(error))
				errorsBot.PostToChannel(id, error.Exception);
		}

		public void Notify(Error error)
		{
		}

		private static readonly List<string> ignorableForTelegramChannelSubstrings = new()
		{
			"The provided anti-forgery token was meant for user",
			"The required anti-forgery cookie \"__RequestVerificationToken\" is not present.",
			"A potentially dangerous Request.Path value was detected from the client"
		};

		private static bool IsErrorIgnoredForTelegramChannel(Error error)
		{
			var message = error.Exception.Message;
			return ignorableForTelegramChannelSubstrings.Any(ignorableSubstring => message.Contains(ignorableSubstring));
		}
	}

	public void PushSeriLogProperties(IDiagnosticContext diagnosticContext, HttpContext httpContext)
	{
		//Get username  
		var username = httpContext.User.Identity.IsAuthenticated ? httpContext.User.Identity.Name : "(unknown user)";
		diagnosticContext.Set("User", username);

		//Get remote real IP address  
		const string xRealIpHeaderName = "X-Real-IP";
		var ip = httpContext.Request.Headers[xRealIpHeaderName].ToString();
		diagnosticContext.Set("IP", !string.IsNullOrWhiteSpace(ip) ? ip : "");
	}

	public void Configure(IApplicationBuilder app)
	{
		app.UseSerilogRequestLogging(options =>
		{
			options.EnrichDiagnosticContext = PushSeriLogProperties;
			options.MessageTemplate = "{User} {IP} HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
		});

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
		
		InitializeAllUtils(app);
	}

	private void InitializeAllUtils(IApplicationBuilder app)
	{
		var courseManager = app.ApplicationServices.GetService<IWebCourseManager>();

		InitTelegramBot(Configuration);
		InitializeCourses(courseManager);
	}

	public static void InitTelegramBot(WebConfiguration configuration)
	{
		var botToken = ApplicationConfiguration.Read<UlearnConfiguration>().Telegram.BotToken ?? "";
		if (string.IsNullOrEmpty(botToken))
			return;

		var telegramBot = new TelegramBotClient(botToken);
		var webhookSecret = configuration.OldWebConfig["ulearn.telegram.webhook.secret"] ?? "";
		var webhookDomain = configuration.OldWebConfig["ulearn.telegram.webhook.domain"] ?? "";
		var webhookUrl = $"https://{webhookDomain}/Telegram/Webhook?secret={webhookSecret}";
		try
		{
			telegramBot.SetWebhookAsync(webhookUrl).Wait();
		}
		catch (Exception ex)
		{
			log.Error(ex);
		}
	}

	private static void InitializeCourses(ICourseUpdater courseManager)
	{
		new UpdateCoursesWorker(courseManager).DoInitialCourseLoadAndRunCoursesUpdateInThreads();
	}

	public class CourseRoleRequirement : IAuthorizationRequirement
	{
		public readonly CourseRoleType minCourseRoleType;

		public CourseRoleRequirement(CourseRoleType minCourseRoleType)
		{
			this.minCourseRoleType = minCourseRoleType;
		}
	}

	public class CourseRoleAuthorizationHandler : BaseCourseAuthorizationHandler<CourseRoleRequirement>
	{
		private readonly ICourseRolesRepo courseRolesRepo;
		private readonly IUsersRepo usersRepo;
		private static ILog log => LogProvider.Get().ForContext(typeof(CourseRoleAuthorizationHandler));

		public CourseRoleAuthorizationHandler(ICourseRolesRepo courseRolesRepo, IUsersRepo usersRepo)
		{
			this.courseRolesRepo = courseRolesRepo;
			this.usersRepo = usersRepo;
		}

		protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CourseRoleRequirement requirement)
		{
			/* Get MVC context. See https://docs.microsoft.com/en-US/aspnet/core/security/authorization/policies#accessing-mvc-request-context-in-handlers */
			if (!(context.Resource is AuthorizationFilterContext mvcContext))
			{
				log.Error("Can't get MVC context in CourseRoleAuthenticationHandler");
				context.Fail();
				return;
			}

			var courseId = GetCourseIdFromRequestAsync(mvcContext);
			// if (string.IsNullOrEmpty(courseId))
			// {
			// 	context.Fail();
			// 	return;
			// }

			if (!context.User.Identity.IsAuthenticated)
			{
				context.Fail();
				return;
			}

			var userId = context.User.GetUserId();
			var user = await usersRepo.FindUserById(userId).ConfigureAwait(false);
			if (user == null)
			{
				context.Fail();
				return;
			}

			if (usersRepo.IsSystemAdministrator(user))
			{
				context.Succeed(requirement);
				return;
			}

			if (await courseRolesRepo.HasUserAccessToCourse(userId, courseId, requirement.minCourseRoleType).ConfigureAwait(false))
				context.Succeed(requirement);
			else
				context.Fail();
		}
	}

	public class BaseCourseAuthorizationHandler<T> : AuthorizationHandler<T> where T : IAuthorizationRequirement
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(BaseCourseAuthorizationHandler<T>));

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, T requirement)
		{
			throw new NotImplementedException();
		}

		/* Find `course_id` arguments in request. Try to get course_id in following order:
		 * route data (/groups/<course_id>/)
		 * query string (/groups/?course_id=<course_id>)
		 * NOTE: not supported JSON request body})
		 */
		protected string GetCourseIdFromRequestAsync(AuthorizationFilterContext mvcContext)
		{
			/* 1. Route data */
			var routeData = mvcContext.RouteData;
			if (routeData.Values["courseId"] is string courseIdFromRoute)
				return courseIdFromRoute;

			var courseIdFromQuery = mvcContext.HttpContext.Request.Query["courseId"].FirstOrDefault();
			if (!courseIdFromQuery.IsNullOrEmpty())
				return courseIdFromQuery;

			log.Error("Can't find `courseId` parameter in request for checking course role requirement. You should inherit your parameters models from ICourseAuthorizationParameters.");
			return null;
		}
	}

	public class CourseAccessRequirement : IAuthorizationRequirement
	{
		public readonly CourseAccessType CourseAccessType;

		public CourseAccessRequirement(CourseAccessType courseAccessType)
		{
			CourseAccessType = courseAccessType;
		}
	}

	public class CourseAccessAuthorizationHandler : BaseCourseAuthorizationHandler<CourseAccessRequirement>
	{
		private readonly ICoursesRepo coursesRepo;
		private readonly ICourseRolesRepo courseRolesRepo;
		private readonly IUsersRepo usersRepo;
		private static ILog log => LogProvider.Get().ForContext(typeof(CourseAccessAuthorizationHandler));

		public CourseAccessAuthorizationHandler(ICoursesRepo coursesRepo, ICourseRolesRepo courseRolesRepo, IUsersRepo usersRepo)
		{
			this.coursesRepo = coursesRepo;
			this.courseRolesRepo = courseRolesRepo;
			this.usersRepo = usersRepo;
		}

		protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CourseAccessRequirement requirement)
		{
			/* Get MVC context. See https://docs.microsoft.com/en-US/aspnet/core/security/authorization/policies#accessing-mvc-request-context-in-handlers */
			if (!(context.Resource is AuthorizationFilterContext mvcContext))
			{
				log.Error("Can't get MVC context in CourseRoleAuthenticationHandler");
				context.Fail();
				return;
			}

			var courseId = GetCourseIdFromRequestAsync(mvcContext);
			if (string.IsNullOrEmpty(courseId))
			{
				context.Fail();
				return;
			}

			if (!context.User.Identity.IsAuthenticated)
			{
				context.Fail();
				return;
			}

			var userId = context.User.GetUserId();
			var user = await usersRepo.FindUserById(userId).ConfigureAwait(false);
			if (user == null)
			{
				context.Fail();
				return;
			}

			if (usersRepo.IsSystemAdministrator(user))
			{
				context.Succeed(requirement);
				return;
			}

			var isCourseAdmin = await courseRolesRepo.HasUserAccessToCourse(userId, courseId, CourseRoleType.CourseAdmin).ConfigureAwait(false);
			if (isCourseAdmin || await coursesRepo.HasCourseAccess(userId, courseId, requirement.CourseAccessType).ConfigureAwait(false))
				context.Succeed(requirement);
			else
				context.Fail();
		}
	}

	public class CourseAccessAuthorizeAttribute : AuthorizeAttribute
	{
		public CourseAccessAuthorizeAttribute(CourseAccessType accessType)
			: base(accessType.GetAuthorizationPolicyName())
		{
		}
	}
}