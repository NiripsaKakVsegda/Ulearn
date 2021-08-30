using System;

namespace uLearn.CSharp.ExponentiationValidation.TestData.Correct
{
	public class DegreeNotEqualsToTwoOrThree
	{
		public double A()
		{
			return Math.Pow(2, 5);
		}

		public double B()
		{
			return Math.Pow(2, 0.5);
		}

		public double C()
		{
			return Math.Pow(2, 1);
		}
	}
}
