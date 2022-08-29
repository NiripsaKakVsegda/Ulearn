namespace uLearn.Web.Core.Utils;

public static class RouteConfig
{
	public static void RegisterRoutes(IRouteBuilder routes)
	{
		//routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
		routes.MapRoute(
			name: "Course.Courses",
			template: "Course/Courses",
			defaults: new { controller = "Course", action = "Courses" }
		);
		routes.MapRoute(
			name: "Course.Flashcards",
			template: "Course/{courseId}/flashcards",
			defaults: new { controller = "Spa", action = "IndexHtml" }
		);
		routes.MapRoute(
			name: "Course.Slide",
			template: "Course/{courseId}/{slideIndex}",
			defaults: new { controller = "Course", action = "Slide", slideIndex = -1 },
			constraints: new { slideIndex = @"-?\d+|" }
		);
		routes.MapRoute(
			name: "Course.SlideById",
			template: "Course/{courseId}/{slideId}",
			defaults: new { controller = "Course", action = "SlideById", slideId = "" },
			constraints: new { slideId = @"(.*_)?[{|\(]?[0-9A-F]{8}[-]?([0-9A-F]{4}[-]?){3}[0-9A-F]{12}[\)|}]?" }
		);
		routes.MapRoute(
			name: "Slide",
			template: "Slide/{slideId}",
			defaults: new { controller = "Course", action = "SlideById" }
		);
		routes.MapRoute(
			name: "Course",
			template: "Course/{courseId}/{action}/{slideIndex}",
			defaults: new { controller = "Course", action = "Slide", slideIndex = -1 }
		);
		routes.MapRoute(
			name: "Exercise.StudentZip",
			template: "Exercise/{courseId}/{slideId}/StudentZip/{*fileName}",
			defaults: new { controller = "Exercise", action = "StudentZip" },
			constraints: new { slideId = @"(.*_)?[{|\(]?[0-9A-F]{8}[-]?([0-9A-F]{4}[-]?){3}[0-9A-F]{12}[\)|}]?" }
		);
		routes.MapRoute(
			name: "Exercise.StepikStudentZip",
			template: "Exercise/StudentZip",
			defaults: new { controller = "Exercise", action = "StudentZip" }
		);
		routes.MapRoute(
			name: "Certificates",
			template: "CertificatesList",
			defaults: new { controller = "Certificates", action = "Index" }
		);
		routes.MapRoute(
			name: "Certificate",
			template: "Certificate/{certificateId}",
			defaults: new { controller = "Certificates", action = "CertificateById" }
		);
		routes.MapRoute(
			name: "CertificateFile",
			template: "Certificate/{certificateId}/{*path}",
			defaults: new { controller = "Certificates", action = "CertificateFile" }
		);
		routes.MapRoute(
			name: "CourseStaticFile",
			template: "Courses/{CourseId}/{*path}",
			defaults: new { controller = "StaticFiles", action = "CourseFile" }
		);
		
		/* For react application which is not able to proxy root url (/) in webpack devserver */
		routes.MapRoute(
			name: "CourseList",
			template: "/CourseList",
			defaults: new { controller = "Home", action = "Index" }
		);

		/* We should enumerate all controllers here.
		   Otherwise all new, react-based routes (i.e. /basicprogramming/groups) will be routed to unknown controller ("basicprogrammingcontroller"),
		   not to SpaController (next route below). */
		routes.MapRoute(
			name: "Default",
			template: "{controller}/{action}",
			defaults: new { controller = "Home", action = "Index" },
			constraints: new { controller = @"^(Account|Admin|Analytics|AntiPlagiarism|Certificates|Comments|Course|Errors|Exercise|Feed|Grader|Hint|Home|Login|Notifications|Questions|Quiz|RestorePassword|Runner|Sandbox|SlideNavigation|Stepik|Telegram|Visits|StaticFiles)$" }
		);

		// /* After all your routes */
		routes.MapRoute(
			name: "NotFound",
			template: "{*pathInfo}",
			defaults: new { controller = "Spa", action = "IndexHtml" }
		);
	}
}