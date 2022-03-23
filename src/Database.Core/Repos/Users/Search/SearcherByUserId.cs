using System.Linq;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos.Users.Search
{
	public class SearcherByUserId : ISearcher
	{
		public SearchField GetSearchField()
		{
			return SearchField.UserId;
		}

		public Task<IQueryable<ApplicationUser>> GetSearchScopeAsync(IQueryable<ApplicationUser> users, ApplicationUser currentUser, string courseId)
		{
			return Task.FromResult(users);
		}

		public Task<bool> IsAvailableForSearchAsync(ApplicationUser currentUser)
		{
			return Task.FromResult(true);
		}

		public Task<IQueryable<ApplicationUser>> SearchAsync(IQueryable<ApplicationUser> users, string term, bool strict = false, int limit = 1000)
		{
			/* Don't search by userId by first 1..4 symbols */
			if (term.Length < 5)
				return Task.FromResult(Enumerable.Empty<ApplicationUser>().AsQueryable());

			/* This searcher works identically in strict and non-strict mode */
			return Task.FromResult(users.Where(u => u.Id == term));
		}
	}
}