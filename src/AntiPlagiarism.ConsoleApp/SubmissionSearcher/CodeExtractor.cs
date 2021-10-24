using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ulearn.Common;
using Ulearn.Common.Extensions;

namespace AntiPlagiarism.ConsoleApp
{
	public class CodeExtractor
	{
		public static List<string> ExtractCode(string path)
		{
			var languageToCodePieces = new Dictionary<Language, List<string>>();

			var dirs = new Stack<string>();
			dirs.Push(path);
			do
			{
				var dir = dirs.Pop();
				Directory.GetDirectories(dir).ForEach(d => dirs.Push(d));

				dir.GetFiles().ForEach(file =>
				{
					var language = LanguageHelpers.GuessByExtension(new FileInfo(file));
					if (!languageToCodePieces.ContainsKey(language))
						languageToCodePieces[language] = new List<string>();
					languageToCodePieces[language].Add(File.ReadAllText(file));
				});

			} while (dirs.Count > 0);
			
			
			return languageToCodePieces.Values
				.Select(codePieces => string.Join('\n', codePieces))
				.ToList();
		}
	}
}