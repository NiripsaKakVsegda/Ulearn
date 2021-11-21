using System;
using Vostok.Logging.Abstractions;

namespace AntiPlagiarism.ConsoleApp
{
	public class ConsoleWorker
	{
		private ILog log;

		public ConsoleWorker(ILog log)
		{
			this.log = log;
		}
	}
}