using System.Threading.Tasks;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Units;

namespace Database.Repos.Flashcards
{
	public class UserFlashcardsUnlockingRepo : IUserFlashcardsUnlockingRepo
	{
		private UlearnDb db;

		public UserFlashcardsUnlockingRepo(UlearnDb db)
		{
			this.db = db;
		}

		public async Task<UserFlashcardsUnlocking> GetUserFlashcardsUnlocking(string userId, Course course, Unit unit)
		{
			var courseId = course.Id.ToLower();
			return await db.UserFlashcardsUnlocking.FirstOrDefaultAsync(x => courseId == x.CourseId && userId == x.UserId && unit.Id == x.UnitId);
		}

		public async Task<UserFlashcardsUnlocking> AddUserFlashcardsUnlocking(string userId, Course course, Unit unit)
		{
			var courseId = course.Id.ToLower();
			var record = new UserFlashcardsUnlocking()
				{ UserId = userId, CourseId = courseId, UnitId = unit.Id };
			db.UserFlashcardsUnlocking.Add(record);

			await db.SaveChangesAsync();

			return await GetUserFlashcardsUnlocking(userId, course, unit);
		}
	}
}