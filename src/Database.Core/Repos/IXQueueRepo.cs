using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos
{
	public interface IXQueueRepo
	{
		Task AddXQueueWatcher(string name, string baseUrl, string queueName, string username, string password);
		Task<List<XQueueWatcher>> GetXQueueWatchers();
		Task<XQueueExerciseSubmission> FindXQueueSubmission(UserExerciseSubmission submission);
		Task AddXQueueSubmission(XQueueWatcher watcher, string xQueueHeader, string courseId, Guid slideId, string code);
		Task MarkXQueueSubmissionThatResultIsSent(XQueueExerciseSubmission submission);
		Task<List<XQueueExerciseSubmission>> GetXQueueSubmissionsReadyToSentResults(XQueueWatcher watcher);
	}
}