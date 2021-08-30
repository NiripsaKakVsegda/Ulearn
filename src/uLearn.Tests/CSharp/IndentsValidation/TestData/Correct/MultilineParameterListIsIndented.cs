namespace Correct
{
	public class MultilineParameterListIsIndented
	{
		public static void Main(
			string[] args)
		{
		}

		public static void M1(
			string[] args,
			string[] args1,
			string[] args3,
			string[] args2)
		{
		}

		public static void M6(
				string[] args,
				string[] args1,
				string[] args3,
				string[] args2)
		{
		}

		public static void M2(
			string[] args,
				string[] args1,
				string[] args3,
				string[] args2)
		{
		}

		public static void M8(
			string[] args, string[] args1,
			string[] args3, string[] args2)
		{
		}

		public static void M7(
				string[] args, string[] args1,
				string[] args3, string[] args2)
		{
		}

		public static void M10(int a, int b,
			string[] args, string[] args1,
			string[] args3, string[] args2)
		{
		}

		public static void M9(int a, int b,
				string[] args, string[] args1,
				string[] args3, string[] args2)
		{
		}

		public static void Main4(
			string[] args,
			string[] args1,
			string[] args3,
			string[] args2
		)
		{
		}

		public static void Main5(
			string[] args,
			string[] args1,
			string[] args3,
			string[] args2
			)
		{
		}

		public static void Main6(
			string[] args,
			string[] args1,
			string[] args3,
			string[] args2
			)
		{
		}
	}
}