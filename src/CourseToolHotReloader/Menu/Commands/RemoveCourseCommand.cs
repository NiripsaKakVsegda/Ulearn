using System;
using System.Threading.Tasks;
using CourseToolHotReloader.Application;

namespace CourseToolHotReloader.Menu.Commands;

[HotkeyDescription("Удалить курс из списка отслеживаемых.")]
public class RemoveCourseCommand : IHotkeyCommand
{
	private readonly IApplication application;

	public RemoveCourseCommand(IApplication application)
	{
		this.application = application;
	}

	public ConsoleHotkey Hotkey { get; } = new(ConsoleKey.D);

	public Task ExecuteAsync()
	{
		application.RemoveCourse();
		return Task.CompletedTask;
	}
}