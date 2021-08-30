using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos
{
	public interface ITextsRepo
	{
		Task<TextBlob> AddText(string text);
		Task<TextBlob> GetText(string hash);
		Task<Dictionary<string, string>> GetTextsByHashes(IEnumerable<string> hashes);
	}
}