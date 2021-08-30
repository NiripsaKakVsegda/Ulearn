using System;

namespace CourseToolHotReloader.Exceptions
{
	public class CourseToolHotReloaderHttpException : Exception
	{
		public CourseToolHotReloaderHttpException(string message)
			: base(message)
		{ }
	}
}