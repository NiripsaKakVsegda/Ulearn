using Database.Repos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Mvc;
using uLearn.Web.Core.Models;

namespace uLearn.Web.Core.ViewComponents;

public class ExternalLoginsListPartial : ViewComponent
{
	private readonly UlearnUserManager userManager;

	public ExternalLoginsListPartial(UlearnUserManager userManager)
	{
		this.userManager = userManager;
	}

	public async Task<IViewComponentResult> InvokeAsync(ExternalLoginsListModel model)
	{
		if (User.Identity.IsAuthenticated)
		{
			var user = await userManager.FindByIdAsync(User.Identity.GetUserId());
			model.UserLogins = user.Logins.ToList();
		}

		return View("ExternalLoginsListPartial", model);
	}
}