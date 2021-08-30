using Microsoft.Owin.Security;

namespace uLearn.Web.Kontur.Passport
{
	public class KonturPassportAuthenticationOptions : AuthenticationOptions
	{
		public KonturPassportAuthenticationOptions(string authenticationType)
			: base(authenticationType)
		{
			Caption = authenticationType;
			ReturnEndpointPath = "/signin-kontur-passport";
			AuthenticationMode = AuthenticationMode.Passive;
		}

		public KonturPassportAuthenticationOptions()
			: this(KonturPassportConstants.AuthenticationType)
		{
		}

		public string ClientId { get; set; }

		public string Caption
		{
			get => Description.Caption;
			set => Description.Caption = value;
		}

		public string ReturnEndpointPath { get; private set; }
		public string SignInAsAuthenticationType { get; set; }

		public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }
	}
}