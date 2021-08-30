using System.Linq;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos.Users.Search
{
	public class SearcherByEmail : AbstractSearcherForInstructors
	{
		public SearcherByEmail(IUsersRepo usersRepo, ICourseRolesRepo courseRolesRepo, IAccessRestrictor accessRestrictor)
			: base(usersRepo, courseRolesRepo, accessRestrictor,
				true, true, true, false,
				SearchField.Email
			)
		{
		}

		public override async Task<IQueryable<ApplicationUser>> SearchAsync(IQueryable<ApplicationUser> users, string term, bool strict = false, int limit = 1000)
		{
			/* Don't search if not email */
			if (!term.Contains("@") || !term.Contains("."))
				return Enumerable.Empty<ApplicationUser>().AsQueryable();

			/* This searcher works identically in strict and non-strict mode */
			return users.Where(u => u.Email == term);
		}
	}
}