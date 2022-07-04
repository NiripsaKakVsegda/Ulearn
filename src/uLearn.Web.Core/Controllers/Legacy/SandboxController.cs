using System.Linq;
using System.Web.Mvc;
using Database;
using Database.DataContexts;
using Database.Models;
using Ulearn.Core.Courses.Manager;
using uLearn.Web.FilterAttributes;
using Ulearn.Core.Courses.Slides.Exercises;
using uLearn.Web.Core.Models;

namespace uLearn.Web.Controllers;

[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
public class SandboxController : Controller
{
	private readonly UserSolutionsRepo solutionsRepo;
	private readonly ICourseStorage courseStorage = WebCourseManager.CourseStorageInstance;

	public SandboxController()
	{
		solutionsRepo = new UserSolutionsRepo(new ULearnDb());
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
			return HttpNotFound();

		submission.SolutionCode.Text = ((ExerciseSlide)courseStorage
				.GetCourse(submission.CourseId)
				.GetSlideByIdNotSafe(submission.SlideId))
			.Exercise
			.GetSourceCode(submission.SolutionCode.Text);

		return View(submission);
	}
}