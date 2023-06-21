using System;
using System.Threading.Tasks;
using CourseToolHotReloader.Application;

namespace CourseToolHotReloader.Menu.Commands;

[HotkeyDescription("Добавить новый курс в список отслеживаемых.")]
public class AddCourseCommand : IHotkeyCommand
{
	private readonly IApplication application;

	public AddCourseCommand(IApplication application)
	{
		this.application = application;
	}

	public ConsoleHotkey Hotkey { get; } = new(ConsoleKey.A);

	public Task ExecuteAsync() => application.AddNewCourseAsync();
}