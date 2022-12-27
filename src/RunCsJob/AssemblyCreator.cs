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
		static readonly PortableExecutableReference mscorlib;
		static readonly PortableExecutableReference system;
		static readonly PortableExecutableReference systemCore;
		static readonly PortableExecutableReference drawing;
		static readonly PortableExecutableReference runtime;

		static AssemblyCreator()
		{
			var directory = Directory.GetCurrentDirectory().PathCombine("OldAssembly");
			var mscorlibPath = directory.PathCombine("mscorlib.dll");
			var systemPath = directory.PathCombine("System.dll");
			var systemCorePath = directory.PathCombine("System.Core.dll");
			var drawingPath = directory.PathCombine("System.Drawing.dll");
			var runtimePath = directory.PathCombine("System.Runtime.dll");
			mscorlib = MetadataReference.CreateFromFile(mscorlibPath);
			system = MetadataReference.CreateFromFile(systemPath);
			systemCore = MetadataReference.CreateFromFile(systemCorePath);
			drawing = MetadataReference.CreateFromFile(drawingPath);
			runtime = MetadataReference.CreateFromFile(runtimePath);
		}

		public static CompileResult CreateAssemblyWithRoslyn(FileRunnerSubmission submission, string workingDirectory, TimeSpan compilationTimeLimit)
		{
			var syntaxTree = CSharpSyntaxTree.ParseText(submission.Code);
			var assemblyName = submission.Id;

			var compilation = CSharpCompilation.Create(
				assemblyName,
				new[] { syntaxTree },
				new MetadataReference[]
				{
					mscorlib,
					system,
					systemCore,
					drawing,
					runtime,

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