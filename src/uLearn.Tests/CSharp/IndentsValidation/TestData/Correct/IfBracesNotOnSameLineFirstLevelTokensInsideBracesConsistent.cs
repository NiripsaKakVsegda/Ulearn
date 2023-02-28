using System.Collections.Generic;
using System.Linq;

namespace Correct
{
	public class IfBracesNotOnSameLineFirstLevelTokensInsideBracesConsistent
	{
		public static void Main4(string[] args)
		{
			var a10 = new[]
			{
					1,
					2,
					3
			};

			for (var i = 0; i < 10; i++) { 
				var a = new {
					name = "asd",
					v = 1
				};
				var b = new {
					name = "asd",
					v = 1
				};
			}

			new[] { 1, 2, 3 }.Select(
				i => new[]
					{
						i,
						i,
						i
					});
		}
		private object a1 = new
		{
			e = 5,
			g = "asd"
		};
		public string Prop2
		{
			get;
			set;
		}
		private int[] i1 = new[]
		{
			1,
			2,
			3
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
		Dictionary<int, int> d1 = new()
		{
			{ 1, 2 },
			{ 2, 3 },
			{ 2, 3 },
			{ 2, 3 },
		};
		Dictionary<int, List<int>> dl1 = new()
		{
			{
				1, new List<int>
				{
					2,
					3
				}
			},
			{
				2, new List<int>()
				{
					5,
					6
				}
			}
		};

		public static void Main5(string[] args)
		{
			for (var i = 0; i < 10; i++)
			for (var j = 0; j < 10; j++)
			for (var k = 0; j < 10; j++)
			for (var l = 0; j < 10; j++)
			{
			}
		}

		public static void Main6(string[] args)
		{
			for (var i = 0; i < 10; i++)
			for (var j = 0; j < 10; j++)
			{
				for (var k = 0; j < 10; j++)
				for (var l = 0; j < 10; j++)
				{
					for (var m = 0; j < 10; j++)
					for (var n = 0; j < 10; j++)
					{
					}
				}
			}
		}

		void M()
		{
			var a = new[]
			{
				1
			};
		}

		void M1()
		{
			var a = new[]
				{
					1
				};
		}

		void M2()
		{
			var a = new[]
					{
						1
					};
		}

		void M3
		(
			string b,
			string c
		)
		{
		}
	}
}