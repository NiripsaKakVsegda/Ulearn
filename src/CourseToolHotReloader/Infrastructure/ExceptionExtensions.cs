using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using CourseToolHotReloader.Infrastructure.Exceptions;

namespace CourseToolHotReloader.Infrastructure;

public static class ExceptionExtensions
{
	public static string GetMessage(this Exception e, string apiUrl = "API", string defaultMessage = "Подробнее в логах")
	{
		return e switch
		{
			UriFormatException => "Указанный в config.json baseUrl имеет некорректный формат",
			HttpRequestException { InnerException: SocketException } => $"Не удалось подключиться к {apiUrl}",
			HttpRequestException { InnerException: IOException } ee when ee.Message.Contains("SSL") =>
				$"Не удалось использовать https при подключении к {apiUrl}",
			IOException => "Ошибка ввода-вывода. Подробнее в логах",
			InternalServerErrorException => "Сервер вернул код 500. Это похоже на баг. Подробнее в логах",
			ForbiddenException => "Сервер вернул код 403. Нет прав на операцию.",
			UnauthorizedException => "Сервер вернул код 401. Повторите процесс авторизации.",
			StatusCodeException ee => $"Сервер вернул код {ee.StatusCode}",
			HttpRequestException => e.Message,
			AggregateException ee => ee.InnerExceptions.Single().GetMessage(apiUrl, defaultMessage),
			_ => defaultMessage
		};
	}
}