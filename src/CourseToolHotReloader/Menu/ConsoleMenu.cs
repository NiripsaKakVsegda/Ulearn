using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CourseToolHotReloader.Application;
using Vostok.Logging.Abstractions;

namespace CourseToolHotReloader.Menu;

public interface IConsoleMenu
{
	Task StartListeningAsync(CancellationToken cancellationToken = default);
	IEnumerable<IHotkeyCommand> GetHotkeyCommands();
}

public class ConsoleMenu : IConsoleMenu
{
	private readonly IReadOnlyDictionary<ConsoleHotkey, IHotkeyCommand> commands;
	private readonly IReadOnlyList<IKeysExtension> keysExtensions;
	private readonly ILogManager? logManager;
	private readonly IApplication application;

	private static ILog Log => LogProvider.Get().ForContext<IConsoleMenu>();

	public ConsoleMenu(
		IEnumerable<IHotkeyCommand> commands,
		IEnumerable<IKeysExtension> keysExtensions,
		IApplication application,
		ILogManager? logManager = null
	)
	{
		this.keysExtensions = keysExtensions.ToList();
		this.commands = commands.ToDictionary(command => command.Hotkey);
		this.application = application;
		this.logManager = logManager;
	}

	public async Task StartListeningAsync(CancellationToken cancellationToken = default)
	{
		Log.Info("Listening started.");
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ReadKeyWithCancellation(cancellationToken);
			await OpenMenuAsync(cancellationToken);
		}
		// ReSharper disable once FunctionNeverReturns
	}

	public IEnumerable<IHotkeyCommand> GetHotkeyCommands() => commands.Values;

	private async Task OpenMenuAsync(CancellationToken cancellationToken)
	{
		using (logManager?.PauseLogging())
		{
			cancellationToken.ThrowIfCancellationRequested();
			Console.WriteLine();
			application.PrintCourses();
			Console.WriteLine();
			PrintHotkeys();
			Console.WriteLine("Нажмите Escape, чтобы закрыть меню и возобновить печать логов в консоль.");
			Console.WriteLine();

			var keyInfo = ReadKeyWithCancellation(cancellationToken);
			var key = keyInfo.Key;
			foreach (var keysExtension in keysExtensions)
				if (keysExtension.TryGetConsoleKey(keyInfo, out var outKey))
				{
					key = outKey;
					break;
				}

			var hotkey = new ConsoleHotkey(key, keyInfo.Modifiers);
			if (!commands.TryGetValue(hotkey, out var command))
			{
				Console.WriteLine("Меню закрыто. Печать логов возобновлена.");
				return;
			}

			Log.Info($"Hotkey detected: {hotkey.ToString()}");
			cancellationToken.ThrowIfCancellationRequested();
			await command.ExecuteAsync();
			Console.WriteLine("Меню закрыто. Печать логов возобновлена.");
		}
	}

	private void PrintHotkeys()
	{
		Console.WriteLine("Горячие клавиши меню:");
		foreach (var command in commands.Values)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write($"{command.Hotkey.ToString(),-15}");
			Console.ResetColor();

			var description = (command
					.GetType()
					.GetCustomAttribute(typeof(HotkeyDescriptionAttribute)) as HotkeyDescriptionAttribute
				)?.Description;
			if (description is not null)
				Console.Write($"{description}");
			Console.WriteLine();
		}
	}

	private static ConsoleKeyInfo ReadKeyWithCancellation(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		var key = Console.ReadKey(true);
		if (cancellationToken.IsCancellationRequested)
			Console.Write(key.KeyChar);
		cancellationToken.ThrowIfCancellationRequested();
		return key;
	}
}