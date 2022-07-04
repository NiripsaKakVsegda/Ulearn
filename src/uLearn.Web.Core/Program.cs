using Database;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ulearn.Core.Configuration;
using uLearn.Web.Core.Data;
using Web.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration
	.GetConnectionString("DefaultConnection");
var configuration = ApplicationConfiguration.Read<WebApiConfiguration>();

// var optionsBuilder = new DbContextOptionsBuilder<UlearnDb>()
// 	.UseLazyLoadingProxies()
// 	.UseNpgsql(configuration.Database, o => o.SetPostgresVersion(13, 2));
// var db = new UlearnDb(optionsBuilder.Options);
builder.Services
	.AddDbContext<ApplicationDbContext>(options =>
			options
				.UseNpgsql(configuration.Database, o => o.SetPostgresVersion(13, 2))
		// options
		// 	.UseSqlite(connectionString)
	);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
	.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
	.AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		// 	AuthenticationType = "Identity.Application",
		options.Cookie.Name = configuration.Web.CookieName;
		options.Cookie.Domain = configuration.Web.CookieDomain;
		options.Cookie.SameSite = configuration.Web.CookieSecure ? SameSiteMode.None : SameSiteMode.Lax;
		//options.CookieManager = configuration.Web.CookieSecure ? new SameSiteCookieManager(new SystemWebCookieManager()) : (ICookieManager)new SystemWebCookieManager()
		options.Cookie.SecurePolicy = configuration.Web.CookieSecure ? CookieSecurePolicy.Always : CookieSecurePolicy.None;
		options.LoginPath = new PathString("/Login");

		//options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
		//options.SlidingExpiration = true;
		//options.AccessDeniedPath = "/Forbidden/";

		//new CookieAuthenticationProvider
		// 	{
		// 		OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ULearnUserManager, ApplicationUser, string>(
		// 			validateInterval: TimeSpan.FromSeconds(30),
		// 			regenerateIdentityCallback: (manager, user) => user.GenerateUserIdentityAsync(manager),
		// 			getUserIdCallback: identity => identity.GetUserId()
		// 		)
		// 	},

		// 	/* Configure sharing cookies between application.
		// 		See https://docs.microsoft.com/en-us/aspnet/core/security/cookie-sharing?tabs=aspnetcore2x for details */
		// 	TicketDataFormat = new AspNetTicketDataFormat(
		// 		new DataProtectorShim(
		// 			DataProtectionProvider.Create(cookieKeyRingDirectory, builder => builder.SetApplicationName("ulearn"))
		// 				.CreateProtector(
		// 					"Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
		// 					"Identity.Application",
		// 					// DefaultAuthenticationTypes.ApplicationCookie,
		// 					"v2"))),
		// });
	});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();