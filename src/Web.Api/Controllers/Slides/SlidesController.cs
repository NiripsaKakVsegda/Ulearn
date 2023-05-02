using System;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Groups;
using Database.Repos.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models.Common;
using Web.Api.Configuration;

namespace Ulearn.Web.Api.Controllers.Slides
{
	[Route("/slides")]
	public class SlidesController : BaseController
	{
		protected readonly ICoursesRepo coursesRepo;
		protected readonly ICourseRolesRepo courseRolesRepo;
		protected readonly IUserSolutionsRepo solutionsRepo;
		protected readonly IUserQuizzesRepo userQuizzesRepo;
		protected readonly IVisitsRepo visitsRepo;
		protected readonly IGroupsRepo groupsRepo;
		protected readonly IUnitsRepo unitsRepo;
		protected readonly IGroupMembersRepo groupMembersRepo;
		protected readonly IAdditionalContentPublicationsRepo additionalContentPublicationsRepo;
		protected readonly SlideRenderer slideRenderer;
		protected readonly WebApiConfiguration configuration;

		public SlidesController(ICourseStorage courseStorage, UlearnDb db, IUsersRepo usersRepo, ICourseRolesRepo courseRolesRepo,
			IUserSolutionsRepo solutionsRepo, IUserQuizzesRepo userQuizzesRepo, IVisitsRepo visitsRepo, IGroupsRepo groupsRepo,
			IGroupMembersRepo groupMembersRepo, IAdditionalContentPublicationsRepo additionalContentPublicationsRepo,
			SlideRenderer slideRenderer, ICoursesRepo coursesRepo, IUnitsRepo unitsRepo, IOptions<WebApiConfiguration> configuration)
			: base(courseStorage, db, usersRepo)
		{
			this.coursesRepo = coursesRepo;
			this.courseRolesRepo = courseRolesRepo;
			this.solutionsRepo = solutionsRepo;
			this.userQuizzesRepo = userQuizzesRepo;
			this.visitsRepo = visitsRepo;
			this.groupsRepo = groupsRepo;
			this.slideRenderer = slideRenderer;
			this.unitsRepo = unitsRepo;
			this.groupMembersRepo = groupMembersRepo;
			this.additionalContentPublicationsRepo = additionalContentPublicationsRepo;
			this.configuration = configuration.Value;
		}

		/// <summary>
		/// Информация о слайде
		/// </summary>
		[HttpGet("{courseId}/{slideId}")]
		public async Task<ActionResult<ApiSlideInfo>> SlideInfo([FromRoute] Course course, [FromRoute] Guid slideId)
		{
			if (course == null)
				return NotFound(new { status = "error", message = "Course not found" });

			var isInstructor = await courseRolesRepo.HasUserAccessToCourse(UserId, course.Id, CourseRoleType.Instructor);
			var isTester = await courseRolesRepo.HasUserAccessToCourse(UserId, course.Id, CourseRoleType.Tester);
			var visibleUnitsIds = await unitsRepo.GetVisibleUnitIds(course, UserId);
			var slide = course.FindSlideById(slideId, isInstructor, visibleUnitsIds);
			if (slide == null)
			{
				var instructorNote = course.FindInstructorNoteByIdNotSafe(slideId);
				if (instructorNote != null && isInstructor)
					slide = instructorNote;
			}

			if (slide == null)
				return NotFound(new { status = "error", message = "Slide not found" });

			var userId = User.GetUserId();

			if (!isTester && !await additionalContentPublicationsRepo.IsSlidePublishedForUser(course.Id, slide, userId))
				return NotFound(new { status = "error", message = "Slide not found" });

			var getSlideMaxScoreFunc = await BuildGetSlideMaxScoreFunc(solutionsRepo, userQuizzesRepo, visitsRepo, groupsRepo, course, userId);
			var getGitEditLinkFunc = await BuildGetGitEditLinkFunc(userId, course, courseRolesRepo, coursesRepo);
			var slideRenderContext = new SlideRenderContext(
				course.Id,
				slide,
				UserId,
				configuration.BaseUrlApi,
				configuration.BaseUrl,
				!isInstructor,
				course.Settings.VideoAnnotationsGoogleDoc,
				Url);

			return await slideRenderer.BuildSlideInfo(slideRenderContext, getSlideMaxScoreFunc, getGitEditLinkFunc);
		}
	}
}