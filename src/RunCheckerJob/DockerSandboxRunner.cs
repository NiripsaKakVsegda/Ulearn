using System.IO;
using Ulearn.Core.RunCheckerJobApi;

namespace RunCheckerJob
{
	public class DockerSandboxRunner : ISandboxRunnerClient
	{
		public RunningResults Run(RunnerSubmission submission)
		{
			return SandboxRunHelper.WithSubmissionWorkingDirectory(submission, this, SandboxRunnerSettings.WorkingDirectory, SandboxRunnerSettings.DeleteSubmissionsAfterFinish);
		}

		private DockerSandboxRunnerSettings GetSpecificSettings(CommandRunnerSubmission submission)
		{
			return new DockerSandboxRunnerSettings(submission.DockerImageName, submission.RunCommand, submission.TimeLimit);
		}

		public RunningResults RunContainerAndGetResultInternal(RunnerSubmission submission, DirectoryInfo submissionWorkingDirectory)
		{
			var commandRunnerSubmission = (CommandRunnerSubmission)submission;
			var settings = GetSpecificSettings(commandRunnerSubmission);
			var result = DockerProcessRunner.Run((CommandRunnerSubmission)submission, settings, submissionWorkingDirectory.FullName);
			result.Id = submission.Id;
			return result;
		}
	}
}