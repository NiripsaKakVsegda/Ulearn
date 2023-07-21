using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Extensions;

namespace Database.Repos
{
	public class CoursesRepo : ICoursesRepo
	{
		private readonly UlearnDb db;

		public CoursesRepo(UlearnDb db)
		{
			this.db = db;
		}

		public async Task<List<string>> GetPublishedCourseIds()
		{
			return await db.CourseVersions
				.Where(v => v.PublishTime != null)
				.Select(v => v.CourseId)
				.Distinct()
				.ToListAsync();
		}

		public async Task<CourseVersion> GetPublishedCourseVersion(string courseId)
		{
			return await db.CourseVersions
				.Where(v => v.CourseId == courseId && v.PublishTime != null)
				.OrderByDescending(v => v.PublishTime)
				.FirstOrDefaultAsync();
		}

		public async Task<List<CourseVersion>> GetCourseVersions(string courseId)
		{
			return await db.CourseVersions
				.Where(v => v.CourseId == courseId)
				.OrderByDescending(v => v.LoadingTime)
				.ToListAsync();
		}

		public async Task<CourseVersion> AddCourseVersion(string courseId, string courseName, Guid versionId, string authorId,
			string pathToCourseXml, string repoUrl, string commitHash, string description, byte[] courseContent)
		{
			var courseVersion = new CourseVersion
			{
				Id = versionId,
				CourseId = courseId,
				CourseName = courseName,
				LoadingTime = DateTime.Now,
				PublishTime = null,
				AuthorId = authorId,
				PathToCourseXml = pathToCourseXml,
				CommitHash = commitHash,
				Description = description,
				RepoUrl = repoUrl
			};
			db.CourseVersions.Add(courseVersion);
			var courseVersionFile = new CourseVersionFile
			{
				CourseVersionId = versionId,
				CourseVersion = courseVersion,
				CourseId = courseId,
				File = courseContent
			};
			db.CourseVersionFiles.Add(courseVersionFile);
			await db.SaveChangesAsync(); // автоматически выполнит обе операции в транзации

			return courseVersion;
		}

		public async Task MarkCourseVersionAsPublished(Guid versionId)
		{
			var courseVersion = await db.CourseVersions.FindAsync(versionId);
			if (courseVersion == null)
				return;

			courseVersion.PublishTime = DateTime.Now;
			await db.SaveChangesAsync();
		}

		public async Task DeleteCourseVersion(string courseId, Guid versionId)
		{
			var courseVersion = await db.CourseVersions.FindAsync(versionId);
			if (courseVersion == null)
				return;

			if (!courseId.EqualsIgnoreCase(courseVersion.CourseId))
				return;

			db.CourseVersions.Remove(courseVersion);
			await db.SaveChangesAsync();
		}

		public async Task<List<CourseVersion>> GetPublishedCourseVersions()
		{
			var courseVersions = await db.CourseVersions.ToListAsync();
			return courseVersions
				.GroupBy(v => v.CourseId.ToLower())
				.Select(g => g.MaxBy(v => v.PublishTime))
				.ToList();
		}

		/* Course accesses */

		private async Task<List<CourseAccess>> GetActualEnabledCourseAccesses(string courseId = null, string userId = null)
		{
			var query = db.CourseAccesses.AsQueryable();
			if (courseId != null)
				query = query.Where(x => x.CourseId == courseId);
			if (userId != null)
				query = query.Where(x => x.UserId == userId);

			query = query
				.GroupBy(a => new { a.CourseId, a.UserId, a.AccessType })
				.Select(g => g.OrderByDescending(a => a.GrantTime).First());

			return (await query.ToListAsync())
				.Where(a => a.IsEnabled)
				.ToList();
		}

		public async Task<CourseAccess> GrantAccess(string courseId, string userId, CourseAccessType accessType, string grantedById, string comment)
		{
			courseId = courseId.ToLower();
			var access = new CourseAccess
			{
				CourseId = courseId,
				UserId = userId,
				AccessType = accessType,
				GrantTime = DateTime.Now,
				GrantedById = grantedById,
				IsEnabled = true,
				Comment = comment
			};
			db.CourseAccesses.Add(access);

			await db.SaveChangesAsync().ConfigureAwait(false);
			return access;
		}

		public async Task<CourseAccess> RevokeAccess(string courseId, string userId, CourseAccessType accessType, string grantedById, string comment)
		{
			courseId = courseId.ToLower();
			var revoke = new CourseAccess
			{
				UserId = userId,
				GrantTime = DateTime.Now,
				GrantedById = grantedById,
				Comment = comment,
				IsEnabled = false,
				CourseId = courseId,
				AccessType = accessType
			};
			db.CourseAccesses.Add(revoke);

			await db.SaveChangesAsync();
			return revoke;
		}

