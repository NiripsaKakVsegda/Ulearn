using System;
using System.Threading.Tasks;
using CourseToolHotReloader.Application;

namespace CourseToolHotReloader.Menu.Commands;

[HotkeyDescription("Войти в аккаунт")]
public class LoginCommand : IHotkeyCommand
{
	private readonly IApplication application;

	public LoginCommand(IApplication application)
	{
		this.application = application;
	}

	public ConsoleHotkey Hotkey { get; } = new(ConsoleKey.L);

	public Task ExecuteAsync() => application.LoginAsync();
}