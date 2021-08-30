using System;

namespace uLearn.CSharp.ExponentiationValidation.TestData.Correct
{
	public class DegreeIsWrappedByVariable
	{
		public double A()
		{
			var b = 2.0;
			return Math.Pow(2, b);
		}
	}
}
