using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ulearn.Common;
using Ulearn.Common.Extensions;

namespace AntiPlagiarism.ConsoleApp.SubmissionPreparer
{
	public class CodeExtractor
	{
		private readonly List<Language> allowedLanguages;

		public CodeExtractor(List<Language> allowedLanguages)
		{
			this.allowedLanguages = allowedLanguages;
		}
		
		public Dictionary<Language, string> ExtractCode(string submissionDirectoryPath)
		{
			var lang2CodePieces = new Dictionary<Language, List<string>>();

			var dirs = new Stack<string>();
			dirs.Push(submissionDirectoryPath);
			
			do
			{
				var dir = dirs.Pop();
				Directory.GetDirectories(dir).ForEach(d => dirs.Push(d));

				foreach (var file in dir.GetFiles())
				{
					var language = LanguageHelpers.GuessByExtension(new FileInfo(file));
					if (allowedLanguages.Contains(language))
					{
						if (!lang2CodePieces.ContainsKey(language))
							lang2CodePieces[language] = new List<string>();
						lang2CodePieces[language].Add(File.ReadAllText(file));
					}
				}
			} while (dirs.Count > 0);
			
			return lang2CodePieces.ToDictionary(kwp => kwp.Key,
				kwp => string.Join('\n', kwp.Value));
		}
	}
}