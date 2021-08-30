using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos
{
	public interface IAdditionalScoresRepo
	{
		Task<(AdditionalScore, int?)> SetAdditionalScore(string courseId, Guid unitId, string userId, string scoringGroupId, int score, string instructorId);
		Task<Dictionary<Tuple<Guid, string>, int>> GetAdditionalScoresForUser(string courseId, string userId);
		Task<Dictionary<string, AdditionalScore>> GetAdditionalScoresForUser(string courseId, Guid unitId, string userId);
		Task<Dictionary<Tuple<string, string>, AdditionalScore>> GetAdditionalScoresForUsers(string courseId, Guid unitId, IEnumerable<string> usersIds);
		Task<Dictionary<Tuple<string, Guid, string>, AdditionalScore>> GetAdditionalScoresForUsers(string courseId, IEnumerable<string> usersIds);
		Task RemoveAdditionalScores(string courseId, Guid unitId, string userId, string scoringGroupId);
	}
}