using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.Owin.Security;
using uLearn.Web.Microsoft.Owin.Security.VK.Provider;

namespace uLearn.Web.Microsoft.Owin.Security.VK
{
	public class VkAuthenticationOptions : AuthenticationOptions
	{
		[SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
			MessageId = "Microsoft.Owin.Security.VK.VkAuthenticationOptions.set_Caption(System.String)", Justification = "Not localizable.")]
		public VkAuthenticationOptions()
			: base(VkAuthenticationConstants.AuthenticationType)
		{
			Caption = VkAuthenticationConstants.AuthenticationType;
			ReturnEndpointPath = "/signin-vk";
			AuthenticationMode = AuthenticationMode.Passive;
			Scope = new List<string>();
			BackchannelTimeout = TimeSpan.FromSeconds(60);
		}

		public string AppId { get; set; }
		public string AppSecret { get; set; }

		/// <summary>
		/// Gets or sets the a pinned certificate validator to use to validate the endpoints used
		/// in back channel communications belong to Facebook.
		/// </summary>
		/// <value>
		/// The pinned certificate validator.
		/// </value>
		/// <remarks>If this property is null then the default certificate checks are performed,
		/// validating the subject name and if the signing chain is a trusted party.</remarks>
		public ICertificateValidator BackchannelCertificateValidator { get; set; }

		/// <summary>
		/// Gets or sets timeout value in milliseconds for back channel communications with Facebook.
		/// </summary>
		/// <value>
		/// The back channel timeout in milliseconds.
		/// </value>
		public TimeSpan BackchannelTimeout { get; set; }

		/// <summary>
		/// The HttpMessageHandler used to communicate with VK.
		/// This cannot be set at the same time as BackchannelCertificateValidator unless the value 
		/// can be downcast to a WebRequestHandler.
		/// </summary>
		public HttpMessageHandler BackchannelHttpHandler { get; set; }

		public string Caption
		{
			get => Description.Caption;
			set => Description.Caption = value;
		}

		public string ReturnEndpointPath { get; set; }
		public string SignInAsAuthenticationType { get; set; }

		public IVkAuthenticationProvider Provider { get; set; }
		public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }

		/// <summary>
		/// A list of permissions to request.
		/// </summary>
		public IList<string> Scope { get; private set; }
	}
}