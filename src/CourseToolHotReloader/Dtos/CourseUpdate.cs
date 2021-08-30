namespace CourseToolHotReloader.Dtos
{
	public interface ICourseUpdate
	{
		public string FullPath { get; set; }
	}

	public class CourseUpdate : ICourseUpdate
	{
		public CourseUpdate(string fullPath)
		{
			FullPath = fullPath;
		}
		
		public string FullPath { get; set; }
	}
}