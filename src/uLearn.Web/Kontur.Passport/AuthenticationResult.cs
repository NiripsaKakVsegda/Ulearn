using System.Collections.Generic;
using System.Security.Claims;

namespace uLearn.Web.Kontur.Passport
{
	public class AuthenticationResult
	{
		public bool Authenticated;

		public IEnumerable<Claim> Claims;

		public string ErrorMessage;

		public bool IsError => !string.IsNullOrEmpty(ErrorMessage);
	}
}