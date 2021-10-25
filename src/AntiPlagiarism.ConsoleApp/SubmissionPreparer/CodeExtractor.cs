using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ulearn.Common;
using Ulearn.Common.Extensions;

namespace AntiPlagiarism.ConsoleApp.SubmissionPreparer
{
	public class CodeExtractor
	{
		private readonly Language language;

		public CodeExtractor(Language language)
		{
			this.language = language;
		}
		
		public string ExtractCode(string submissionDirectoryPath)
		{
			var codePieces = new List<string>();

			var dirs = new Stack<string>();
			dirs.Push(submissionDirectoryPath);
			
			do
			{
				var dir = dirs.Pop();
				Directory.GetDirectories(dir).ForEach(d => dirs.Push(d));

				foreach (var file in dir.GetFiles())
				{
					if (language ==  LanguageHelpers.GuessByExtension(new FileInfo(file)))
						codePieces.Add(File.ReadAllText(file));
				}
			} while (dirs.Count > 0);
			
			return string.Join('\n', codePieces);
		}
	}
}