using Database.Repos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Mvc;
using uLearn.Web.Core.Controllers;
using uLearn.Web.Core.Models;

namespace uLearn.Web.Core.ViewComponents;

public class ChangeDetails : ViewComponent
{
	private readonly UlearnUserManager userManager;

	public ChangeDetails(UlearnUserManager userManager)
	{
		this.userManager = userManager;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var user = await userManager.FindByNameAsync(User.Identity?.Name);
		var hasPassword = await ControllerUtils.HasPassword(userManager, User.Identity.GetUserId());

		return View("ChangeDetailsPartial", new UserViewModel
		{
			Name = user.UserName,
			User = user,
			HasPassword = hasPassword,
			FirstName = user.FirstName,
			LastName = user.LastName,
			Email = user.Email,
			Gender = user.Gender,
		});
	}
}