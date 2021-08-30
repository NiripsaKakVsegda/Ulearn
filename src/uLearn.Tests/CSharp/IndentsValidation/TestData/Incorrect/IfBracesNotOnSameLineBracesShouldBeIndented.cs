namespace Incorrect
{
	public class IfBracesNotOnSameLineBracesShouldBeIndented
	{ // фигурные скобки должны иметь отступ > чем отступ родителя
		void M0()
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

		void M21()
		{
			var a = new[]
{
				1
};
		}

		void M122()

	{
		
	}

		void M124()

{
	
}

		void M1223()
//asd
	{
		
	}
	}
}