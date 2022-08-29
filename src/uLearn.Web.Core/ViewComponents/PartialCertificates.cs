using Database.Models;
using Database.Repos;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Core.Courses.Manager;
using uLearn.Web.Core.Controllers;

namespace uLearn.Web.Core.ViewComponents;

public class PartialCertificates : ViewComponent
{
	private readonly ICertificatesRepo certificatesRepo;
	private readonly UlearnUserManager userManager;
	private readonly ICourseStorage courseStorage;

	public PartialCertificates(ICertificatesRepo certificatesRepo, UlearnUserManager userManager, ICourseStorage courseStorage)
	{
		this.certificatesRepo = certificatesRepo;
		this.userManager = userManager;
		this.courseStorage = courseStorage;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var user = await userManager.FindByNameAsync(User.Identity.Name);
		if (user == null)
			return View("ListPartial", new UserCertificatesViewModel
			{
				User = user,
				Certificates = new List<Certificate>(),
				CoursesTitles = new Dictionary<string, string>(),
			});

		var certificates = await certificatesRepo.GetUserCertificates(user.Id);
		var coursesTitles = courseStorage.GetCourses().ToDictionary(c => c.Id.ToLower(), c => c.Title);

		return View("ListPartial", new UserCertificatesViewModel
		{
			User = user,
			Certificates = certificates,
			CoursesTitles = coursesTitles,
		});
	}
}