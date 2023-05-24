namespace CourseToolHotReloader.Infrastructure.Exceptions;

public class UnauthorizedException : CourseToolHotReloaderHttpException
{
	public UnauthorizedException()
		: base($"Status code is 401.")
	{ }
}