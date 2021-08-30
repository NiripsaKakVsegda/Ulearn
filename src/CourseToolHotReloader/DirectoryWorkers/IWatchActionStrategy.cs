using System.IO;

namespace CourseToolHotReloader.DirectoryWorkers
{
	public interface IWatchActionStrategy
	{
		public void Renamed(object sender, RenamedEventArgs e);
		public void Deleted(object sender, FileSystemEventArgs e);
		public void Created(object sender, FileSystemEventArgs e);
		public void Changed(object sender, FileSystemEventArgs e);
	}

}