using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;

namespace uLearn.Web.Models
{
	public class ExternalLoginsListModel
	{
		public ExternalLoginsListModel()
		{
			UserLogins = new List<IdentityUserLogin>();
			AvailableProviders = new List<string>();
		}

		public string Action { get; set; }

		public string ReturnUrl { get; set; }

		public bool RememberMe { get; set; }

		public ExternalLoginListType Type { get; set; }

		public List<IdentityUserLogin> UserLogins { get; set; }

		/* If empty, all providers are available */
		public List<string> AvailableProviders { get; set; }
	}

	public enum ExternalLoginListType
	{
		Registration,
		Login,
		Link
	}
}