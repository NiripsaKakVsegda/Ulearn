using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Provider;

namespace uLearn.Web.Microsoft.Owin.Security.VK.Provider
{
	public class VkReturnEndpointContext : ReturnEndpointContext
	{
		public VkReturnEndpointContext(
			IOwinContext context,
			AuthenticationTicket ticket)
			: base(context, ticket)
		{
		}
	}
}