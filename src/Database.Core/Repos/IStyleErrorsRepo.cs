using System.Collections.Generic;
using System.Threading.Tasks;
using Ulearn.Core.CSharp;

namespace Database.Repos
{
	public interface IStyleErrorsRepo
	{
		Task<bool> IsStyleErrorEnabled(StyleErrorType errorType);
		Task<Dictionary<StyleErrorType, bool>> GetStyleErrorSettings();
		Task EnableStyleError(StyleErrorType errorType, bool isEnabled);
	}
}