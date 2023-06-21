using System;
using System.Threading.Tasks;
using CourseToolHotReloader.Application;

namespace CourseToolHotReloader.Menu.Commands;

[HotkeyDescription("Открыть последний обновленный курс в браузере.")]
public class OpenLastUpdatedCourseInBrowserCommand : IHotkeyCommand
{
	private readonly IApplication application;

	public OpenLastUpdatedCourseInBrowserCommand(IApplication application)
	{
		this.application = application;
	}

	public ConsoleHotkey Hotkey { get; } = new(ConsoleKey.O);

	public Task ExecuteAsync()
	{
		application.OpenCourseInBrowser(true);
		return Task.CompletedTask;
	}
}