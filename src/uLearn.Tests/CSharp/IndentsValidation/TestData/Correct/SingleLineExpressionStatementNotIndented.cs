using System.Linq;

namespace Correct
{
	public class SingleLineExpressionStatementNotIndented
	{
		public static void Main(string[] args)
		{
			new[] { 1, 2, 3 }.Select(i => i);
			new[] { 1, 2, 3 }.Select(i => i).Where(i => i < 2);
			new[] { 1, 2, 3 }.Select(i => new[] { i }.Select(a => a));
			new[] { 1, 2, 3 }.Select(i => new[] { i }.Select(a => a)).Where(arr => arr.Count() < 2);
		}
	}
}