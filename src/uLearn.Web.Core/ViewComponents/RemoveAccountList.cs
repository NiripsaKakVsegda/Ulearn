using Database.Repos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Mvc;
using uLearn.Web.Core.Controllers;

namespace uLearn.Web.Core.ViewComponents;

public class RemoveAccountList:ViewComponent
{
	private readonly UlearnUserManager userManager;
	
	public RemoveAccountList(UlearnUserManager userManager)
	{
		this.userManager = userManager;
	}
	
	public async Task<IViewComponentResult> InvokeAsync()
	{
		var user = await userManager.FindByIdAsync(User.Identity.GetUserId());
		var linkedAccounts = await userManager.GetLoginsAsync(user);

		ViewBag.User = user;
		ViewBag.ShowRemoveButton = await ControllerUtils.HasPassword(userManager, user.Id) || linkedAccounts.Count > 1;

		return View("_RemoveAccountPartial", linkedAccounts);
	}
}