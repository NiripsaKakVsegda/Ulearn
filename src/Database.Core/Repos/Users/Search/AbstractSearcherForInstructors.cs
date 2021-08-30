using System.Linq;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos.Users.Search
{
	public abstract class AbstractSearcherForInstructors : ISearcher
	{
		protected readonly IUsersRepo usersRepo;
		protected readonly ICourseRolesRepo courseRolesRepo;
		protected readonly IAccessRestrictor accessRestrictor;

		private readonly bool hasSystemAdministratorAccess;
		private readonly bool hasCourseAdminAccess;
		private readonly bool hasInstructorAccessToGroupMembers;
		private readonly bool hasInstructorAccessToCourseInstructors;
		private readonly SearchField searchField;

		protected AbstractSearcherForInstructors(
			IUsersRepo usersRepo, ICourseRolesRepo courseRolesRepo, IAccessRestrictor accessRestrictor,
			bool hasSystemAdministratorAccess, bool hasCourseAdminAccess, bool hasInstructorAccessToGroupMembers, bool hasInstructorAccessToCourseInstructors,
			SearchField searchField
		)
		{
			this.usersRepo = usersRepo;
			this.courseRolesRepo = courseRolesRepo;
			this.accessRestrictor = accessRestrictor;

			this.hasSystemAdministratorAccess = hasSystemAdministratorAccess;
			this.hasCourseAdminAccess = hasCourseAdminAccess;
			this.hasInstructorAccessToGroupMembers = hasInstructorAccessToGroupMembers;
			this.hasInstructorAccessToCourseInstructors = hasInstructorAccessToCourseInstructors;

			this.searchField = searchField;
		}

		public SearchField GetSearchField()
		{
			return searchField;
		}

		public virtual Task<IQueryable<ApplicationUser>> GetSearchScopeAsync(IQueryable<ApplicationUser> users, ApplicationUser currentUser, string courseId)
		{
			return accessRestrictor.RestrictUsersSetAsync(
				users, currentUser, courseId,
				hasSystemAdministratorAccess,
				hasCourseAdminAccess,
				hasInstructorAccessToGroupMembers,
				hasInstructorAccessToCourseInstructors
			);
		}

		public virtual Task<bool> IsAvailableForSearchAsync(ApplicationUser currentUser)
		{
			if (usersRepo.IsSystemAdministrator(currentUser))
				return Task.FromResult(true);

			return courseRolesRepo.HasUserAccessTo_Any_Course(currentUser.Id, CourseRoleType.Instructor);
		}

		public abstract Task<IQueryable<ApplicationUser>> SearchAsync(IQueryable<ApplicationUser> users, string term, bool strict = false, int limit = 1000);
	}
}