using System;
using System.IO;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Ulearn.Common.Extensions;
using Ulearn.Core.RunCheckerJobApi;

namespace RunCsJob
{
	public static class AssemblyCreator
	{
		public static CompileResult CreateAssemblyWithRoslyn(FileRunnerSubmission submission, string workingDirectory, TimeSpan compilationTimeLimit)
		{
			var syntaxTree = CSharpSyntaxTree.ParseText(submission.Code);
			var assemblyName = submission.Id;

			var directory = Directory.GetCurrentDirectory().PathCombine("OldAssembly");
			var mscorlib = directory.PathCombine("mscorlib.dll");
			var system = directory.PathCombine("System.dll");
			var systemCore = directory.PathCombine("System.Core.dll");
			var drawing = directory.PathCombine("System.Drawing.dll");
			var runtime = directory.PathCombine("System.Runtime.dll");

			var compilation = CSharpCompilation.Create(
				assemblyName,
				new[] { syntaxTree },
				new MetadataReference[]
				{
					MetadataReference.CreateFromFile(mscorlib),
					MetadataReference.CreateFromFile(system),
					MetadataReference.CreateFromFile(systemCore),
					MetadataReference.CreateFromFile(drawing),
					MetadataReference.CreateFromFile(runtime),

					// MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // mscorlib
					// MetadataReference.CreateFromFile(typeof(Uri).Assembly.Location), // System
					// MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location), // System.Core
					// MetadataReference.CreateFromFile(typeof(Point).Assembly.Location), // System.Drawing,
					// MetadataReference.CreateFromFile(typeof(ValueType).Assembly.Location), // System.Runtime

					//MetadataReference.CreateFromFile(typeof(Regex).Assembly.Location), // System.Regex for dotnet 6
				},
				new CSharpCompilationOptions(OutputKind.ConsoleApplication));

			var assemblyFilename = Path.Combine(workingDirectory, assemblyName + ".exe");

			using (var cts = new CancellationTokenSource(compilationTimeLimit))
			{
				var startTime = DateTime.Now;
				var emitResult = compilation.Emit(assemblyFilename, cancellationToken: cts.Token);
				return new CompileResult(emitResult, assemblyFilename, DateTime.Now - startTime);
			}
		}
	}

	public class CompileResult
	{
		public readonly EmitResult EmitResult;
		public readonly string PathToAssembly;
		public readonly TimeSpan Elapsed;

		public CompileResult(EmitResult emitResult, string pathToAssembly, TimeSpan elapsed)
		{
			EmitResult = emitResult;
			PathToAssembly = pathToAssembly;
			Elapsed = elapsed;
		}
	}
}