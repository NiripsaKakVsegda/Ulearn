using System;
using System.Threading.Tasks;

namespace Database.Repos
{
	public interface ILtiRequestsRepo
	{
		Task Update(string courseId, string userId, Guid slideId, string ltiRequestJson);
		Task<string> Find(string courseId, string userId, Guid slideId);
	}
}