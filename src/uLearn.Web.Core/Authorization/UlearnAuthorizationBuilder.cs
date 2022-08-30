using Castle.Core.Internal;
using Database.Models;
using Database.Repos;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Ulearn.Common.Extensions;
using uLearn.Web.Core.Authentication;
using uLearn.Web.Core.Utils;
using Vostok.Logging.Abstractions;
using Web.Api.Configuration;

namespace uLearn.Web.Core.Authorization;

public class UlearnAuthorizationBuilder : IConfigBuilder<AuthorizationOptions>
{
	public const string StudentsPolicyName = "Students";
	public const string InstructorsPolicyName = "Instructors";
	public const string CourseAdminsPolicyName = "CourseAdmins";
	public const string SysAdminsPolicyName = "SysAdmins";

	public void Build(WebConfiguration configuration, AuthorizationOptions options)
	{
		options.AddPolicy(StudentsPolicyName, AddDefaultAuthentication);
		options.AddPolicy(InstructorsPolicyName, policy =>
		{
			AddDefaultAuthentication(policy);
			policy.Requirements.Add(new CourseRoleRequirement(CourseRoleType.Instructor));
		});
		options.AddPolicy(CourseAdminsPolicyName, policy =>
		{
			AddDefaultAuthentication(policy);
			policy.Requirements.Add(new CourseRoleRequirement(CourseRoleType.CourseAdmin));
		});
		options.AddPolicy(SysAdminsPolicyName, policy =>
		{
			AddDefaultAuthentication(policy);
			policy.RequireRole(new List<string> { LmsRoleType.SysAdmin.GetDisplayName() });
		});

		foreach (var courseAccessType in Enum.GetValues(typeof(CourseAccessType)).Cast<CourseAccessType>())
		{
			var policyName = courseAccessType.GetAuthorizationPolicyName();
			options.AddPolicy(policyName, policy =>
			{
				AddDefaultAuthentication(policy);
				policy.Requirements.Add(new CourseAccessRequirement(courseAccessType));
			});
		}
	}

	private static void AddDefaultAuthentication(AuthorizationPolicyBuilder policy)
	{
		policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
		policy.AddAuthenticationSchemes(UlearnAuthenticationConstants.DefaultAuthenticationScheme);
		policy.RequireAuthenticatedUser();
	}
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