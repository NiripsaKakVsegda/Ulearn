namespace uLearn.CSharp.IndentsValidation.TestData.Incorrect
{
	public class BracesMightNotBeAlignedIfOpenBraceDoesntHaveLeadingTrivia
	{
		public static void Main1(string[] args) {

		}

		public static void Main2(string[] args) {
			var a = 0;
		}

		public static void Main3(string[] args)     {
			var a = 0;
		}

		public static void Main4(string[] args)     {
				var a = 0;
		}
	}
}