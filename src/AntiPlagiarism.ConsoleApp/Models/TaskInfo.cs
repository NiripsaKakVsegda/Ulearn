using System;

namespace AntiPlagiarism.ConsoleApp.Models
{
	public class TaskInfo
	{
		public string Title;
		public Guid Id;
		
		public TaskInfo(string title)
		{
			Title = title;
			Id = Guid.NewGuid();
		}
	}
}