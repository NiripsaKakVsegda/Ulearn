using System.Security.Claims;

namespace uLearn.Web.Core.External.Kontur;

public class AuthenticationResult
{
	public bool Authenticated;

	public IEnumerable<Claim> Claims;

	public string ErrorMessage;

	public bool IsError => !string.IsNullOrEmpty(ErrorMessage);
}