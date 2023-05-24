namespace CourseToolHotReloader.Infrastructure.Exceptions;

public class InternalServerErrorException : CourseToolHotReloaderHttpException
{
	public InternalServerErrorException(string message)
		: base(message)
	{
	}

	public InternalServerErrorException()
	{
	}
}