using System.Collections.Generic;
using Ulearn.Common;

namespace AntiPlagiarism.ConsoleApp.Models
{
	public class Config
	{
		public string Token;
		public string EndPointUrl = "http://localhost:33333/";
		// todo запрашивать языки у пользователя
		public List<Language> Languages = new() { Language.CSharp, Language.Python3 };
	}
}