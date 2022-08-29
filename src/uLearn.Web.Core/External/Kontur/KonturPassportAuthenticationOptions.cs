using Microsoft.AspNetCore.Authentication;

namespace uLearn.Web.Core.External.Kontur;

public class KonturPassportAuthenticationOptions
{
	public KonturPassportAuthenticationOptions(string authenticationType)
	{
		ReturnEndpointPath = "/signin-kontur-passport";
	}

	public KonturPassportAuthenticationOptions()
		: this(KonturPassportConstants.AuthenticationType)
	{
	}

	public string ClientId { get; set; }

	public string ReturnEndpointPath { get; private set; }
	public string SignInAsAuthenticationType { get; set; }

	public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }
}