using Database.Repos;
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
			var user = await userManager.FindByNameAsync(User.Identity.Name);
			model.UserLogins = user.Logins.ToList();
		}

		return View("ExternalLoginsListPartial", model);
	}
}