using System.Text.RegularExpressions;

namespace Ulearn.Web.Api.Utils;

public static class StringUtils
{
	public static string RemoveSpacesDuplicates(string line) => Regex.Replace(line.Trim(), @"\s+", " ");
}