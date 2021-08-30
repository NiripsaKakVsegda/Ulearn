namespace Correct
{
	public class IfBracesNotOnSameLineBracesIndented
	{
		void M0()
		{
			var a = new[]
			{
				1
			};
		}

		void M1()
		{
			var a = new[]
				{
					1
				};
		}

		void M2()
		{
			var a = new[]
					{
						1
					};
		}

		void M122()

		{

		}

		void M1252()

			{

			}

		void M1223()
//asd
		{

		}

		void M12234()
			//asd
		{

		}
	}
}