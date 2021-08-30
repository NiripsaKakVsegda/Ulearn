using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using NinjaNye.SearchExtensions;

namespace Database.Repos.Users.Search
{
	public class SearcherByNames : AbstractSearcherForInstructors
	{
		public SearcherByNames(IUsersRepo usersRepo, ICourseRolesRepo courseRolesRepo, IAccessRestrictor accessRestrictor)
			: base(usersRepo, courseRolesRepo, accessRestrictor,
				true, true, true, true,
				SearchField.Name
			)
		{
		}

		public override async Task<IQueryable<ApplicationUser>> SearchAsync(IQueryable<ApplicationUser> users, string term, bool strict = false, int limit = 1000)
		{
			if (string.IsNullOrEmpty(term))
				return Enumerable.Empty<ApplicationUser>().AsQueryable();

			if (strict)
				return users.Search(u => u.FirstName, u => u.LastName).EqualTo(term);

			var namesSubstring = term.ToLower();
			return users.Search(u => u.Names).Containing(namesSubstring).OrderBy(u => u.Id).Take(limit);
		}
	}
}