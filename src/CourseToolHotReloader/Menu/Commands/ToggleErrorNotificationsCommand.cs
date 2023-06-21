using System;
using System.Threading.Tasks;
using CourseToolHotReloader.Application;

namespace CourseToolHotReloader.Menu.Commands;

[HotkeyDescription("Включить/выключить звуковые уведомления об ошибке загрузки курса.")]
public class ToggleErrorNotificationsCommand : IHotkeyCommand
{
	private readonly IApplication application;

	public ToggleErrorNotificationsCommand(IApplication application)
	{
		this.application = application;
	}

	public ConsoleHotkey Hotkey { get; } = new(ConsoleKey.S);

	public Task ExecuteAsync()
	{
		application.ToggleErrorNotifications();
		return Task.CompletedTask;
	}
}