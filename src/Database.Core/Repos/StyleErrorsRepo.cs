using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Ulearn.Core.CSharp;

namespace Database.Repos
{
	public class StyleErrorsRepo : IStyleErrorsRepo
	{
		private readonly UlearnDb db;
		private Dictionary<StyleErrorType, bool> settingsCache;

		public StyleErrorsRepo(UlearnDb db)
		{
			this.db = db;
		}

		public async Task<bool> IsStyleErrorEnabled(StyleErrorType errorType)
		{
			await CacheSettings();
			if (settingsCache.ContainsKey(errorType))
				return settingsCache[errorType];

			/* By default all style validation errors are enabled */
			return true;
		}

		public async Task<Dictionary<StyleErrorType, bool>> GetStyleErrorSettings()
		{
			await CacheSettings();
			var allErrorTypes = typeof(StyleErrorType).GetEnumValues().Cast<StyleErrorType>();

			var result = new Dictionary<StyleErrorType, bool>();
			foreach (var errorType in allErrorTypes)
				result[errorType] = await IsStyleErrorEnabled(errorType);

			return result;
		}

		public async Task EnableStyleError(StyleErrorType errorType, bool isEnabled)
		{
			var oldSettings = await db.StyleErrorSettings.FirstOrDefaultAsync(s => s.ErrorType == errorType);
			if (oldSettings == null)
				db.StyleErrorSettings.Add(new StyleErrorSettings
				{
					ErrorType = errorType,
					IsEnabled = isEnabled,
				});
			else
			{
				oldSettings.IsEnabled = isEnabled;
			}

			await db.SaveChangesAsync();
		}

		private async Task CacheSettings()
		{
			if (settingsCache == null)
				settingsCache = await db.StyleErrorSettings.ToDictionaryAsync(s => s.ErrorType, s => s.IsEnabled);
		}
	}
}