using System.Threading.Tasks;
using Database.Models;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Units;

namespace Database.Repos.Flashcards
{
	public interface IUserFlashcardsUnlockingRepo
	{
		Task<UserFlashcardsUnlocking> GetUserFlashcardsUnlocking(string userId, Course course, Unit unit);
		Task<UserFlashcardsUnlocking> AddUserFlashcardsUnlocking(string userId, Course course, Unit unit);
	}
}