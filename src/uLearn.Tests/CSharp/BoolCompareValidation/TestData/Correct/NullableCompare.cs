namespace uLearn.CSharp.BoolCompareValidation
{
	public class NullableCompare
	{
		private TestClass testClass = new TestClass();

		public bool CompareWithNullable(bool? a)
		{
			if (false == a)
			{
			}
			return a == true;
		}

		public bool CompareWithNullableCheck(bool? b)
		{
			if (b ?? true)
			{
			}
			if (testClass?.str ?? true)
			{
			}
			return b ?? false;
		}
	}

	class TestClass
	{
		public bool? str;
	}
}