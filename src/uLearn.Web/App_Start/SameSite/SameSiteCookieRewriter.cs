// https://github.com/blowdart/AspNetSameSiteSamples/blob/master/AspNet472CSharpMVC5/SameSiteCookieRewriter.cs

using System.Web;

namespace uLearn.Web.SameSite
{
	public class SameSiteCookieRewriter
	{
		public static void FilterSameSiteNoneForIncompatibleUserAgents(object sender)
		{
			HttpApplication application = sender as HttpApplication;
			if (application != null)
			{
				var userAgent = application.Context.Request.UserAgent;
				if (BrowserDetection.DisallowsSameSiteNone(userAgent))
				{
					application.Response.AddOnSendingHeaders(context =>
					{
						var cookies = context.Response.Cookies;
						for (var i = 0; i < cookies.Count; i++)
						{
							var cookie = cookies[i];
							if (cookie.SameSite == SameSiteMode.None)
							{
								cookie.SameSite = (SameSiteMode)(-1); // Unspecified
							}
						}
					});
				}
			}
		}
	}
}