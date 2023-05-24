using System;
using System.Threading.Tasks;
using CourseToolHotReloader.Application;

namespace CourseToolHotReloader.Menu.Commands;

[HotkeyDescription("Полностью перезагрузить последний обновленный курс")]
public class ReloadLastUpdatedCourseCommand : IHotkeyCommand
{
	private readonly IApplication application;

	public ReloadLastUpdatedCourseCommand(IApplication application)
	{
		this.application = application;
	}

	public ConsoleHotkey Hotkey { get; } = new(ConsoleKey.R);

	public Task ExecuteAsync() => application.ReloadCourseAsync(true);
}