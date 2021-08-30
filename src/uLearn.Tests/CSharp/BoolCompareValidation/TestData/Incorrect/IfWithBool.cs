namespace uLearn.CSharp.BoolCompareValidation.TestData.Incorrect
{
	public class IfWithBool
	{
		void CheckIfStatementWithErrors(bool a)
		{
			if (a == true) {}
			if (a != true){}
			if (a == false){}
			if (a != false){}
			if (true == a){}
			if (false == a){}
			if (true != a){}
			if (false != a){}
		}
	}
}