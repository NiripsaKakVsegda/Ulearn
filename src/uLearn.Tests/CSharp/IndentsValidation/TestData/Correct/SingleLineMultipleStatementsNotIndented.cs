using System.Collections.Generic;

namespace Correct
{
	public class SingleLineMultipleStatementsNotIndented
	{
		private int a = 0; private int b = 0; private int c = 0;
		private int d = 0; private int e, f, g = 0; private List<string> h = new List<string> {"a", "b"};

		public static void Main(string[] args)
		{
			var a = 0; var b = 0; var c = 0;
			var a1 = 0; var b2 = 0; var c3 = 0;
		}
	}
}