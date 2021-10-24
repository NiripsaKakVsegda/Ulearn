using System;

namespace AntiPlagiarism.ConsoleApp.Models
{
	public class Task
	{
		public string Title;
		public Guid Id;
		
		public Task(string title)
		{
			Title = title;
			Id = Guid.NewGuid();
		}
	}
}