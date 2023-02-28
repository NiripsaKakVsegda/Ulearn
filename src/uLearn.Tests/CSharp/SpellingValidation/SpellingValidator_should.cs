using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using FluentAssertions;
using NUnit.Framework;
using Ulearn.Common.Extensions;
using Ulearn.Core.CSharp.Validators.SpellingValidator;

namespace uLearn.CSharp.SpellingValidation
{
	[TestFixture]
	public class SpellingValidator_should
	{
		private static DirectoryInfo TestDataDir => new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..",
			"..", "CSharp", "SpellingValidation", "TestData"));
		
		[OneTimeSetUp]
		public void SetUp()
		{
			Approvals.RegisterDefaultNamerCreation(() => new RelativeUnitTestFrameworkNamer());
		}

		private static DirectoryInfo IncorrectTestDataDir => TestDataDir.GetDirectories("Incorrect").Single();
		private static DirectoryInfo CorrectTestDataDir => TestDataDir.GetDirectories("Correct").Single();

		private static IEnumerable<FileInfo> CorrectFiles => CorrectTestDataDir.EnumerateFiles();
		private static IEnumerable<FileInfo> IncorrectFiles => IncorrectTestDataDir.EnumerateFiles();

		private static DirectoryInfo BasicProgrammingDirectory => new(ExplicitTestsExamplesPaths.BasicProgrammingDirectoryPath);

		private static IEnumerable<FileInfo> BasicProgrammingFiles()
		{
			if (!BasicProgrammingDirectory.Exists)
				return new FileInfo[0];
			return BasicProgrammingDirectory
				.EnumerateFiles("*.cs", SearchOption.AllDirectories)
				.Where(f => !f.Name.Equals("Settings.Designer.cs") &&
							!f.Name.Equals("Resources.Designer.cs") &&
							!f.Name.Equals("AssemblyInfo.cs"));
		}

		private static DirectoryInfo ULearnSubmissionsDirectory => new(ExplicitTestsExamplesPaths.ULearnSubmissionsDirectoryPath);

		private static IEnumerable<FileInfo> SubmissionsFiles()
		{
			if (!ULearnSubmissionsDirectory.Exists)
				return new FileInfo[0];
			return ULearnSubmissionsDirectory
				.EnumerateFiles("*.cs", SearchOption.AllDirectories)
				.Where(f => f.Name.Contains("Accepted"));
		}

		private readonly SpellingValidator validator = new();

		[Test]
		[TestCaseSource(nameof(IncorrectFiles))]
		[UseReporter(typeof(DiffReporter))]
		public void FindErrors(FileInfo file)
		{
			var code = file.ContentAsUtf8();
			var errors = validator.FindErrors(code);
			var errorMessages = errors.Select(e => e.GetMessageWithPositions());

			using (ApprovalResults.ForScenario(file.Name))
			{
				Approvals.Verify(string.Join(Environment.NewLine, errorMessages));
			}
		}

		[TestCaseSource(nameof(CorrectFiles))]
		public void NotFindErrors(FileInfo file)
		{
			var code = file.ContentAsUtf8();
			var errors = validator.FindErrors(code);
			if (errors != null)
			{
				Console.WriteLine(errors);
			}

			errors.Should().BeNullOrEmpty();
		}

		[Explicit]
		[TestCaseSource(nameof(BasicProgrammingFiles))]
		public void NotFindErrors_InBasicProgramming(FileInfo file)
		{
			var fileContent = file.ContentAsUtf8();

			var errors = validator.FindErrors(fileContent);

			if (errors.Any())
			{
				var joinedErrors = string.Join(Environment.NewLine, errors.Select(err => err.GetMessageWithPositions()));
				File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..",
						"..", "CSharp", "ExampleFiles", "errors", "spelling_validation", $"{file.Name}_errors.txt"),
					$@"{fileContent}

{joinedErrors}");
				Assert.Fail();
			}
		}

		[Explicit]
		[TestCaseSource(nameof(SubmissionsFiles))]
		public void NotFindErrors_InCheckAcceptedFiles(FileInfo file)
		{
			var fileContent = file.ContentAsUtf8();

			var errors = validator.FindErrors(fileContent);
			if (errors.Any())
			{
				var joinedErrors = string.Join(Environment.NewLine, errors.Select(err => err.GetMessageWithPositions()));
				File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..",
						"..", "CSharp", "ExampleFiles", "submissions_errors", "spelling_validation", $"{file.Name}_errors.txt"),
					$@"{fileContent}

{joinedErrors}");
				Assert.Fail();
			}
		}
	}
}