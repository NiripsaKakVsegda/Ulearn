using System.Threading.Tasks;

namespace uLearn.Web.Microsoft.Owin.Security.VK.Provider
{
	public interface IVkAuthenticationProvider
	{
		Task Authenticated(VkAuthenticatedContext context);
		Task ReturnEndpoint(VkReturnEndpointContext context);
	}
}