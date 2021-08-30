using System;
using System.Threading;
using System.Threading.Tasks;

namespace CourseToolHotReloader
{
	internal class ConsoleSpinner
	{
		private int counter;
		private readonly string[] sequence;
		private readonly string space;
		private CancellationTokenSource cancelTokenSource;

		private ConsoleSpinner()
		{
			counter = 0;
			sequence = new[] { " /", " -", " \\", " |" };
			//sequence = new[] { "░", "▒", "▓" };
			//sequence = new[] { "╚═", "═╝", "═╗", "╔═" };
			//sequence = new[] { "█", "▄" };
			//sequence = new[] { ".", "o", "0", "o"};
			//sequence = new[] { "+", "x", "  +", "  x", "   +", "   x", "    +", "    x", "     +", "     x"  };
			//sequence = new[] { "█        ", "██       ", "███      ", "████     ", "█████    ", "██████   ", "███████  ", "████████ ", "█████████", "████████ ", "███████  ", "██████   ", "█████    ", "███      ", "██       " };
			//sequence = new[] { "█        ", " █       ", "  █      ", "   █     ", "    █    ", "     █   ", "      █  ", "       █ ", "        █", "       █ ", "      █  ", "     █   ", "    █    ", "  █      ", " █       " };
			//sequence = new[] { "V", "<", "^", ">" };
			//sequence = new[] { ".   ", "..  ", "... ", "...." };
			space = "  ";                
		}

		public static ConsoleSpinner CreateAndRunWithText(string text)
		{
			Console.Write(text);
			var cs = new ConsoleSpinner();
			cs.Start();
			return cs;
		}

		private void Start()
		{
			cancelTokenSource = new CancellationTokenSource();
			var ct = cancelTokenSource.Token;
			Task.Run(async () =>
			{
				Console.CursorVisible = false;
				while (true)
				{
					await Task.Delay(100);

					if (ct.IsCancellationRequested)
					{
						Console.CursorVisible = true;
						return;
					}

					Turn();
				}
			}, cancelTokenSource.Token);
		}

		public void Stop()
		{
			cancelTokenSource.Cancel();
			cancelTokenSource.Dispose();
			Console.Write(space);
			Console.WriteLine();
		}

		private void Turn()
		{
			counter++;

			if (counter >= sequence.Length)
				counter = 0;

			Console.Write(sequence[counter]);
			Console.SetCursorPosition(Console.CursorLeft - sequence[counter].Length, Console.CursorTop);
		}
	}
}