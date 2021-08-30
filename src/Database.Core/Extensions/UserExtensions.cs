using Database.Models;
using Database.Repos.Users;

namespace Database.Extensions
{
	public static class UserExtensions
	{
		public static bool IsUlearnBot(this ApplicationUser user)
		{
			return user.UserName == UsersRepo.UlearnBotUsername;
		}
	}
}