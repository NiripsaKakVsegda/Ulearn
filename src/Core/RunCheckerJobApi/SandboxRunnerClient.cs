using System.IO;

namespace Ulearn.Core.RunCheckerJobApi
{
	public interface ISandboxRunnerClient
	{
		RunningResults Run(RunnerSubmission submission);
		RunningResults RunContainerAndGetResultInternal(RunnerSubmission submission, DirectoryInfo submissionWorkingDirectory);
	}
}