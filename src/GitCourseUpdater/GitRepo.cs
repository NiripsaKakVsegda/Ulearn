using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using JetBrains.Annotations;
using LibGit2Sharp;
using Newtonsoft.Json;
using Ulearn.Common;
using Vostok.Logging.Abstractions;
using Ulearn.Common.Extensions;

namespace GitCourseUpdater
{
	public interface IGitRepo : IDisposable
	{
		(MemoryStream zip, string courseXmlSubdirectoryInRepo) GetCurrentStateAsZip(string courseSubdirectoryInRepo = "");
		CommitInfo GetCurrentCommitInfo();
		CommitInfo GetCommitInfo(string hash);
		List<string> GetChangedFiles(string fromHash, string toHash, string courseSubdirectoryInRepo = null);
		void Checkout(string commitHashOrBranchName);
	}

	public class GitException : Exception
	{
		public bool MayBeSSHException;

		public GitException(string massage, Exception innerException)
			: base(massage, innerException)
		{
		}
	}

	public class GitRepo : IGitRepo
	{
		private string url;
		//private CredentialsHandler credentialsHandler;
		private DirectoryInfo reposBaseDir;
		private string repoDirName;
		private Repository repo;
		private static ILog log => LogProvider.Get().ForContext(typeof(GitRepo));
		private string privateKeyPath;
		private string publicKeyPath;
		private static object @lock = new(); // Потокобезопасность не гарантируется библиотекой libgit2

		// url example git@github.com:user/myrepo.git
		// В GitRepo используется Monitor. Он должен быть освобожден в том же потоке, что и взят.
		public GitRepo(string url, DirectoryInfo reposBaseDir, string publicKey, string privateKey, DirectoryInfo keysTempDirectory)
		{
			this.url = url;
			this.reposBaseDir = reposBaseDir;
			if (!reposBaseDir.Exists)
				reposBaseDir.Create();

			Monitor.Enter(@lock);

			var filenameGuid = Guid.NewGuid().ToString();
			privateKeyPath = Path.Combine(keysTempDirectory.FullName, filenameGuid);
			publicKeyPath = privateKeyPath + ".pub";
			File.WriteAllText(privateKeyPath, privateKey);
			File.WriteAllText(publicKeyPath, publicKey);

			try
			{
				if (!TryUpdateExistingRepo())
					Clone();
			}
			catch (Exception ex)
			{
				Dispose(); // Если объект не создан, извне Dispose не вызовут
				if (ex.Message.Contains("Permission denied"))
					throw new GitException(ex.Message, ex) { MayBeSSHException = true };
				throw;
			} 
		}

		public (MemoryStream zip, string courseXmlSubdirectoryInRepo) GetCurrentStateAsZip(string courseSubdirectoryInRepo = "")
		{
			if (courseSubdirectoryInRepo == null)
				courseSubdirectoryInRepo = "";
			log.Info($"Start load '{repoDirName}' to zip");
			var repoDir = reposBaseDir.GetSubdirectory(repoDirName);
			var courseRepoDir = new DirectoryInfo(Path.Combine(repoDir.FullName, courseSubdirectoryInRepo));
			if (!courseRepoDir.GetFile("course.xml").Exists)
			{
				var courseXmlDirectory = courseRepoDir.GetFiles("course.xml", SearchOption.AllDirectories).FirstOrDefault()?.Directory;
				if (courseXmlDirectory != null)
				{
					courseRepoDir = courseXmlDirectory;
					courseSubdirectoryInRepo = courseXmlDirectory.GetRelativePath(repoDir);
				}
			}
			var zip = ZipUtils.CreateZipFromDirectory(new List<string> { courseRepoDir.FullName }, new List<string> {".git/"},  null);
			log.Info($"Successfully load '{repoDirName}' to zip");
			return (zip, courseSubdirectoryInRepo);
		}

		public CommitInfo GetCurrentCommitInfo()
		{
			var lastCommit = repo.Commits.First();
			var commitInfo = ToCommitInfo(lastCommit);
			log.Info($"GetCurrentCommitInfo '{repoDirName}': {JsonConvert.SerializeObject(commitInfo)}");
			return commitInfo;
		}

		[CanBeNull]
		public CommitInfo GetCommitInfo(string hash)
		{
			var commit = repo.Lookup<Commit>(hash);
			if (commit == null)
			{
				log.Warn($"Commit not found repo '{repoDirName}' hash '{hash}'");
				return null;
			}

			return ToCommitInfo(commit);
		}


		// null, если коммит не найден. Возвращает полные пути в репозитории
		[CanBeNull]
		public List<string> GetChangedFiles(string fromHash, string toHash, string courseSubdirectoryInRepo = null)
		{
			var commitFrom = repo.Lookup<Commit>(fromHash);
			var commitTo = repo.Lookup<Commit>(toHash);
			if (commitFrom == null || commitTo == null)
				return null;
			var treeChanges = repo.Diff.Compare<TreeChanges>(commitFrom.Tree, commitTo.Tree);
			var paths = treeChanges.Select(c => c.Path).ToList();
			log.Info($"All changed files in repo '{url}' between '{commitFrom}' '{commitTo}' courseSubdirectory '{courseSubdirectoryInRepo}':");
			foreach (var path in paths)
				log.Info($"Path: '{path}'");
			if (courseSubdirectoryInRepo != null)
			{
				var normalisedCourseSubdirectoryInRepo = NormalizePath(courseSubdirectoryInRepo);
				paths = paths.Where(p => NormalizePath(p).StartsWith(normalisedCourseSubdirectoryInRepo, StringComparison.OrdinalIgnoreCase)).ToList();
			}
			return paths;
		}
		
