namespace CourseToolHotReloader.Exceptions
{
	public class InternalServerErrorException : CourseToolHotReloaderHttpException
	{
		public InternalServerErrorException(string message)
			: base(message)
		{ }
	}
}