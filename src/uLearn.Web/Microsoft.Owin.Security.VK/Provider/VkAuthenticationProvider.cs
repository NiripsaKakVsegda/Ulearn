using System;
using System.Threading.Tasks;

namespace uLearn.Web.Microsoft.Owin.Security.VK.Provider
{
	public class VkAuthenticationProvider : IVkAuthenticationProvider
	{
		public VkAuthenticationProvider()
		{
			OnAuthenticated = context => Task.FromResult<object>(null);
			OnReturnEndpoint = context => Task.FromResult<object>(null);
		}

		public Func<VkAuthenticatedContext, Task> OnAuthenticated { get; set; }
		public Func<VkReturnEndpointContext, Task> OnReturnEndpoint { get; set; }

		public virtual Task Authenticated(VkAuthenticatedContext context)
		{
			return OnAuthenticated(context);
		}

		public virtual Task ReturnEndpoint(VkReturnEndpointContext context)
		{
			return OnReturnEndpoint(context);
		}
	}
}