		private static string NormalizePath(string path)
		{
			return path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Trim(Path.AltDirectorySeparatorChar);
		}

		private static readonly Regex sha1Regex = new("[a-f0-9]{40}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public void Checkout(string commitHashOrBranchName)
		{
			if (sha1Regex.IsMatch(commitHashOrBranchName))
				CheckoutCommit(commitHashOrBranchName);
			else
				CheckoutBranchAndResetToOrigin(commitHashOrBranchName);
		}

		private void CheckoutCommit(string hash)
		{
			log.Info($"Start checkout '{url}' commit '{hash}' in '{repoDirName}'");
			Commands.Checkout(repo, hash);
			log.Info($"Successfully checkout '{url}' commit '{hash}' in '{repoDirName}'");
		}

		private void CheckoutBranchAndResetToOrigin(string name)
		{
			name = name.Replace("origin/", "");
			var remoteBranch = repo.Branches[$"origin/{name}"];
			if (remoteBranch == null)
				throw new ArgumentException($"Branch 'origin/{name}' does not exist");
			if (repo.Branches[name] == null)
			{
				var localBranch = repo.CreateBranch(name, remoteBranch.Tip);
				repo.Branches.Update(localBranch, b =>
				{
					b.Remote = "origin";
					b.UpstreamBranch = $"refs/heads/{name}";
				});
			}

			log.Info($"Start checkout '{url}' branch '{name}' in '{repoDirName}'");
			Commands.Checkout(repo, name);
			log.Info($"Successfully checkout '{url}' branch '{name}' in '{repoDirName}'");
			repo.Reset(ResetMode.Hard, remoteBranch.Tip);
			log.Info($"Successfully reset '{url}' branch '{name}' in '{repoDirName}' to commit '{repo.Branches[name].Tip.Sha}'");
		}

		private static CommitInfo ToCommitInfo(Commit commit)
		{
			return new CommitInfo
			{
				Hash = commit.Sha,
				Message = commit.Message,
				AuthorName = commit.Author.Name,
				AuthorEmail = commit.Author.Email,
				Time = commit.Author.When,
			};
		}

		// Пытается найти уже склоненную версию репозитория на диске и обновить её из удаленного репозитория
		private bool TryUpdateExistingRepo()
		{
			try
			{
				repoDirName = GetExistingRepoFolderName();
				if (repoDirName == null)
					return false;
				var repoPath = reposBaseDir.GetSubdirectory(repoDirName);
				repo = new Repository(repoPath.FullName);
				if (HasUncommittedChanges())
					throw new LibGit2SharpException($"Has uncommited changes in '{repoDirName}'");
				FetchAll();
				CheckoutBranchAndResetToOrigin("master");
			}
			catch (Exception ex)
			{
				log.Warn(ex, $"Could not update existing repository '{repoDirName}'");
				return false;
			}

			return true;
		}

		private string GetExistingRepoFolderName()
		{
			var names = reposBaseDir.GetDirectories(Url2Name() + "@*").Select(d => d.Name).ToList();
			if (names.Count == 0)
				return null;
			return names.Max();
		}

		// Создает чистую папку и клонирует в неё
		private void Clone()
		{
			repoDirName = Url2Name() + "@" + DateTime.Now.ToSortable();
			var repoPath = reposBaseDir.GetSubdirectory(repoDirName);
			log.Info($"Start clone '{url}' into '{repoDirName}'");
			GitShWorker.Clone(repoPath.FullName, privateKeyPath, url);
			repo = new Repository(repoPath.FullName);
			log.Info($"Successfully clone '{url}' into '{repoDirName}'");
		}

		private string Url2Name()
		{
			var parts = url.Split(':');
			var name = parts.Last().Substring(0, parts.Last().Length - 4).Replace('/', '_');
			return GetSafeFilename(name);
		}

		[CanBeNull]
		private static string GetSafeFilename(string filename)
		{
			return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
		}

		private void FetchAll()
		{
			log.Info($"Start fetch all '{url}' in '{repoDirName}'");
			var repoPath = reposBaseDir.GetSubdirectory(repoDirName);
			foreach (var remote in repo.Network.Remotes)
			{
				var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification).Join("");
				GitShWorker.Fetch(refSpecs, repoPath.FullName, privateKeyPath, url);
			}
 
			log.Info($"Successfully fetch all '{url}' in '{repoDirName}'");
		}

		private bool HasUncommittedChanges()
		{
			var status = repo.RetrieveStatus();
			return status.IsDirty;
		}

		public void Dispose()
		{
			try
			{
				repo?.Dispose();
				var privateKeyFileInfo = new FileInfo(privateKeyPath);
				var publicKeyFileInfo = new FileInfo(publicKeyPath);
				if (privateKeyFileInfo.Exists)
					privateKeyFileInfo.Delete();
				if (publicKeyFileInfo.Exists)
					publicKeyFileInfo.Delete();
			}
			finally
			{
				Monitor.Exit(@lock);
			}
		}
	}
}