using System;
using System.Collections.Generic;
using CourseToolHotReloader.Dtos;

namespace CourseToolHotReloader.UpdateWorkers;

public interface ICourseUpdateQuery
{
	void RegisterUpdate(CourseUpdate update);
	void RegisterCreate(CourseUpdate courseUpdate);
	void RegisterDelete(CourseUpdate update);

	CourseUpdateQueryLocker LockUpdates();
	void UnlockUpdates(CourseUpdateQueryLocker queryLocker);
	ICollection<CourseUpdate> GetAllCourseUpdate();
	ICollection<CourseUpdate> GetAllDeletedFiles();
	void Clear();
}

public class CourseUpdateQuery : ICourseUpdateQuery
{
	private readonly object locker = new();

	private CourseUpdateQueryLocker? currentQueryLocker;
	private bool isLocked;

	private HashSet<CourseUpdate> updatesQuery = new();
	private HashSet<CourseUpdate> createdFiles = new();
	private HashSet<CourseUpdate> deletedFiles = new();

	private HashSet<CourseUpdate> updatesQueryTemp = new();
	private HashSet<CourseUpdate> createdFilesTemp = new();
	private HashSet<CourseUpdate> deletedFilesTemp = new();

	public void RegisterUpdate(CourseUpdate update)
	{
		lock (locker)
		{
			RegisterUpdateInternal(update, updatesQuery);
			if (isLocked)
				RegisterUpdateInternal(update, updatesQueryTemp);
		}
	}

	public void RegisterCreate(CourseUpdate update)
	{
		lock (locker)
		{
			RegisterCreateInternal(update, updatesQuery, createdFiles, deletedFiles);
			if (isLocked)
				RegisterCreateInternal(update, updatesQueryTemp, createdFilesTemp, deletedFilesTemp);
		}
	}

	public void RegisterDelete(CourseUpdate update)
	{
		lock (locker)
		{
			RegisterDeleteInternal(update, updatesQuery, createdFiles, deletedFiles);
			if (isLocked)
				RegisterDeleteInternal(update, updatesQueryTemp, createdFilesTemp, deletedFilesTemp);
		}
	}

	public CourseUpdateQueryLocker LockUpdates()
	{
		lock (locker)
		{
			if (isLocked)
			{
				updatesQueryTemp.Clear();
				deletedFilesTemp.Clear();
				createdFilesTemp.Clear();
			}

			var updated = new CourseUpdate[updatesQuery.Count];
			var deleted = new CourseUpdate[deletedFiles.Count];

			updatesQuery.CopyTo(updated);
			deletedFiles.CopyTo(deleted);

			isLocked = true;
			currentQueryLocker = new CourseUpdateQueryLocker(updated, deleted, this);

			return currentQueryLocker;
		}
	}

	public void UnlockUpdates(CourseUpdateQueryLocker queryLocker)
	{
		lock (locker)
		{
			if (!isLocked || queryLocker != currentQueryLocker)
				return;

			if (queryLocker.ClearOldUpdatesOnUnlock)
			{
				updatesQuery = updatesQueryTemp;
				deletedFiles = deletedFilesTemp;
				createdFiles = createdFilesTemp;

				updatesQueryTemp = new HashSet<CourseUpdate>();
				deletedFilesTemp = new HashSet<CourseUpdate>();
				createdFilesTemp = new HashSet<CourseUpdate>();
			}
			else
			{
				updatesQueryTemp.Clear();
				deletedFilesTemp.Clear();
				createdFilesTemp.Clear();
			}

			isLocked = false;
			currentQueryLocker = null;
		}
	}

	public ICollection<CourseUpdate> GetAllCourseUpdate()
	{
		lock (locker)
		{
			var updated = new CourseUpdate[updatesQuery.Count];
			updatesQuery.CopyTo(updated);
			return updated;
		}
	}

	public ICollection<CourseUpdate> GetAllDeletedFiles()
	{
		lock (locker)
		{
			var deleted = new CourseUpdate[deletedFiles.Count];
			deletedFiles.CopyTo(deleted);
			return deleted;
		}
	}

	public void Clear()
	{
		lock (locker)
		{
			updatesQuery.Clear();
			createdFiles.Clear();
			deletedFiles.Clear();

			updatesQueryTemp.Clear();
			createdFilesTemp.Clear();
			deletedFilesTemp.Clear();
		}
	}

	private static void RegisterUpdateInternal(CourseUpdate update, ISet<CourseUpdate> updatesQuery)
	{
		updatesQuery.Add(update);
	}

	private static void RegisterCreateInternal(
		CourseUpdate update,
		ISet<CourseUpdate> updatesQuery,
		ISet<CourseUpdate> createdFiles,
		ISet<CourseUpdate> deletedFiles
	)
	{
		updatesQuery.Add(update);
		deletedFiles.Remove(update);
		createdFiles.Add(update);
	}

	private static void RegisterDeleteInternal(
		CourseUpdate update,
		ISet<CourseUpdate> updatesQuery,
		ISet<CourseUpdate> createdFiles,
		ISet<CourseUpdate> deletedFiles
	)
	{
		updatesQuery.Remove(update);

		if (!createdFiles.Remove(update))
			deletedFiles.Add(update);
	}
}

public class CourseUpdateQueryLocker : IDisposable
{
	public ICollection<CourseUpdate> Updated { get; }
	public ICollection<CourseUpdate> Deleted { get; }

	public bool ClearOldUpdatesOnUnlock { get; set; }

	private readonly ICourseUpdateQuery courseUpdateQuery;
	private bool isDisposed;

	public CourseUpdateQueryLocker(
		ICollection<CourseUpdate> updated,
		ICollection<CourseUpdate> deleted,
		ICourseUpdateQuery courseUpdateQuery
	)
	{
		Updated = updated;
		Deleted = deleted;
		this.courseUpdateQuery = courseUpdateQuery;
	}

	public void UnlockQuery()
	{
		courseUpdateQuery.UnlockUpdates(this);
	}

	public void Dispose()
	{
		if (isDisposed)
			return;

		UnlockQuery();
		isDisposed = true;
		GC.SuppressFinalize(this);
	}
}