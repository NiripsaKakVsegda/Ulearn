using System.Collections.Generic;
using System.Linq;

namespace Incorrect
{ // внутри фигурных скобок первый токен должен иметь дополнительный отступ
	public class ClassShouldIndentChildrenAlways
	{
	public static void I_Am_Not_Indented(string[] args)
	{

	}
	}

	public enum EnumShouldIndentChildrenAlways
	{
	I_Am_Not_Indented, Me_Too,
	I_Am_Not_Indented1
	}

	public interface InterfaceShouldIndentChildrenAlways
	{
	void I_Am_Not_Indented(params object[] args); void Do();
	}

	public struct StructureShouldIndentChildrenAlways
	{
public static void I_Am_Not_Indented(string[] args)
		{

		}
	}

	public class IfBracesNotOnSameLineTokensInsideBracesShouldBeIndented
	{
		public static void Main2(string[] args)
		{
		var a = 0; var b = 0; var c = 0;
		}

		public static void Main15(string[] args) {
		var a = 0; var b = 0; var c = 0;
		}

		private object a1 = new
		{
		e = 5,
		g = "asd"
		};

		private object a2 = new
		{
	e = 5,
	g = "asd",
	h = new List<int>
	{
	1
	}
		};

		IEnumerable<int> ints1 = new[] { 1, 2, 3 }.SelectMany(
			i => new[] 
			{
			i,
			} 
			.Select(a => a));

		IEnumerable<int> ints3 = new[] { 1, 2, 3 }.SelectMany(
			i => new[] 
			{
		i,
		i
			} 
			.Select(a => a));

		public string Prop2
		{
	get; set;
		}

		private int[] i1 = new[]
		{
		1, 2, 3
		};

		private int[] i2 = new[]
		{
	1, 2, 3
		};

		public List<string> Coll1 => new()
		{
		"a"
		};

		public List<string> Coll2 => new()
		{
	"a"
		};
		public List<string> Coll5 => new()
		{
		"a",
		"b"
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
		};

		Dictionary<int, int> d2 = new()
		{
		{ 1, 2 },
		{
				2,
				3
		}
		};

		Dictionary<int, List<int>> dl = new()
		{
			{
			1, new List<int>
			{
				2, 3
			}
			}
		};
	}
}