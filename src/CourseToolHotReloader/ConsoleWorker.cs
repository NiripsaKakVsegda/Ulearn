using System;
using System.Security;
using Vostok.Logging.Abstractions;

namespace CourseToolHotReloader
{
	public static class ConsoleWorker
	{
		private static ILog log => LogProvider.Get();

		public static void WriteLine(string text)
		{
			Console.WriteLine(text);
			log.Info(text);
		}

		public static void WriteLineWithTime(string text)
		{
			Console.WriteLine($"{DateTime.Now:HH:mm:ss} {text}");
			log.Info(text);
		}

		public static bool AskQuestion(string question)
		{
			Console.Write($"{question} д(ДА)/н(Нет):");
			return GetAnswer();
		}

		public static void WriteError(string errorMessage)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			var text = errorMessage;
			Console.WriteLine(text);
			Console.ResetColor();
			log.Error(text);
		}

		public static void WriteErrorWithTime(string errorMessage)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"{DateTime.Now:HH:mm:ss} {errorMessage}");
			Console.ResetColor();
			log.Error(errorMessage);
		}

		public static void WriteAlert(string alertMessage)
		{
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			var text = alertMessage;
			Console.WriteLine(text);
			Console.ResetColor();
			log.Warn(text);
		}

		public static void Debug(string text)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"DEBUG {text}");
			Console.ResetColor();
			log.Debug(text);
		}

		private static bool GetAnswer()
		{
			var answer = Console.ReadLine();
			return answer != null &&
					(answer.Equals("д", StringComparison.OrdinalIgnoreCase)
					|| answer.Equals("да", StringComparison.OrdinalIgnoreCase));
		}

		public static string GetLogin()
		{
			Console.Write("логин:");
			return Console.ReadLine();
		}

		public static string GetCourseId()
		{
			Console.WriteLine("Введите id основной версии курса на ulearn. (Когда вы находитесь на любом слайде курса, id можно посмотреть в строке браузера ulearn.me/Course/вот здесь/)");
			Console.Write("> ");
			return Console.ReadLine();
		}

		public static SecureString GetPassword()
		{
			Console.Write("пароль:");
			var password = new SecureString();
			while (true)
			{
				var currentKey = Console.ReadKey(true);
				if (currentKey.Key == ConsoleKey.Enter)
					break;

				if (currentKey.Key == ConsoleKey.Backspace)
				{
					if (password.Length <= 0)
						continue;
					password.RemoveAt(password.Length - 1);
					Console.Write("\b \b");
				}
				else if (currentKey.KeyChar != '\u0000')
					// KeyChar == '\u0000' if the key pressed does not correspond to a printable character, e.g. F1, Pause-Break, etc
				{
					password.AppendChar(currentKey.KeyChar);
					Console.Write("*");
				}
			}

			Console.WriteLine();
			return password;
		}
	}
}