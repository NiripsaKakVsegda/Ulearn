using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.VisualBasic.FileIO;
using RunCsJob;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.Helpers;
using Ulearn.Core.RunCheckerJobApi;
using SearchOption = Microsoft.VisualBasic.FileIO.SearchOption;

namespace uLearn.CourseTool.Validating
{
	public class ProjectExerciseValidator : BaseValidator
	{
		private readonly CsProjectExerciseBlock ex;
		private readonly CsProjectExerciseBlock.FilesProvider fp;
		private readonly ExerciseSlide slide;
		private readonly CsSandboxRunnerSettings settings;
		private readonly ExerciseStudentZipBuilder exerciseStudentZipBuilder = new();

		public ProjectExerciseValidator(BaseValidator baseValidator, CsSandboxRunnerSettings settings, ExerciseSlide slide, CsProjectExerciseBlock exercise)
			: base(baseValidator)
		{
			this.settings = settings;
			this.slide = slide;
			ex = exercise;
			fp = new CsProjectExerciseBlock.FilesProvider(exercise, CourseDirectory);
		}

		public void ValidateExercises()
		{
			if (ReportErrorIfExerciseFolderMissesRequiredFiles())
				return;

			ReportWarningIfWrongAnswersAreSolutionsOrNotOk();

			if (!ReportWarningIfExerciseFolderDoesntContainSolutionFile())
				ReportErrorIfSolutionNotBuildingOrNotPassesTests();

			ReportErrorIfStudentsZipHasErrors();

			ReportErrorIfInitialCodeIsSolutionOrVerdictNotOk();
		}

		private bool ReportWarningIfExerciseFolderDoesntContainSolutionFile()
		{
			if (ExerciseDirectoryContainsSolutionFile())
				return false;
			ReportSlideWarning(slide, $"Exercise directory doesn't contain {fp.CorrectSolutionFile.Name}");
			return true;
		}

		private bool ReportErrorIfExerciseFolderMissesRequiredFiles()
		{
			var exerciseFilesRelativePaths = FileSystem.GetFiles(fp.ExerciseDirectory.FullName, SearchOption.SearchAllSubDirectories)
				.Select(path => new FileInfo(path).GetRelativePath(fp.ExerciseDirectory.FullName))
				.ToList();

			return ReportErrorIfMissingCsproj() | ReportErrorIfMissingUserCodeFile();

			bool ReportErrorIfMissingUserCodeFile() => ReportErrorIfMissingFile(ex.UserCodeFilePath);
			bool ReportErrorIfMissingCsproj() => ReportErrorIfMissingFile(ex.CsprojFileName);

			bool ReportErrorIfMissingFile(string path)
			{
				if (exerciseFilesRelativePaths.Any(p => p.Equals(path, StringComparison.InvariantCultureIgnoreCase)))
					return false;
				ReportSlideError(slide, $"Exercise folder ({fp.ExerciseDirectory.Name}) doesn't contain ({path})");
				return true;
			}
		}

		private bool ExerciseDirectoryContainsSolutionFile()
			=> fp.UserCodeFileParentDirectory.GetFiles().Any(f => f.Name.Equals(fp.CorrectSolutionFile.Name, StringComparison.InvariantCultureIgnoreCase));

		private void ReportErrorIfSolutionNotBuildingOrNotPassesTests()
		{
			var solutionCode = fp.CorrectSolutionFile.ContentAsUtf8();
			var submission = ex.CreateSubmission(ex.CsprojFileName, solutionCode, CourseDirectory);
			var result = new CsSandboxRunnerClient().Run(submission);

			if (!IsCompiledAndExecuted(result))
				ReportSlideError(slide, $"Correct solution file {fp.CorrectSolutionFile.Name} verdict is not OK. RunResult = {result}");

			if (!ex.IsCorrectRunResult(result))
				ReportSlideError(slide, $"Correct solution file {fp.CorrectSolutionFile.Name} is not solution. RunResult = {result}. " +
										$"ExpectedOutput = {ex.ExpectedOutput.NormalizeEoln()} " +
										$"RealOutput = {result.GetOutput().NormalizeEoln()}");
			var buildResult = ex.BuildSolution(solutionCode);

			if (buildResult.HasStyleErrors)
			{
				var errorMessage = string.Join("\n", buildResult.StyleErrors.Select(e => e.GetMessageWithPositions()));
				ReportSlideWarning(slide, $"Correct solution file {fp.CorrectSolutionFile.Name} has style issues. {errorMessage}");
			}
		}

		private void ReportWarningIfWrongAnswersAreSolutionsOrNotOk()
		{
			var filesWithWrongAnswer = FileSystem.GetFiles(fp.ExerciseDirectory.FullName, SearchOption.SearchAllSubDirectories)
				.Select(name => new FileInfo(name))
				.Where(f => ex.IsWrongAnswer(f.Name));

			foreach (var waFile in filesWithWrongAnswer)
			{
				var result = new CsSandboxRunnerClient().Run(ex.CreateSubmission(waFile.Name, waFile.ContentAsUtf8(), CourseDirectory));

				ReportWarningIfWrongAnswerVerdictIsNotOk(waFile.Name, result);
				ReportWarningIfWrongAnswerIsSolution(waFile.Name, result);
			}
		}

