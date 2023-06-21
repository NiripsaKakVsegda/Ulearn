using System.IO;

namespace CourseToolHotReloader.UpdateWorkers.WatchActionStrategies;

public interface IWatchActionStrategy
{
	void Changed(object sender, FileSystemEventArgs e);
	void Created(object sender, FileSystemEventArgs e);
	void Deleted(object sender, FileSystemEventArgs e);
	void Renamed(object sender, RenamedEventArgs e);
}