using System;
using System.Collections.Generic;

namespace CourseToolHotReloader.Menu;

public readonly record struct ConsoleHotkey(ConsoleKey Key, ConsoleModifiers Modifiers = 0)
{
	public override string ToString()
	{
		var parts = new List<string>();
		if (Modifiers is ConsoleModifiers.Control)
			parts.Add("Ctrl");
		if (Modifiers is ConsoleModifiers.Alt)
			parts.Add("Alt");
		if (Modifiers is ConsoleModifiers.Shift)
			parts.Add("Shift");
		
		parts.Add(Key.ToString());
		return string.Join(" + ", parts);
	}
}