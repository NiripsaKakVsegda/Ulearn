using System.Linq;
using System.Text;
using Ulearn.Common.Extensions;

namespace uLearn.CSharp
{
	public static class SpellingValidatorPrimitives
	{
		public static string MakeTypeNameAbbreviation(this string typeName)
		{
			return string.Join("", typeName.SplitByCamelCase().Select(word => word[0]));
		}
	}
}