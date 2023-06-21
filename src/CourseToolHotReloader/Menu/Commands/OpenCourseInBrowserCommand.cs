using System;
using System.Threading.Tasks;
using CourseToolHotReloader.Application;

namespace CourseToolHotReloader.Menu.Commands;

[HotkeyDescription("Открыть курс в браузере.")]
public class OpenCourseInBrowserCommand : IHotkeyCommand
{
	private readonly IApplication application;

	public OpenCourseInBrowserCommand(IApplication application)
	{
		this.application = application;
	}

	public ConsoleHotkey Hotkey { get; } = new(ConsoleKey.O, ConsoleModifiers.Control);

	public Task ExecuteAsync()
	{
		application.OpenCourseInBrowser();
		return Task.CompletedTask;
	}
}