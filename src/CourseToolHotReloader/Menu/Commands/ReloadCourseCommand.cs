using System;
using System.Threading.Tasks;
using CourseToolHotReloader.Application;

namespace CourseToolHotReloader.Menu.Commands;

[HotkeyDescription("Полностью перезагрузить курс")]
public class ReloadCourseCommand : IHotkeyCommand
{
	private readonly IApplication application;

	public ReloadCourseCommand(IApplication application)
	{
		this.application = application;
	}

	public ConsoleHotkey Hotkey { get; } = new(ConsoleKey.R, ConsoleModifiers.Control);

	public Task ExecuteAsync() => application.ReloadCourseAsync();
}