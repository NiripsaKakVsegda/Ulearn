using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Ulearn.Common.Extensions;

namespace RunCsJob
{
	internal static class RunningResultsExtensions
	{
		public static bool HasErrors(this ImmutableArray<Diagnostic> diagnostics)
		{
			return diagnostics.Count(d => d.DefaultSeverity == DiagnosticSeverity.Error) > 0;
		}

		public static string DumpCompilationOutput(this ImmutableArray<Diagnostic> diagnostics)
		{
			if (diagnostics.Length == 0)
			{
				return "";
			}

			var sb = new StringBuilder();
			var errors = diagnostics.Where(d => d.DefaultSeverity.IsOneOf(DiagnosticSeverity.Error, DiagnosticSeverity.Warning)).ToList();
			foreach (var error in errors)
			{
				sb.Append(error);
			}

			return sb.ToString();
		}
	}
}