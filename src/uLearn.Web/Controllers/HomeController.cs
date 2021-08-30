using System.Web.Mvc;
using Database.DataContexts;
using Database.Models;
using Microsoft.AspNet.Identity;

namespace uLearn.Web.Controllers
{
	public class HomeController : Controller
	{
		private readonly UserManager<ApplicationUser> userManager = new ULearnUserManager(new ULearnDb());

		public ActionResult Index()
		{
			if (User.Identity.GetUserId() == null || ControllerUtils.HasPassword(userManager, User))
				return View();
			return RedirectToAction("Manage", "Account");
		}

		public ActionResult Terms()
		{
			return View();
		}
	}
}