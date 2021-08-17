using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;
using Ulearn.Common;

namespace Database.Repos
{
	public interface ICoursesRepo
	{
		[ItemCanBeNull]
		Task<CourseVersion> GetPublishedCourseVersion(string courseId);
		Task<List<CourseVersion>> GetCourseVersions(string courseId);

		Task<CourseVersion> AddCourseVersion(string courseId, string courseName, Guid versionId, string authorId,
			string pathToCourseXml, string repoUrl, string commitHash, string description, byte[] courseContent);

		Task MarkCourseVersionAsPublished(Guid versionId);
		Task DeleteCourseVersion(string courseId, Guid versionId);
		Task<List<CourseVersion>> GetPublishedCourseVersions();
		Task<CourseAccess> GrantAccess(string courseId, string userId, CourseAccessType accessType, string grantedById, string comment);
		Task<List<CourseAccess>> RevokeAccess(string courseId, string userId, CourseAccessType accessType, string grantedById, string comment);
		Task<List<CourseAccess>> GetCourseAccesses(string courseId);
		Task<List<CourseAccess>> GetCourseAccesses(string courseId, string userId);
		Task<DefaultDictionary<string, List<CourseAccess>>> GetCoursesAccesses(IEnumerable<string> coursesIds);
		Task<bool> HasCourseAccess(string userId, string courseId, CourseAccessType accessType);
		Task<List<CourseAccess>> GetUserAccesses(string userId);
		Task<List<string>> GetPublishedCourseIds();
		Task<List<string>> GetCoursesUserHasAccessTo(string userId, CourseAccessType accessType);
		Task<CourseVersionFile> GetVersionFile(Guid courseVersion);
		Task<CourseVersionFile> GetPublishedVersionFile(string courseId);
		Task<CourseGit> GetCourseRepoSettings(string courseId);
	}
}