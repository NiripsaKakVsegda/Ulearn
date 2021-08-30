namespace uLearn.CSharp.BoolCompareValidation.TestData.Incorrect
{
	public class TernaryOperatorWithBool
	{
		void CheckTernaryOperatorsWithErrors(bool a, bool b)
		{
			var expression = true == a ? 1 : 0;
			expression = true != a ? 1 : 0;
			expression = false == a ? 1 : 0;
			expression = false != a ? 1 : 0;
			expression = a == true ? 1 : 0;
			expression = a != true ? 1 : 0;
			expression = a == false ? 1 : 0;
			expression = a != false ? 1 : 0;
		}
	}
}