using Ulearn.Common;

namespace AntiPlagiarism.ConsoleApp.Models
{
	public class Config
	{
		public string Token;
		public string EndPointUrl = "http://localhost:33333/";
		public Language Language = Language.CSharp;
	}
}