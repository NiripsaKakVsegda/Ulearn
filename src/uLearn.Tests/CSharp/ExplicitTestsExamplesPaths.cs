using System;
using System.IO;

namespace uLearn.CSharp
{
	public static class ExplicitTestsExamplesPaths
	{
		public static string BasicProgrammingDirectoryPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..",
			"..", "CSharp", "ExampleFiles", "BasicProgramming-master"); // TODO: при запуске ручных тестов создать папку с нужными файлами и указать правильынй путь

		public static string ULearnSubmissionsDirectoryPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..",
			"..", "CSharp", "ExampleFiles", "submissions", "0002");
	}
}