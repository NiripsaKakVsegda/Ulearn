using System.Net;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models.Parameters;
using Ulearn.Web.Api.Models.Responses;
using Web.Api.Configuration;

namespace Ulearn.Web.Api.Controllers;

[Route("/courses/{courseId}/accesses")]
public class CourseAccessesController : BaseController
{
	private readonly ICoursesRepo coursesRepo;
	private readonly ICourseRolesRepo courseRolesRepo;
	private readonly WebApiConfiguration configuration;

	public CourseAccessesController(
		ICourseStorage courseStorage,
		UlearnDb db,
		IUsersRepo usersRepo,
		ICoursesRepo coursesRepo,
		IOptions<WebApiConfiguration> options,
		ICourseRolesRepo courseRolesRepo
	)
		: base(courseStorage, db, usersRepo)
	{
		this.coursesRepo = coursesRepo;
		this.courseRolesRepo = courseRolesRepo;
		configuration = options.Value;
	}

	public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var courseId = (string)context.ActionArguments["courseId"];
		var parameters = (ToggleCourseAccessParameters)context.ActionArguments["parameters"];
		var user = await usersRepo.FindUserById(parameters.UserId);
		if (user is null)
		{
			context.Result = NotFound(new ErrorResponse($"User with id {parameters.UserId} not found!"));
			return;
		}

		var isStudentAccess = parameters.AccessType.IsStudentCourseAccess();
		if (!isStudentAccess && !await courseRolesRepo.HasUserAccessToCourse(UserId, courseId, CourseRoleType.CourseAdmin))
		{
			context.Result = StatusCode(
				(int)HttpStatusCode.Forbidden,
				new ErrorResponse($"Only course admins can toggle access {parameters.AccessType.ToString()}!")
			);
			return;
		}

		await base.OnActionExecutionAsync(context, next).ConfigureAwait(false);
	}

	[HttpPost]
	[Authorize(Policy = "Instructors")]
	[SwaggerResponse((int)HttpStatusCode.NotFound)]
	[SwaggerResponse((int)HttpStatusCode.Forbidden)]
	public async Task<ActionResult<CourseAccessResponse>> GrantAccess([FromRoute] string courseId, [FromBody] ToggleCourseAccessParameters parameters)
	{
		var isStudentAccess = parameters.AccessType.IsStudentCourseAccess();

		if (!isStudentAccess && !await courseRolesRepo.HasUserAccessToCourse(parameters.UserId, courseId, CourseRoleType.Instructor))
			return StatusCode(
				(int)HttpStatusCode.Forbidden,
				new ErrorResponse($"Access {parameters.AccessType.ToString()} can be granted only to instructors!")
			);

		var access = await coursesRepo.GrantAccess(
			courseId,
			parameters.UserId,
			parameters.AccessType,
			UserId,
			parameters.Comment
		);

		return new CourseAccessResponse
		{
			Id = access.Id,
			CourseId = access.CourseId,
			User = BuildShortUserInfo(access.User),
			GrantedBy = BuildShortUserInfo(access.GrantedBy),
			AccessType = access.AccessType,
			GrantTime = access.GrantTime,
			ExpiresOn = isStudentAccess
				? access.GrantTime + configuration.StudentCourseAccesses.ExpiresIn
				: null,
			Comment = access.Comment
		};
	}

	[HttpDelete]
	[Authorize(Policy = "Instructors")]
	[SwaggerResponse((int)HttpStatusCode.NotFound)]
	[SwaggerResponse((int)HttpStatusCode.Forbidden)]
	public async Task<ActionResult<CourseAccessResponse>> RevokeAccess([FromRoute] string courseId, [FromBody] ToggleCourseAccessParameters parameters)
	{
		await coursesRepo.RevokeAccess(
			courseId,
			parameters.UserId,
			parameters.AccessType,
			UserId,
			parameters.Comment
		);

		return NoContent();
	}
}