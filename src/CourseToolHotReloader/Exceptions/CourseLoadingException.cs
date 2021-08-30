using System;

namespace CourseToolHotReloader.Exceptions
{
	public class CourseLoadingException : Exception
	{
		public CourseLoadingException(string message)
			: base(message)
		{
		}

		public CourseLoadingException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}