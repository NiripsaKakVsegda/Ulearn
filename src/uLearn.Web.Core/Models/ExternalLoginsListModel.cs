using Microsoft.AspNetCore.Identity;

namespace uLearn.Web.Core.Models;

public class ExternalLoginsListModel
{
	public ExternalLoginsListModel()
	{
		UserLogins = new List<IdentityUserLogin<string>>();
	}

	public string ActionName { get; set; }

	public string ReturnUrl { get; set; }

	public bool RememberMe { get; set; }

	public ExternalLoginListType Type { get; set; }

	public List<IdentityUserLogin<string>> UserLogins { get; set; }

	/* If empty, all providers are available */
	public List<string>? AvailableProviders { get; set; }
}

public enum ExternalLoginListType
{
	Registration,
	Login,
	Link
}