		private void ReportWarningIfWrongAnswerVerdictIsNotOk(string waFileName, RunningResults waResult)
		{
			if (!IsCompiledAndExecuted(waResult))
				ReportSlideWarning(slide, $"Code verdict of file with wrong answer ({waFileName}) is not OK. RunResult = " + waResult);
		}

		private void ReportWarningIfWrongAnswerIsSolution(string waFileName, RunningResults waResult)
		{
			if (ex.IsCorrectRunResult(waResult))
				ReportSlideWarning(slide, $"Code of file with wrong answer ({waFileName}) is solution!");
		}

		private void ReportErrorIfInitialCodeIsSolutionOrVerdictNotOk()
		{
			var initialCode = fp.UserCodeFile.ContentAsUtf8();
			var submission = ex.CreateSubmission(ex.CsprojFileName, initialCode, CourseDirectory);
			var result = new CsSandboxRunnerClient().Run(submission);

			if (ex.StudentZipIsCompilable)
				ReportErrorIfInitialCodeVerdictIsNotOk(result);

			ReportErrorIfInitialCodeIsSolution(result);
		}

		private void ReportErrorIfInitialCodeVerdictIsNotOk(RunningResults result)
		{
			if (!IsCompiledAndExecuted(result))
				ReportSlideError(slide, "Exercise initial code verdict is not OK. RunResult = " + result);
		}

		private void ReportErrorIfInitialCodeIsSolution(RunningResults result)
		{
			if (ex.IsCorrectRunResult(result))
				ReportSlideError(slide, "Exercise initial code (available to students) is solution!");
		}

		private void ReportErrorIfStudentsZipHasErrors()
		{
			var tempExZipFilePath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).GetFile($"{slide.Id}.exercise.zip");
			var tempExFolder = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExerciseFolder_From_StudentZip"));

			exerciseStudentZipBuilder.BuildStudentZip(slide, tempExZipFilePath, CourseDirectory);
			ZipUtils.UnpackZip(tempExZipFilePath.ReadAllContent(), tempExFolder.FullName);
			try
			{
				ReportErrorIfStudentsZipHasWrongAnswerOrSolutionFiles(tempExFolder);

				var csprojFile = tempExFolder.GetFile(ex.CsprojFileName);
				MsBuildLocationHelper.InitPathToMsBuild();
				FuncUtils.Using(
					new ProjectCollection(ToolsetDefinitionLocations.Registry | ToolsetDefinitionLocations.ConfigurationFile),
					projectCollection =>
					{
						var csproj = new Project(csprojFile.FullName, null, null, projectCollection);
						ReportErrorIfCsprojHasUserCodeOfNotCompileType(tempExFolder, csproj);
						ReportErrorIfCsprojHasWrongAnswerOrSolutionItems(tempExFolder, csproj);
					},
					projectCollection => projectCollection.UnloadAllProjects());

				if (!ex.StudentZipIsCompilable)
					return;

				var buildResult = MsBuildRunner.BuildProject(CsSandboxRunnerSettings.MsBuildSettings, fp.CsprojFile.Name, tempExFolder);
				ReportErrorIfStudentsZipNotBuilding(buildResult);
			}
			finally
			{
				tempExFolder.Delete(true);
				tempExZipFilePath.Delete();
			}
		}

		private void ReportErrorIfStudentsZipNotBuilding(MSbuildResult res)
		{
			if (!res.Success)
				ReportSlideError(slide, ex.CsprojFileName + " not building! " + res);
		}

		private void ReportErrorIfStudentsZipHasWrongAnswerOrSolutionFiles(DirectoryInfo unpackedZipDir)
		{
			var wrongAnswersOrSolution = GetOrderedFileNames(unpackedZipDir.GetRelativePathsOfFiles(), ExerciseStudentZipBuilder.IsAnyWrongAnswerOrAnySolution);

			if (wrongAnswersOrSolution.Any())
				ReportSlideError(slide, $"Student zip exercise directory has 'wrong answer' and/or solution files ({string.Join(", ", wrongAnswersOrSolution)})");
		}

		private string[] GetOrderedFileNames(IEnumerable<string> names, Func<string, bool> predicate) => names.Where(predicate).OrderBy(n => n).ToArray();

		private void ReportErrorIfCsprojHasUserCodeOfNotCompileType(DirectoryInfo unpackedZipDir, Project csproj)
		{
			var userCode = csproj.Items.Single(i => i.UnevaluatedInclude.Equals(ex.UserCodeFilePath, StringComparison.InvariantCultureIgnoreCase));

			if (!userCode.ItemType.Equals("Compile", StringComparison.InvariantCultureIgnoreCase))
				ReportSlideError(slide, $"Student's csproj has user code item ({userCode.UnevaluatedInclude}) of not compile type");
		}

		private void ReportErrorIfCsprojHasWrongAnswerOrSolutionItems(DirectoryInfo unpackedZipDir, Project csproj)
		{
			var csProjItems = csproj.Items.Select(i => i.UnevaluatedInclude);

			var wrongAnswersOrSolution = GetOrderedFileNames(csProjItems, ExerciseStudentZipBuilder.IsAnyWrongAnswerOrAnySolution);

			if (wrongAnswersOrSolution.Any())
				ReportSlideError(slide, $"Student's csproj has 'wrong answer' and/or solution items ({string.Join(", ", wrongAnswersOrSolution)})");
		}
	}
}