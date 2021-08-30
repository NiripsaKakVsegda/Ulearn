using NUnit.Framework;

namespace uLearn.CSharp.BoolCompareValidation.TestData.Correct
{
	public class UnknownTypes
	{
		public void CheckNoErrorsIfTypesAreUnknown()
		{
			if (new TestCaseAttribute(42).Explicit == true) { }
		}
	}
}
