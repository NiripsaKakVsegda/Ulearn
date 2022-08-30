namespace uLearn.Web.Core.Attributes;

public class IsErrorAttribute : Attribute
{
	public static bool DefaultValue = false;

	public readonly bool IsError;

	public IsErrorAttribute(bool isError)
	{
		IsError = isError;
	}
}