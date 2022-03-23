using System;

namespace uLearn.CSharp.SpellingValidation.TestData.Correct
{
	public class FileWithExceptions
	{
		public int Pos => 1;

		public Func<int> GetFunc(int arg)
		{
			return () => arg;
		}
	}
}
