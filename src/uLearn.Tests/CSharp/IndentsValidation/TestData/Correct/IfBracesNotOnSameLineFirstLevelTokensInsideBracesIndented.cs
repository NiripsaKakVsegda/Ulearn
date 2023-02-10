using System.Collections.Generic;

namespace Correct
{
	public class IfBracesNotOnSameLineFirstLevelTokensInsideBracesIndented
	{
		public static void Main3(string[] args)
		{
			var a = new {
				name = "asd"
			};
		}

		public static void Mai500(string[] args)
		{
			var a = new[] { 1 }; var b = new[] {
				2, 3
			};
		}

		public static void Main4(string[] args)
		{
			for (var i = 0; i < 10; i++) { 
				var a = new {
					name = "asd"
				};
			}
		}
		private object a1 = new
		{
			e = 5
		};
		List<List<List<int>>> l = new()
		{
			new List<List<int>>
			{
				new()
				{
					1
				}
			}
		};
		Dictionary<int, int> d1 = new()
		{
			{ 1, 2 }
		};
		Dictionary<int, List<int>> dl1 = new()
		{
			{
				1, new List<int>
				{
					2
				}
			}
		};

		public static void Main5(string[] args)
		{
			for (var i = 0; i < 10; i++)
			for (var j = 0; j < 10; j++)
			{
				var a = new
				{
					name = "asd"
				};
			}
		}

		public static void Main7(string[] args)
		{
			for (var i = 0; i < 10; i++)
			{
				for (var j = 0; j < 10; j++)
				{
					var a = new
					{
						name = "asd"
					};
				}
			}
		}
	}

	public enum EnumIndentsChildrenAlways
	{
		I_Am_Indented, XXX
	}
	
	public interface InterfaceIndentsChildrenAlways
	{
		void I_Am_Indented(params object[] args); void O();
	}

	public struct StructureIndentsChildrenAlways
	{
		public static void I_Am_Indented(string[] args)
		{

		}
	}
}