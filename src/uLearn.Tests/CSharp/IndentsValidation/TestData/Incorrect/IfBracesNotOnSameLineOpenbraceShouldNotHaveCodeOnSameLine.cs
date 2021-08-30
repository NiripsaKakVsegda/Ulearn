using System.Collections.Generic;

namespace uLearn.CSharp.IndentsValidation.TestData.Incorrect
{
	public class IfBracesNotOnSameLineOpenbraceShouldNotHaveCodeOnSameLine
	{ // после открывающей фигурной скобки на той же строке не должно быть кода
		public static void Main17(string[] args) { var a = 0;
			var b = 0; var c = 0;
		}

		public static void Main3(string[] args)
		{    var a = 0; var b = 0; var c = 0;
		}

		public static void Main10(string[] args)
		{var a = 0; var b = 0; var c = 0;
		}

		public static void Main7(string[] args)
		{var a = 0;
			var b = 0;
			var c = 0;
		}

		Dictionary<int, int> dl = new Dictionary<int, int>
		{
			{ 1, 2
			}
		};

		Dictionary<int, int> dl1 = new Dictionary<int, int>
		{
			{ 1,
				2
			}
		};
	}
}