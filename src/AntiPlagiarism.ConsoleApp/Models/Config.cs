using System.Collections.Generic;
using Ulearn.Common;

namespace AntiPlagiarism.ConsoleApp.Models
{
	public class Config
	{
		public string Token;
		public string EndPointUrl = "http://localhost:33333/";
		public int MaxCodeLinesCount = 200;
		public int MaxInQuerySubmissionsCount = 3;
		public List<string> ExcludedPaths = new() { "node_modules", "diffs" };
		public List<Language> ExcludedLanguages = new() { Language.Text };
	}
}