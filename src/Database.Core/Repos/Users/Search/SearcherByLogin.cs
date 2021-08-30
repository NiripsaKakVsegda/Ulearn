using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using NinjaNye.SearchExtensions;

namespace Database.Repos.Users.Search
{
	public class SearcherByLogin : AbstractSearcherForInstructors
	{
		public SearcherByLogin(IUsersRepo usersRepo, ICourseRolesRepo courseRolesRepo, IAccessRestrictor accessRestrictor)
			: base(usersRepo, courseRolesRepo, accessRestrictor,
				true, true, true, true,
				SearchField.Login
			)
		{
		}

		public override async Task<IQueryable<ApplicationUser>> SearchAsync(IQueryable<ApplicationUser> users, string term, bool strict = false, int limit = 1000)
		{
			if (string.IsNullOrEmpty(term))
				return Enumerable.Empty<ApplicationUser>().AsQueryable();

			if (strict)
				return users.Search(u => u.UserName).EqualTo(term);

			var namesPrefix = term.ToLower() + " ";
			return users.Search(u => u.Names).StartsWith(namesPrefix).OrderBy(u => u.Id).Take(limit);
		}
	}
}