namespace CourseToolHotReloader.Exceptions
{
	public class ForbiddenException : CourseToolHotReloaderHttpException
	{
		public ForbiddenException()
			: base($"Status code is 403.")
		{ }
	}
}