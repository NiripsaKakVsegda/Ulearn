using System.Threading.Tasks;

namespace CourseToolHotReloader.Menu;

public interface IHotkeyCommand
{
	ConsoleHotkey Hotkey { get; }

	Task ExecuteAsync();
}