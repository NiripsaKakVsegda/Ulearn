using System;

namespace CourseToolHotReloader.Menu;

public interface IKeysExtension
{
	bool TryGetConsoleKey(ConsoleKeyInfo keyInfo, out ConsoleKey key);
}