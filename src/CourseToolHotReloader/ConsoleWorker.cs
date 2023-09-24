using System;
using System.Security;
using CourseToolHotReloader.Infrastructure;
using Vostok.Logging.Abstractions;

namespace CourseToolHotReloader;

public static class ConsoleWorker
{
	private static ILog Log => LogProvider.Get();

	public static void WriteLine(string text)
	{
		Console.WriteLine(text);
		Log.Info(text);
	}

	public static void WriteLineWithTime(string text)
	{
		Console.WriteLine($"{DateTime.Now:HH:mm:ss} {text}");
		Log.Info(text);
	}

	public static void WriteError(string errorMessage, Exception? e = null)
	{
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(errorMessage);
		Console.ResetColor();
		if (e is null)
			Log.Error(errorMessage);
		else
			Log.Error(e, errorMessage);
	}

	public static void WriteErrorWithTime(string errorMessage, Exception? e = null)
	{
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine($"{DateTime.Now:HH:mm:ss} {errorMessage}");
		Console.ResetColor();
		if (e is null)
			Log.Error(errorMessage);
		else
			Log.Error(e, errorMessage);
	}

	public static void WriteAlert(string alertMessage)
	{
		Console.ForegroundColor = ConsoleColor.DarkYellow;
		Console.WriteLine(alertMessage);
		Console.ResetColor();
		Log.Warn(alertMessage);
	}

	public static void Debug(string text)
	{
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine($"DEBUG {text}");
		Console.ResetColor();
		Log.Debug(text);
	}

	public static string GetCourseId()
	{
		Console.WriteLine("Введите id основной версии курса на ulearn. (Когда вы находитесь на любом слайде курса, id можно посмотреть в строке браузера ulearn.me/Course/вот здесь/)");
		Console.Write("> ");
		return Console.ReadLine() ?? "";
	}

	public static string GetCoursePath()
	{
		Console.WriteLine("Введите путь к папке, в которой находится файл course.xml. (Желательно использовать абсолютный путь)");
		Console.Write("> ");
		return Console.ReadLine() ?? "";
	}

	public static SecureString GetToken(string tokenUrl)
	{
		Console.WriteLine($"Авторизуйтесь на {tokenUrl}, получите токен и вставьте его сюда.");
		Console.Write("> ");
		
		Utils.OpenInBrowser(tokenUrl)
			.RefineError($"Ошибка при попытке открыть страницу авторизации в браузере")
			.OnFail(WriteError);

		
		var token = new SecureString();
		while (true)
		{
			var currentKey = Console.ReadKey(true);
			if (currentKey.Key == ConsoleKey.Enter)
				break;

			if (currentKey.Key == ConsoleKey.Backspace)
			{
				if (token.Length <= 0)
					continue;
				token.RemoveAt(token.Length - 1);
				Console.Write("\b \b");
			}
			else if (currentKey.KeyChar != '\u0000')
				// KeyChar == '\u0000' if the key pressed does not correspond to a printable character, e.g. F1, Pause-Break, etc
			{
				token.AppendChar(currentKey.KeyChar);
				Console.Write("*");
			}
		}

		Console.WriteLine();
		return token;
	}

	public static string GetCourseIndex(string message = "")
	{
		Console.WriteLine($"Введите номер курса{message}");
		Console.Write("> ");
		return Console.ReadLine() ?? "";
	}

	public static bool GetAnswer(string question)
	{
		Console.Write($"{question} (y/n): ");
		while (true)
		{
			var answer = char.ToLower(Console.ReadKey(true).KeyChar);
			switch (answer)
			{
				case 'y' or 'н':
					Console.WriteLine('y');
					return true;
				case 'n' or 'т':
					Console.WriteLine('n');
					return false;
			}
		}
	}

	public static void NotifyError()
	{
		Console.Beep();
	}
}