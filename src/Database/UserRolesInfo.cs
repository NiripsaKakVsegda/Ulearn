using System.Collections.Generic;

namespace Database
{
	public class UserRolesInfo
	{
		public string UserId { get; set; }
		public string UserName { get; set; }
		public string UserVisibleName { get; set; }
		public List<string> Roles { get; set; }
	}
}