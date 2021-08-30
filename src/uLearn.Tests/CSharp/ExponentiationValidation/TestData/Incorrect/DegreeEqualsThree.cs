using System;

namespace uLearn.CSharp.ExponentiationValidation.TestData.Incorrect
{
	public class DegreeEqualsThree
	{
		public double MathPowWithIntNumber()
		{
			return Math.Pow(2, 3);
		}

		public double MathPowWithDoubleNumber()
		{
			return Math.Pow(2, 3.0);
		}
	}
}
