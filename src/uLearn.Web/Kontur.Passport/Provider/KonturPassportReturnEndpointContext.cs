using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Provider;

namespace uLearn.Web.Kontur.Passport.Provider
{
	public class KonturPassportReturnEndpointContext : ReturnEndpointContext
	{
		public KonturPassportReturnEndpointContext(IOwinContext context, AuthenticationTicket ticket)
			: base(context, ticket)
		{
		}
	}
}