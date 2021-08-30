namespace CourseToolHotReloader.Exceptions
{
	public class UnauthorizedException : CourseToolHotReloaderHttpException
	{
		public UnauthorizedException()
			: base($"Status code is 401.")
		{ }
	}
}