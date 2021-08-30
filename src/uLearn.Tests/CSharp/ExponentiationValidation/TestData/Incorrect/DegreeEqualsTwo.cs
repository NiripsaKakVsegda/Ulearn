using System;

namespace uLearn.CSharp.ExponentiationValidation.TestData.Incorrect
{
	public class DegreeEqualsTwo
	{
		public double MathPowWithIntNumber()
		{
			return Math.Pow(2, 2);
		}

		public double MathPowWithDoubleNumber()
		{
			return Math.Pow(2, 2.0);
		}
	}
}
