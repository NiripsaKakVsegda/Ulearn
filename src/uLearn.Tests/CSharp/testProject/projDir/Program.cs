using System;

namespace test
{
	internal static class Program
	{
		private static void Main()
		{
			new Link.L();
			DependenceToOneClass();
			DependenceToAnotherClass();
		}

		private static void DependenceToOneClass()
		{
			if (MeaningOfLifeTask.GetIt() != 42)
				Console.WriteLine("Not OK");
		}

		private static void DependenceToAnotherClass()
		{
			AnotherTask.Return_27();
		}
	}
}