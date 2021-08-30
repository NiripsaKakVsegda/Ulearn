using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualBasic.FileIO;
using NUnit.Framework;
using test;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.Courses.Units;
using Ulearn.Core.Helpers;

namespace uLearn.CSharp
{
	[TestFixture]
	public class CourseValidator_ReportError_should
	{
		private static readonly string courseDirectory = TestsHelper.TestDirectory;
		private static readonly string unitDirectoryPathRelativeToCourse = "ReportErrorTests_Temp_SlideFolder";
		private static readonly string unitDirectoryAbsolutePath = Path.Combine(courseDirectory, unitDirectoryPathRelativeToCourse);
		private static readonly DirectoryInfo unitDirectory = new DirectoryInfo(unitDirectoryAbsolutePath);

		private static CsProjectExerciseBlock exerciseBlock;
		private static CsProjectExerciseBlock.FilesProvider fp;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			TestsHelper.RecreateDirectory(unitDirectoryAbsolutePath);
		}

		[SetUp]
		public void SetUp()
		{
			FileSystem.CopyDirectory(TestsHelper.ProjSlideFolderPath, unitDirectoryAbsolutePath, true);

			exerciseBlock = new CsProjectExerciseBlock
			{
				StartupObject = "test.Program",
				UserCodeFilePath = TestsHelper.UserCodeFileName,
				UnitDirectoryPathRelativeToCourse = unitDirectoryPathRelativeToCourse,
				CsProjFilePath = TestsHelper.CsProjFilePath,
			};
			fp = new CsProjectExerciseBlock.FilesProvider(exerciseBlock, courseDirectory);

			var studentZipFilepath = Path.Combine(unitDirectoryAbsolutePath, "ProjDir.exercise.zip");
			if (File.Exists(studentZipFilepath))
				File.Delete(studentZipFilepath);

			var context = new SlideBuildingContext("Test", new Unit(null, unitDirectoryPathRelativeToCourse), CourseSettings.DefaultSettings, new DirectoryInfo(courseDirectory), unitDirectory, null);
			// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
			exerciseBlock.BuildUp(context, ImmutableHashSet<string>.Empty).ToList();
		}

		[Test]
		[Ignore("ProjectExerciseValidator generates exercise zip from exercise block. Zip file built here is not care")]
		public void ReportError_If_StudentZip_HasErrors()
		{
			FileSystem.CopyDirectory(unitDirectory.GetSubdirectory("projDir").FullName,
				unitDirectory.GetSubdirectory("FullProjDir").FullName);
			exerciseBlock.CsProjFilePath = Path.Combine("FullProjDir", TestsHelper.CsProjFilename);
			SaveTempZipFileWithFullProject();

			var validatorOutput = TestsHelper.ValidateBlock(exerciseBlock);

			validatorOutput
				.Should().Contain(
					$"Student zip exercise directory has 'wrong answer' and/or solution files ({TestsHelper.OrderedWrongAnswersAndSolutionNames})");
			validatorOutput
				.Should().Contain($"Student's csproj has user code item ({exerciseBlock.UserCodeFilePath}) of not compile type");
			validatorOutput
				.Should().Contain(
					$"Student's csproj has 'wrong answer' and/or solution items ({TestsHelper.OrderedWrongAnswersAndSolutionNames})");
		}

		private void SaveTempZipFileWithFullProject()
		{
			var zipWithFullProj = new FileInfo(Path.Combine(unitDirectoryAbsolutePath, "FullProjDir.exercise.zip"));
			var noExcludedFiles = new Func<FileInfo, bool>(_ => false);
			var noExcludedDirs = new string[0];

			var csProjFile = TestsHelper.ProjExerciseFolder.GetFile(TestsHelper.CsProjFilename);

			new LazilyUpdatingZip(
					TestsHelper.ProjExerciseFolder,
					noExcludedDirs,
					noExcludedFiles,
					ResolveCsprojLink,
					ExerciseStudentZipBuilder.ResolveCsprojLinks(csProjFile, CsProjectExerciseBlock.BuildingToolsVersion),
					zipWithFullProj)
				.UpdateZip();

			MemoryStream ResolveCsprojLink(FileInfo file)
			{
				return file.Name.Equals(exerciseBlock.CsprojFileName) ? ProjModifier.ModifyCsproj(file, ProjModifier.ReplaceLinksWithItems) : null;
			}
		}

		[Test]
		public void ReportError_If_ExerciseFolder_HasErrors()
		{
			File.Delete(fp.UserCodeFile.FullName);
			File.Delete(Path.Combine(unitDirectoryAbsolutePath, exerciseBlock.CsProjFilePath));

			var validatorOutput = TestsHelper.ValidateBlock(exerciseBlock);

			validatorOutput
				.Should().Contain($"Exercise folder ({fp.ExerciseDirectory.Name}) doesn't contain ({exerciseBlock.CsprojFileName})");
			validatorOutput
				.Should().Contain($"Exercise folder ({fp.ExerciseDirectory.Name}) doesn't contain ({exerciseBlock.UserCodeFilePath})");
		}

		[Test]
		public void ReportError_If_CorrectSolution_Not_Building()
		{
			File.WriteAllText(fp.CorrectSolutionFile.FullName, "");

			var validatorOutput = TestsHelper.ValidateBlock(exerciseBlock);

			validatorOutput
				.Should().Contain(
					$"Correct solution file {fp.CorrectSolutionFile.Name} verdict is not OK. RunResult = Id: test.csproj, Verdict: CompilationError");
		}

		[Test]
		public void ReportError_If_NUnitTestRunner_Tries_To_Run_NonExisting_Test_Class()
		{
			exerciseBlock.NUnitTestClasses = new[] { "non_existing.test_class", };
			exerciseBlock.ReplaceStartupObjectForNUnitExercises();

			var validatorOutput = TestsHelper.ValidateBlock(exerciseBlock);

			validatorOutput
				.Should()
				.Contain(
					$"Correct solution file {fp.CorrectSolutionFile.Name} verdict is not OK. RunResult = Id: test.csproj, Verdict: RuntimeError: System.ArgumentException: Error in checking system: test class non_existing.test_class does not exist");
		}

		[Test]
		public void ReportError_If_Solution_For_ProjectExerciseBlock_Is_Not_Solution()
		{
			exerciseBlock.NUnitTestClasses = new[] { $"test.{nameof(OneFailingTest)}" };
			exerciseBlock.ReplaceStartupObjectForNUnitExercises();

			var validatorOutput = TestsHelper.ValidateBlock(exerciseBlock);
			validatorOutput
				.Should()
				.Contain(
					$"Correct solution file {fp.CorrectSolutionFile.Name} is not solution. RunResult = Id: test.csproj, Verdict: Ok")
				.And
				.Contain("Как минимум один из тестов не пройден")
				.And
				.Contain("I_am_a_failure");
		}
	}
}