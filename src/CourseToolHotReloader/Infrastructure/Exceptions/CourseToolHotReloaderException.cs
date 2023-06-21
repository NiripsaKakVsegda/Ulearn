using System;

namespace CourseToolHotReloader.Infrastructure.Exceptions;

public class CourseToolHotReloaderHttpException : Exception
{
	public CourseToolHotReloaderHttpException(string message)
		: base(message)
	{
	}

	public CourseToolHotReloaderHttpException()
	{
	}
}