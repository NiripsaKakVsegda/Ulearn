using System;

namespace AntiPlagiarism.ConsoleApp.Models
{
	public class UserActions
	{
		public string Command;
		public Action Action;
		public string Help;
		
		public UserActions(string command, Action action, string help)
		{
			Command = command;
			Action = action;
			Help = help;
		}
	}
}