using System;
using System.Reflection;
using CourseToolHotReloader.Menu;

namespace CourseToolHotReloader.Infrastructure;

public static class HotkeyListenerExtensions
{
	public static void PrintHotkeys(this IConsoleMenu listener)
	{
		var commands = listener.GetHotkeyCommands();
		Console.WriteLine("Горячие клавиши консоли:");
		foreach (var command in commands)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write($"{command.Hotkey.ToString(),-15}");
			Console.ResetColor();
			var description = (command.GetType().GetCustomAttribute(typeof(HotkeyDescriptionAttribute)) as HotkeyDescriptionAttribute)?.Description;
			if (description is not null)
				Console.Write($"{description}");
			Console.WriteLine();
		}
		Console.WriteLine();
		
	}
}