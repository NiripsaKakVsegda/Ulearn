using System.Net;

namespace CourseToolHotReloader.Infrastructure.Exceptions;

public class StatusCodeException : CourseToolHotReloaderHttpException
{
	public readonly HttpStatusCode StatusCode;

	public StatusCodeException(HttpStatusCode statusCode)
		: base($"Status code is {statusCode}.")
	{
		StatusCode = statusCode;
	}
}