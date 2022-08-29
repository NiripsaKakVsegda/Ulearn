using Database.Repos;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Extensions;

namespace uLearn.Web.Core.Controllers;

public class HomeController : Controller
{
	protected readonly UlearnUserManager userManager;

	public HomeController(UlearnUserManager userManager)
	{
		this.userManager = userManager;
	}

	public async Task<ActionResult> Index()
	{
		var userId = User.GetUserId();
		if (userId == null || await ControllerUtils.HasPassword(userManager, userId))
			return View();
		return RedirectToAction("Manage", "Account");
	}

	public ActionResult Terms()
	{
		return View();
	}
}