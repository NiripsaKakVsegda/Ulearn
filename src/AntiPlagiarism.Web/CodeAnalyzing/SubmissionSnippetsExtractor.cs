using System;
using System.Collections.Generic;
using AntiPlagiarism.Web.Configuration;
using AntiPlagiarism.Web.Database.Models;
using Microsoft.Extensions.Options;
using Vostok.Logging.Abstractions;
using Ulearn.Common.Extensions;

namespace AntiPlagiarism.Web.CodeAnalyzing
{
	public class SubmissionSnippetsExtractor
	{
		private readonly CodeUnitsExtractor codeUnitsExtractor;
		private readonly SnippetsExtractor snippetsExtractor;
		private static ILog log => LogProvider.Get().ForContext(typeof(SubmissionSnippetsExtractor));
		private readonly AntiPlagiarismConfiguration configuration;

		private readonly List<ITokenInSnippetConverter> tokenConverters = new List<ITokenInSnippetConverter>
		{
			new TokensKindsConverter(),
			new TokensValuesConverter(),
		};

		public SubmissionSnippetsExtractor(CodeUnitsExtractor codeUnitsExtractor, SnippetsExtractor snippetsExtractor,
			IOptions<AntiPlagiarismConfiguration> options)
		{
			this.codeUnitsExtractor = codeUnitsExtractor;
			this.snippetsExtractor = snippetsExtractor;
			configuration = options.Value;
		}

		public IEnumerable<Tuple<int, Snippet>> ExtractSnippetsFromSubmission(Submission submission)
		{
			log.Info("Достаю сниппеты из решения {submissionId}, длина сниппетов: {tokensCount} токенов", submission.Id, configuration.AntiPlagiarism.SnippetTokensCount);
			var codeUnits = codeUnitsExtractor.Extract(submission.ProgramText, submission.Language);
			foreach (var codeUnit in codeUnits)
			{
				foreach (var tokenConverter in tokenConverters)
				{
					var snippets = snippetsExtractor.GetSnippets(codeUnit.Tokens, configuration.AntiPlagiarism.SnippetTokensCount, tokenConverter);
					foreach (var (index, snippet) in snippets.Enumerate())
					{
						yield return Tuple.Create(codeUnit.FirstTokenIndex + index, snippet);
					}
				}
			}
		}
	}
}