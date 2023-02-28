using System.Collections.Generic;
using System.Linq;

namespace uLearn.CSharp.IndentsValidation.TestData.Incorrect
{
	public class IfBracesNotOnSameLineFirstLevelTokensInsideBracesShouldBeConsistent
	{ // внутри фигурных скобок все токены верхнего уровня вложенности должны иметь консистентный отступ
		List<List<List<int>>> l2 = new()
		{
			new List<List<int>>
			{
				new()
				{
					1,
				2,
					3
				}
			}
		};

		private int[] a =
		{
			1,
			2,
				3
		};

		private int[] a10 =
		{
				1,
				2,
					3
		};

		Dictionary<int, int> d2 = new()
		{
			{ 1, 2 },
			{
				2,
			3
			}
		};

		Dictionary<int, int> d1 = new()
		{
			{ 1, 2 },
				{ 2, 3 },
		};

		List<List<List<int>>> l = new()
		{
			new List<List<int>>
			{
				new()
				{
					1,
						2,
				3
				}
			}
		};

		public List<string> Coll6 => new()
		{
			"a",
		"b"
		};

		IEnumerable<int> ints2 = new[] { 1, 2, 3 }.SelectMany(
			i => new[]
				{
					i,
				i
				});
	}
}