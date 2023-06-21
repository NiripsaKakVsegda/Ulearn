using System.Diagnostics;

namespace CourseToolHotReloader.Infrastructure;

public static class Utils
{
	public static string BuildTempCourseUrl(string siteUrl, string baseCourseId, string userId) =>
		$"{siteUrl}/Course/{baseCourseId}_{userId}";

	public static Result<None> OpenInBrowser(string url) => Result.OfAction(
		() => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }),
		e => e.GetMessage()
	);
}