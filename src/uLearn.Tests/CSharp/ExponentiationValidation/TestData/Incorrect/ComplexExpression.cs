using System;

namespace uLearn.CSharp.ExponentiationValidation.TestData.Incorrect
{
	public class ComplexExpression
	{
		public double A()
		{
			double Lambda() => 2 * Math.Pow(Math.PI * 100500 - 1, 2);
			return Lambda();
		}
	}
}
