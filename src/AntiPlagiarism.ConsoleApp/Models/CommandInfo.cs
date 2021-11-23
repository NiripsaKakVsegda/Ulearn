using System;

namespace AntiPlagiarism.ConsoleApp.Models
{
	public class CommandInfo
	{
		public string Command;
		public Action Action;
		public string Help;
		
		public CommandInfo(string command, Action action, string help)
		{
			Command = command;
			Action = action;
			Help = help;
		}
	}
}