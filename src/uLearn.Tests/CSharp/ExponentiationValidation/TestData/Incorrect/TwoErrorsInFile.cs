using System;

namespace uLearn.CSharp.ExponentiationValidation.TestData.Incorrect
{
	public class TwoErrorsInFile
	{
		public double A()
		{
			var b = Math.Pow(2, 2);
			return Math.Pow(2, 3);
		}
	}
}
