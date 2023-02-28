using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ApprovalTests.Namers;

namespace uLearn
{
	// Библиотека ApprovalTests предполагает, что approval-файлы лежат в тех же папках, что и исходный код теста.
	// Расположение исходного кода берется из StackTrace. А в StackTrace указываются пути до файлов в момент билда.
	// Билд и запуск тестов в TC не обязаны происходить в одной и той же папке.
	// Этот класс исправляет пути до папки с исходным кодом, чтобы ApprovalTests брали из них approval-файлы.
	public class RelativeUnitTestFrameworkNamer : UnitTestFrameworkNamer
	{
		private static readonly DirectoryInfo assemblyLocation = new(Path.GetDirectoryName(Assembly.GetAssembly(typeof(RelativeUnitTestFrameworkNamer)).Location));
		private static readonly DirectoryInfo projectSourceCodeLocation;

		static RelativeUnitTestFrameworkNamer()
		{
			projectSourceCodeLocation = assemblyLocation;
			while (projectSourceCodeLocation.Name != "bin")
				projectSourceCodeLocation = projectSourceCodeLocation.Parent;
			projectSourceCodeLocation = projectSourceCodeLocation.Parent;
		}

		public override string SourcePath
		{
			get
			{
				var pathRelativeSourceCodeDirectory = new List<string>();
				var path = new DirectoryInfo(stackTraceParser.SourcePath);
				while (path.Name != projectSourceCodeLocation.Name)
				{
					pathRelativeSourceCodeDirectory.Insert(0, path.Name);
					path = path.Parent;
				}
				return Path.Combine(new []{projectSourceCodeLocation.FullName}.Concat(pathRelativeSourceCodeDirectory).Append(Subdirectory).ToArray());
			}
		}
	}
}