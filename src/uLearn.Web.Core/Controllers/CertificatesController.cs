using Database;
using Database.Models;
using Database.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;
using uLearn.Web.Core.Extensions;
using uLearn.Web.Core.Utils;

namespace uLearn.Web.Core.Controllers;

public class CertificatesController : Controller
{
	private readonly ICertificatesRepo certificatesRepo;
	private readonly UlearnUserManager userManager;
	private readonly ICourseStorage courseStorage;
	private readonly CertificateGenerator certificateGenerator;

	public CertificatesController(
		ICourseStorage courseStorage,
		ICertificatesRepo certificatesRepo,
		UlearnUserManager userManager,
		CertificateGenerator certificateGenerator
	)
	{
		this.courseStorage = courseStorage;

		this.certificatesRepo = certificatesRepo;
		this.userManager = userManager;
		this.certificateGenerator = certificateGenerator;
	}

	[AllowAnonymous]
	public async Task<ActionResult> Index(string userId = "")
	{
		if (string.IsNullOrEmpty(userId) && User.Identity.IsAuthenticated)
			userId = User.GetUserId();

		if (string.IsNullOrEmpty(userId))
			return new NotFoundResult();

		var user = await userManager.FindByIdAsync(userId);
		if (user == null)
			return new NotFoundResult();

		var certificates = await certificatesRepo.GetUserCertificates(userId);
		var coursesTitles = courseStorage.GetCourses().ToDictionary(c => c.Id.ToLower(), c => c.Title);

		return View("List", new UserCertificatesViewModel
		{
			User = user,
			Certificates = certificates,
			CoursesTitles = coursesTitles,
		});
	}

	[Authorize]
	public async Task<ActionResult> Partial()
	{
		var userId = User.GetUserId();
		var user = await userManager.FindByIdAsync(userId);
		if (user == null)
			return new NotFoundResult();

		var certificates = await certificatesRepo.GetUserCertificates(userId);
		var coursesTitles = courseStorage.GetCourses().ToDictionary(c => c.Id.ToLower(), c => c.Title);

		return PartialView("ListPartial", new UserCertificatesViewModel
		{
			User = user,
			Certificates = certificates,
			CoursesTitles = coursesTitles,
		});
	}

	public async Task<ActionResult> CertificateById(Guid certificateId)
	{
		var redirect = this.GetRedirectToUrlWithTrailingSlash();
		if (redirect != null)
			return RedirectPermanent(redirect);

		var certificate = await certificatesRepo.FindCertificateById(certificateId);
		if (certificate == null)
			return new NotFoundResult();

		if (certificate.IsPreview && !User.HasAccessFor(certificate.Template.CourseId, CourseRoleType.Instructor))
			return new NotFoundResult();

		var course = courseStorage.GetCourse(certificate.Template.CourseId);

		certificateGenerator.EnsureCertificateTemplateIsUnpacked(certificate.Template);

		var certificateUrl = Url.RouteUrl("Certificate", new { certificateId = certificate.Id }, Request.GetRealScheme());
		var renderedCertificate = await certificateGenerator.RenderCertificate(certificate, course, certificateUrl);

		return View("Certificate", new CertificateViewModel
		{
			Course = course,
			Certificate = certificate,
			RenderedCertificate = renderedCertificate,
		});
	}

	public async Task<ActionResult> CertificateFile(Guid certificateId, string path)
	{
		var certificate = await certificatesRepo.FindCertificateById(certificateId);
		if (certificate == null)
			return new NotFoundResult();

		if (path.Contains(".."))
			return new NotFoundResult();

		return RedirectPermanent($"/Certificates/{certificate.Template.ArchiveName}/{path}");
	}
}

public class UserCertificatesViewModel
{
	public ApplicationUser User { get; set; }
	public List<Certificate> Certificates { get; set; }
	public Dictionary<string, string> CoursesTitles { get; set; }
}

public class CertificateViewModel
{
	public Course Course { get; set; }
	public Certificate Certificate { get; set; }
	public string RenderedCertificate { get; set; }
}