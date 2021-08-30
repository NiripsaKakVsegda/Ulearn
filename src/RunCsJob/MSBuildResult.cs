using System;
using System.Linq;

namespace RunCsJob
{
	public class MSbuildResult
	{
		public bool Success;
		public string ErrorMessage;
		public string PathToExe;

		public override string ToString()
		{
			var lines = ErrorMessage.Split(new[] { "\r\n" }, StringSplitOptions.None);
			var friendlyMessage = string.Join("\r\n",
				lines.Where(line => !line.ToLower().Contains("\\csc.exe ")));
			return Success ? "Success" : friendlyMessage;
		}
	}
}