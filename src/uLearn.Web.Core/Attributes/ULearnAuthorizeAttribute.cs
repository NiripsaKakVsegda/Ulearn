using Database.Models;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ulearn.Web.Core.Attributes;

// public class ULearnAuthorizeAttribute : AuthorizeAttribute
// {
// 	public CourseRoleType MinAccessLevel = CourseRoleType.Student;
// 	
// 	protected void HandleUnauthorizedRequest(AuthorizationHandlerContext filterContext)
// 	{
// 		var request = filterContext.Request;
// 		if (request.HttpMethod == "POST" && request.UrlReferrer != null)
// 		{
// 			filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", controller = "Login", returnUrl = request.UrlReferrer.PathAndQuery }));
// 			return;
// 		}
// 	
// 		base.HandleUnauthorizedRequest(filterContext);
// 	}
// 	
// 	protected override bool AuthorizeCore(HttpContextBase httpContext)
// 	{
// 		if (httpContext == null)
// 			throw new ArgumentNullException(nameof(httpContext));
// 	
// 		var user = httpContext.User;
// 		if (!user.Identity.IsAuthenticated)
// 			return false;
// 	
// 		var userId = httpContext.User.Identity.GetUserId();
// 		var usersRepo = new UsersRepo(new ULearnDb());
// 		if (usersRepo.FindUserById(userId) == null) // I.e. if user has been deleted
// 			return false;
// 	
// 		if (MinAccessLevel == CourseRoleType.Student && !ShouldBeSysAdmin)
// 			return true;
// 	
// 		if (ShouldBeSysAdmin && user.IsSystemAdministrator())
// 			return true;
// 	
// 		if (MinAccessLevel == CourseRoleType.Student)
// 			return false;
// 	
// 		var courseIds = httpContext.Request.Unvalidated.QueryString.GetValues("courseId");
// 		if (courseIds == null)
// 			return user.HasAccess(MinAccessLevel);
// 	
// 		if (courseIds.Length != 1)
// 			return false;
// 	
// 		return user.HasAccessFor(courseIds[0], MinAccessLevel);
// 	}
//
// 	public bool ShouldBeSysAdmin { get; set; }
//
// 	public new string Users
// 	{
// 		set { throw new NotSupportedException(); }
// 	}
//
// 	private static readonly char[] delims = { ',' };
//
// 	private static string[] SplitString(string original)
// 	{
// 		if (String.IsNullOrEmpty(original))
// 		{
// 			return new string[0];
// 		}
//
// 		return original
// 			.Split(delims)
// 			.Select(piece => piece.Trim())
// 			.Where(s => !String.IsNullOrEmpty(s))
// 			.ToArray();
// 	}
// }