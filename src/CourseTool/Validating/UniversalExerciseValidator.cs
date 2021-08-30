using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using RunCheckerJob;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.RunCheckerJobApi;

namespace uLearn.CourseTool.Validating
{
	public class UniversalExerciseValidator : BaseValidator
	{
		private readonly UniversalExerciseBlock ex;
		private readonly ExerciseSlide slide;
		private readonly DockerSandboxRunnerSettings settings;
		private readonly UniversalExerciseBlock.FilesProvider fp;

		public UniversalExerciseValidator(BaseValidator baseValidator, DockerSandboxRunnerSettings settings, ExerciseSlide slide, UniversalExerciseBlock exercise)
			: base(baseValidator)
		{
			this.settings = settings;
			this.slide = slide;
			ex = exercise;
			fp = new UniversalExerciseBlock.FilesProvider((UniversalExerciseBlock)slide.Exercise, CourseDirectory);
		}

		public void ValidateExercises()
		{
			ReportErrorIfDockerImageSettingsIsWrong();

			if (ReportErrorIfExerciseFolderMissesRequiredFiles())
				return;

			ReportErrorsInRegion();

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !EnvironmentVariablesUtils.ExistsOnPath("docker.exe"))
			{
				ReportWarning("Docker not found in PATH");
				return;
			}

			ReportErrorIfSolutionNotBuildingOrNotPassesTests();

			ReportErrorIfWrongAnswerExistsButNotBuildingOrPassesTests();

			ReportErrorsInCheckPointsSettings();

			if (ex.CheckInitialSolution)
				ReportErrorIfInitialCodeIsSolutionOrVerdictNotOk();
		}

		private void ReportErrorIfDockerImageSettingsIsWrong()
		{
			if (ex.DockerImageName == null)
				ReportSlideError(slide, "Property 'dockerImageName' not specified");
			if (ex.DockerImageName == null)
				ReportSlideError(slide, "Property 'run' not specified");
			if (ex.DockerImageName != null && !UniversalExerciseBlock.DockerImageNameRegex.Match(ex.DockerImageName).Success)
				ReportSlideError(slide, $"Property dockerImageName '{ex.DockerImageName}' do not match regex '{UniversalExerciseBlock.DockerImageNameRegex}'");
			if (ex.RunCommand != null && !UniversalExerciseBlock.RunCommandRegex.Match(ex.RunCommand).Success)
				ReportSlideError(slide, $"Property run '{ex.RunCommand}' do not match regex '{UniversalExerciseBlock.RunCommandRegex}'");
		}

		private void ReportErrorsInRegion()
		{
			if (ex.Region != null && !ex.NoStudentZip)
				ReportSlideError(slide, "Region works only if StudentZipIsDisabled is true");
			if (ex.Region != null)
			{
				if (ex.SolutionRegionContent.Value == null)
					ReportSlideError(slide, $"Region '{ex.Region}' not exists in file '{ex.UserCodeFilePath}'");
				if (ex.ExerciseInitialCode == null && ex.InitialRegionContent.Value == null)
					ReportSlideError(slide, $"Region '{ex.Region}' not exists in file '{ex.InitialUserCodeFilePath}'");
			}
		}

		private bool ReportErrorIfExerciseFolderMissesRequiredFiles()
		{
			var doNotFindNextErrors =
				ReportSlideError(!fp.ExerciseDirectory.Exists,
					$"Exercise directory '{ex.ExerciseDirPath}' doesn't exist")
				|| ReportSlideError(!fp.UserCodeFile.Exists,
					$"User code file '{ex.UserCodeFilePath}' doesn't exist in exercise directory '{ex.ExerciseDirPath}")
				|| ReportSlideError(!fp.InitialUserCodeFile.Exists && (!ex.NoStudentZip || ex.NoStudentZip && ex.ExerciseInitialCode == null),
					$"Exercise directory '{ex.ExerciseDirPath}' doesn't contain '{ex.InitialUserCodeFilePath}'");
			foreach (var pathToIncludeForChecker in ex.PathsToIncludeForChecker.EmptyIfNull())
			{
				if (!doNotFindNextErrors)
				{
					var di = new DirectoryInfo(Path.Combine(fp.UnitDirectory.FullName, pathToIncludeForChecker));
					doNotFindNextErrors =
						ReportSlideError(!new DirectoryInfo(CourseDirectory).IsInside(di),
							$"includePathForChecker '{pathToIncludeForChecker}' is not in subtree of directory with course.xml")
						|| ReportSlideError(!di.Exists,
							$"includePathForChecker '{pathToIncludeForChecker}' doesn't exist");
				}
			}

			return doNotFindNextErrors;
		}

		private void ReportErrorIfInitialCodeIsSolutionOrVerdictNotOk()
		{
			CheckBuildAndTests(ex.GetInitialCode(fp), "Initial code", false);
		}

		private void ReportErrorIfSolutionNotBuildingOrNotPassesTests()
		{
			CheckBuildAndTests(ex.GetCorrectSolution(fp), $"Solution file '{ex.CorrectSolutionFilePath}'", true);
		}

		private void ReportErrorIfWrongAnswerExistsButNotBuildingOrPassesTests()
		{
			var wrongAnswerFiles = fp.WrongAnswerFiles;
			foreach (var waFile in wrongAnswerFiles)
			{
				var code = ex.GetRegionContent(waFile) ?? waFile.ContentAsUtf8();
				CheckBuildAndTests(code, $"Wrong answer file '{waFile.Name}'", false);
			}
		}

		private void ReportErrorsInCheckPointsSettings()
		{
			ReportSlideError(ex.ExerciseType == ExerciseType.CheckPoints && ex.PassingPoints.HasValue, "Property 'passingPoints' not specified");
		}

		private void CheckBuildAndTests(string code, string fileDescription, bool shouldPassTests)
		{
			var submission = (CommandRunnerSubmission)ex.CreateSubmission(Utils.NewNormalizedGuid(), code, fp.CourseDirectory);
			submission.RunCommand = settings.RunCommand;
			var result = new DockerSandboxRunner().Run(submission);
			var condition = ex.ExerciseType == ExerciseType.CheckOutput && result.Verdict != Verdict.Ok && shouldPassTests || 
							result.Verdict != Verdict.WrongAnswer && !shouldPassTests;
			if (!ReportSlideError(condition, $"{fileDescription} verdict is not OK. RunResult = {result}"))
			{
				var not = shouldPassTests ? "not " : "";
				ReportSlideError(shouldPassTests ^ ex.IsCorrectRunResult(result), $"{fileDescription} is {not}solution. RunResult = {result}. " +
																				$"ExpectedOutput = {ex.ExpectedOutput.NormalizeEoln()} " +
																				$"RealOutput = {result.GetOutput().NormalizeEoln()}");
			}
		}

		private bool ReportSlideError(bool ifCondIsTrue, string withMessage)
		{
			if (!ifCondIsTrue)
				return false;
			ReportSlideError(slide, withMessage);
			return true;
		}
	}
}