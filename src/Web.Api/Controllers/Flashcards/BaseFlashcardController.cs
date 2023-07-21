using System;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Users;
using Microsoft.Extensions.Options;
using Ulearn.Core.Courses.Manager;
using Web.Api.Configuration;

namespace Ulearn.Web.Api.Controllers.Flashcards;

public abstract class BaseFlashcardController : BaseController
{
	protected readonly ICourseRolesRepo courseRolesRepo;
	protected readonly ICoursesRepo coursesRepo;
	protected readonly WebApiConfiguration configuration;

	protected BaseFlashcardController(
		ICourseStorage courseStorage,
		UlearnDb db,
		IUsersRepo usersRepo,
		ICourseRolesRepo courseRolesRepo,
		ICoursesRepo coursesRepo,
		IOptions<WebApiConfiguration> configuration
	)
		: base(courseStorage, db, usersRepo)
	{
		this.courseRolesRepo = courseRolesRepo;
		this.coursesRepo = coursesRepo;
		this.configuration = configuration.Value;
	}

	protected async Task<bool> CanUserModerateFlashcards(string courseId)
	{
		if (!IsAuthenticated)
			return false;
		if (await courseRolesRepo.HasUserAccessToCourse(UserId, courseId, CourseRoleType.Instructor))
			return true;
		var access = await coursesRepo.FindCourseAccess(UserId, courseId, CourseAccessType.ModerateUserGeneratedFlashcards);
		if (access is null || !access.IsEnabled)
			return false;
		return access.GrantTime + configuration.StudentCourseAccesses.ExpiresIn > DateTime.Now;
	}
}