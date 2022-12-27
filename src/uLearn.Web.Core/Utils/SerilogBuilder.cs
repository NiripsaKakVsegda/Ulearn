using Serilog;
using Serilog.AspNetCore;
using Web.Api.Configuration;

namespace uLearn.Web.Core.Utils;

public class SerilogBuilder : IConfigBuilder<RequestLoggingOptions>
{
	public void Build(WebConfiguration configuration, RequestLoggingOptions options)
	{
		options.EnrichDiagnosticContext = PushSeriLogProperties;
		options.MessageTemplate = "{User} {IP} HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
	}

	private void PushSeriLogProperties(IDiagnosticContext diagnosticContext, HttpContext httpContext)
	{
		//Get username  
		var username = httpContext.User.Identity.IsAuthenticated ? httpContext.User.Identity.Name : "(unknown user)";
		diagnosticContext.Set("User", username);

		//Get remote real IP address  
		const string xRealIpHeaderName = "X-Real-IP";
		var ip = httpContext.Request.Headers[xRealIpHeaderName].ToString();
		diagnosticContext.Set("IP", !string.IsNullOrWhiteSpace(ip) ? ip : "");
	}
}