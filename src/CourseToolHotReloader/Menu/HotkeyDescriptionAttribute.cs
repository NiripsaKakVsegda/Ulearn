using System;

namespace CourseToolHotReloader.Menu;

[AttributeUsage(AttributeTargets.Class)]
public class HotkeyDescriptionAttribute : Attribute
{
	public readonly string Description;

	public HotkeyDescriptionAttribute(string description)
	{
		Description = description;
	}
}