		public async Task<List<CourseAccess>> GetCourseAccesses(string courseId)
		{
			return await GetActualEnabledCourseAccesses(courseId: courseId);
		}

		public async Task<List<CourseAccess>> GetCourseAccesses(string courseId, string userId)
		{
			return await GetActualEnabledCourseAccesses(courseId: courseId, userId: userId);
		}

		public async Task<DefaultDictionary<string, List<CourseAccess>>> GetCoursesAccesses(IEnumerable<string> coursesIds)
		{
			var courseAccesses = (await GetActualEnabledCourseAccesses())
				.Where(a => coursesIds.Contains(a.CourseId, StringComparer.OrdinalIgnoreCase))
				.GroupBy(a => a.CourseId)
				.ToDictionary(g => g.Key, g => g.ToList());
			return courseAccesses.ToDefaultDictionary();
		}

		public async Task<bool> HasCourseAccess(string userId, string courseId, CourseAccessType accessType)
		{
			var access = await db.CourseAccesses
				.Where(a =>
					a.CourseId == courseId &&
					a.UserId == userId &&
					a.AccessType == accessType
				)
				.OrderByDescending(a => a.GrantTime)
				.FirstOrDefaultAsync();
			return access is not null && access.IsEnabled;
		}

		public Task<CourseAccess> FindCourseAccess(string userId, string courseId, CourseAccessType accessType)
		{
			return db.CourseAccesses
				.Where(a =>
					a.CourseId == courseId &&
					a.UserId == userId &&
					a.IsEnabled &&
					a.AccessType == accessType
				)
				.OrderByDescending(a => a.GrantTime)
				.FirstOrDefaultAsync();
		}

		public async Task<List<CourseAccess>> GetUserAccesses(string userId)
		{
			return await GetActualEnabledCourseAccesses(userId: userId);
		}

		public async Task<List<string>> GetCoursesUserHasAccessTo(string userId, CourseAccessType accessType)
		{
			return (await GetActualEnabledCourseAccesses(userId: userId))
				.Where(a => a.AccessType == accessType)
				.Select(a => a.CourseId)
				.Distinct()
				.ToList();
		}

		public async Task<CourseVersionFile> GetVersionFile(Guid courseVersion)
		{
			return await db.CourseVersionFiles.FirstOrDefaultAsync(f => f.CourseVersionId == courseVersion);
		}

		public async Task<CourseVersionFile> GetPublishedVersionFile(string courseId)
		{
			var publishedCourseVersion = await GetPublishedCourseVersion(courseId);
			return await GetVersionFile(publishedCourseVersion.Id);
		}

		[CanBeNull]
		public async Task<CourseGit> GetCourseRepoSettings(string courseId)
		{
			var data = await db.CourseGitRepos.Where(v => v.CourseId == courseId).OrderByDescending(v => v.CreateTime).FirstOrDefaultAsync();
			if (data?.RepoUrl == null)
				return null;
			return data;
		}

		public async Task<List<CourseGit>> FindCoursesByRepoUrl(string repoUrl)
		{
			return (await db.CourseGitRepos
					.Where(r => r.RepoUrl == repoUrl)
					.ToListAsync())
				.GroupBy(r => r.CourseId)
				.Select(g => g.MaxBy(r => r.CreateTime))
				.ToList();
		}

		public async Task SetCourseRepoSettings(CourseGit courseGit)
		{
			courseGit.CreateTime = DateTime.Now;
			db.CourseGitRepos.Update(courseGit);
			await db.SaveChangesAsync();
		}

		public async Task RemoveCourseRepoSettings(string courseId)
		{
			var courseGit = new CourseGit { CourseId = courseId };
			await SetCourseRepoSettings(courseGit).ConfigureAwait(false);
		}

		public async Task UpdateKeysByRepoUrl(string repoUrl, string publicKey, string privateKey)
		{
			using (var transaction = db.Database.BeginTransaction())
			{
				var repos = await FindCoursesByRepoUrl(repoUrl);
				foreach (var repo in repos)
				{
					repo.PublicKey = publicKey;
					repo.PrivateKey = privateKey;
					await SetCourseRepoSettings(repo).ConfigureAwait(false);
				}

				transaction.Commit();
			}
		}

		public async Task<List<CourseAccess>> GetUserAccessHistoryByCourseId(string userId, string courseId)
		{
			courseId = courseId.ToLower();
			return await db.CourseAccesses
				.Where(x => x.UserId == userId && x.CourseId == courseId)
				.ToListAsync();
		}

		public async Task<List<CourseAccess>> GetUserAccessHistory(string userId)
		{
			return await db.CourseAccesses.Where(x => x.UserId == userId).ToListAsync();
		}
	}
}