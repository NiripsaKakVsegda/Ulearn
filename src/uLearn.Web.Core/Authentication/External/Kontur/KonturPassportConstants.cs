using Database.Models;

namespace uLearn.Web.Core.Authentication.External.Kontur;

public static class KonturPassportConstants
{
	public const string AuthenticationType = LoginProviders.KonturPassport;
	public const string LoginClaimType = "sub";
	public const string SidClaimType = "id";
	public const string EmailClaimType = "email";
	public const string AvatarUrlClaimType = "picture";
	public const string NameClaimType = "name";
}