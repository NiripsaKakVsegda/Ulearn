using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;
using Ulearn.Common;
using Ulearn.Common.Extensions;

namespace AntiPlagiarism.Web.CodeAnalyzing
{
	public class TokensExtractor
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(TokensExtractor));

		private static IEnumerable<Token> FilterWhitespaceTokens(IEnumerable<Token> tokens)
		{
			return tokens.Where(t => !string.IsNullOrWhiteSpace(t.Value));
		}

		private static IEnumerable<Token> FilterCommentTokens(IEnumerable<Token> tokens)
		{
			return tokens.Where(t => !t.Type.StartsWith("Comment") || t.Type.StartsWith("Comment.Preproc"));
		}

		[NotNull]
		public List<Token> GetFilteredTokensFromPygmentize(string code, Language language)
		{
			var tokens = GetAllTokensFromPygmentize(code, language).EmptyIfNull();
			return FilterCommentTokens(FilterWhitespaceTokens(tokens)).ToList();
		}

		[CanBeNull]
		public List<Token> GetAllTokensFromPygmentize(string code, Language language)
		{
			var (codeWithNLineEndings, originalLineEndings) = PrepareLineEndingsForPygmentize(code);
			var pygmentizeResult = GetPygmentizeResult(codeWithNLineEndings, language);
			if (pygmentizeResult == null)
				return null;
			var tokensWithNLineEndings = ParseTokensFromPygmentize(pygmentizeResult).ToList();
			var tokens = ReturnOriginalLineEndings(tokensWithNLineEndings, originalLineEndings).ToList();
			SetPositions(tokens);
			return tokens;
		}

		private IEnumerable<Token> ParseTokensFromPygmentize(string pygmentizeResult)
		{
			var lines = pygmentizeResult.TrimEnd().SplitToLines();
			for (var i = 0; i < lines.Length; i++)
			{
				var line = lines[i];
				var parts = line.Split('\t', 2);
				var tokenType = parts[0];
				tokenType = tokenType.Substring("Token.".Length);
				var tokenContentInQuotes = parts[1];
				var tokenContentEscaped = tokenContentInQuotes.Substring(1, parts[1].Length - 2);
				var tokenContent = Regex.Unescape(tokenContentEscaped);
				var token = new Token
				{
					Type = tokenType,
					Value = tokenContent
				};
				yield return token;
			}
		}

		// Pygmentize заменяет в токенах конец строки на \n. Заменим сами на \n, а потом восстановим в токенах
		private static readonly Regex lineEndingsRegex = new Regex("\r\n|\n", RegexOptions.Compiled);
		private (string code, List<string> originalLineEndings) PrepareLineEndingsForPygmentize(string code)
		{
			var originalLineEndings = lineEndingsRegex.Matches(code).Select(m => m.Value).ToList();
			var resultCode = code.Replace("\r\n", "\n");
			return (resultCode, originalLineEndings);
		}

		private IEnumerable<Token> ReturnOriginalLineEndings(List<Token> tokens, List<string> originalLineEndings)
		{
			var lineNumber = 0;
			for (var i=0; i<tokens.Count; i++)
			{
				var token = tokens[i];
				if (token.Value == "\n")
				{
					// pygmentize добавляет токен перевода строки в конце кода. Убираю его, если исходно не было
					if (originalLineEndings.Count == lineNumber && i == tokens.Count - 1)
						yield break;
					token.Value = originalLineEndings[lineNumber];
					lineNumber++;
					yield return token;
					continue;
				}
				var parts = token.Value.Split('\n'); // Если \n в конце, последняя часть будет пустой строкой
				var sb = new StringBuilder();
				foreach (var part in parts.SkipLast(1))
				{
					sb.Append(part);
					// pygmentize добавляет токен перевода строки в конце кода. Убираю его, если исходно не было
					if (originalLineEndings.Count == lineNumber && i == tokens.Count - 1)
						yield break;
					sb.Append(originalLineEndings[lineNumber]);
					lineNumber++;
				}
				sb.Append(parts.Last());
				token.Value = sb.ToString();
				yield return token;
			}
		}

		public static void ThrowExceptionIfTokensNotMatchOriginalCode(string code, List<Token> tokens)
		{
			for (var i = 0; i < tokens.Count; i++)
			{
				var token = tokens[i];
				if (code.Substring(token.Position, token.Value.Length) != token.Value)
					throw new Exception();
			}
			var allTokensContentLength = tokens.Sum(t => t.Value.Length);
			if (code.Length != allTokensContentLength)
				throw new Exception();
		}

		private static void SetPositions(IEnumerable<Token> tokens)
		{
			var pos = 0;
			foreach (var token in tokens)
			{
				token.Position = pos;
				pos += token.Value.Length;
			}
		}

		private string GetPygmentizeResult(string code, Language language)
		{
			var lexer = language.GetAttribute<LexerAttribute>().Lexer;
			var arguments = lexer == null ? "-g" : $"-l {lexer}";
			arguments += " -f tokens -O encoding=utf-8";

			var sw = Stopwatch.StartNew();
			using (var process = BuildProcess(arguments))
			{
				process.Start();
				const int limit = 10 * 1024 * 1024;
				var utf8StandardErrorReader = new StreamReader(process.StandardError.BaseStream, Encoding.UTF8);
				var utf8StandardOutputReader = new StreamReader(process.StandardOutput.BaseStream, Encoding.UTF8);
				var readErrTask = new AsyncReader(utf8StandardErrorReader, limit).GetDataAsync();
				var readOutTask = new AsyncReader(utf8StandardOutputReader, limit).GetDataAsync();
				process.StandardInput.BaseStream.Write(Encoding.UTF8.GetBytes(code));
				process.StandardInput.BaseStream.Close();
				var isFinished = Task.WaitAll(new Task[] { readErrTask, readOutTask }, 1000);
				var ms = sw.ElapsedMilliseconds;

				if (!process.HasExited)
					Shutdown(process);

				if (readErrTask.Result.Length > 0)
					log.Warn($"pygmentize написал на stderr: {readErrTask.Result}");

				if (!isFinished)
					log.Warn($"Не хватило времени ({ms} ms) на работу pygmentize");
				else
					log.Info($"pygmentize закончил работу за {ms} ms");

				if (process.ExitCode != 0)
					log.Info($"pygmentize завершился с кодом {process.ExitCode}");

				return isFinished && readErrTask.Result.Length == 0 && readOutTask.Result.Length > 0
					? readOutTask.Result
					: null;
			}
		}

		private static void Shutdown(Process process)
		{
			try
			{
				process.Kill();
			}
			catch (Win32Exception)
			{
				/* Sometimes we can catch Access Denied error because the process is already terminating. It's ok, we don't need to rethrow exception */
			}
			catch (InvalidOperationException)
			{
				/* If process has already terminated */
			}

			var remainingTimeoutMs = 3000;
			while (!process.HasExited)
			{
				const int time = 10;
				Thread.Sleep(time);
				remainingTimeoutMs -= time;
				if (remainingTimeoutMs <= 0)
					throw new Exception($"process {process.Id} is not completed after kill");
			}
		}

		private static Process BuildProcess(string arguments)
		{
			return new Process
			{
				StartInfo =
				{
					Arguments = arguments,
					FileName = "pygmentize",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					RedirectStandardInput =  true,
					CreateNoWindow = true,
					UseShellExecute = false
				}
			};
		}
	}
}