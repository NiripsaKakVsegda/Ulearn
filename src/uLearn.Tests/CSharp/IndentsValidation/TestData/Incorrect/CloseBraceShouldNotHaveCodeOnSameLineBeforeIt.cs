using System.Collections.Generic;

namespace uLearn.CSharp.IndentsValidation.TestData.Incorrect
{
	public class CloseBraceShouldNotHaveCodeOnSameLineBeforeIt
	{
		public static void M(string[] args)
		{
			var a = 0
		; }

		public static void M1(string[] args)
		{
			var a = 0
		;      }

		public static void M4(string[] args)
		{
			var b = 0;
			var a = 0
		;     }

		public static void M5(string[] args)
		{
			var b = 0;
			

			for (int i = 0; i < 10; i++)
			{
				var g = 0
			; }

			for (int i = 0; i < 10; i++)
			{
				var
			k = 0; }

			var a = new List<int> { 1, 2, 3 }
		;     }
	}
}