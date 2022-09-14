using Database;
using Database.Models;
using Database.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Courses.Slides.Exercises;
using uLearn.Web.Core.Authorization;
using uLearn.Web.Core.Models;

namespace uLearn.Web.Core.Controllers;

[Authorize(Policy = UlearnAuthorizationBuilder.InstructorsPolicyName)] 
public class SandboxController : Controller
{
	private readonly IUserSolutionsRepo solutionsRepo;
	private readonly ICourseStorage courseStorage = WebCourseManager.CourseStorageInstance;

	public SandboxController(IUserSolutionsRepo solutionsRepo)
	{
		this.solutionsRepo = solutionsRepo;
	}

	public ActionResult Index(int max = 200, int skip = 0)
	{
		var submissions = solutionsRepo.GetAllSubmissions(max, skip).ToList();
		return View(new SubmissionsListModel
		{
			Submissions = submissions
		});
	}

	public ActionResult GetDetails(int id)
	{
		var submission = solutionsRepo.FindNoTrackingSubmission(id);

		if (submission == null)
			return NotFound();

		submission.SolutionCode.Text = ((ExerciseSlide)courseStorage
				.GetCourse(submission.CourseId)
				.GetSlideByIdNotSafe(submission.SlideId))
			.Exercise
			.GetSourceCode(submission.SolutionCode.Text);

		return View(submission);
	}
}