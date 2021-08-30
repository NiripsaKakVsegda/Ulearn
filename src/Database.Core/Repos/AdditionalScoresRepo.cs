using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Database.Repos
{
	public class AdditionalScoresRepo : IAdditionalScoresRepo
	{
		private readonly UlearnDb db;

		public AdditionalScoresRepo(UlearnDb db)
		{
			this.db = db;
		}

		/// <summary>
		/// Returns new AdditionalScore and old score (or null if not exists)
		/// </summary>
		public async Task<(AdditionalScore, int?)> SetAdditionalScore(string courseId, Guid unitId, string userId, string scoringGroupId, int score, string instructorId)
		{
			int? oldScore = null;
			using (var transaction = db.Database.BeginTransaction())
			{
				var scores = await db.AdditionalScores.Where(s => s.CourseId == courseId && s.UnitId == unitId && s.UserId == userId && s.ScoringGroupId == scoringGroupId).ToListAsync();
				if (scores.Any())
					oldScore = scores.First().Score;
				db.AdditionalScores.RemoveRange(scores);

				var additionalScore = new AdditionalScore
				{
					CourseId = courseId,
					UnitId = unitId,
					UserId = userId,
					ScoringGroupId = scoringGroupId,
					Score = score,
					InstructorId = instructorId,
					Timestamp = DateTime.Now,
				};
				db.AdditionalScores.Add(additionalScore);

				await db.SaveChangesAsync();
				await transaction.CommitAsync();

				return (additionalScore, oldScore);
			}
		}

		/* Dictionary<(unitId, scoringGroupId), additionalScore> */
		public async Task<Dictionary<Tuple<Guid, string>, int>> GetAdditionalScoresForUser(string courseId, string userId)
		{
			return (await db.AdditionalScores
				.Where(s => s.CourseId == courseId && s.UserId == userId)
				.ToListAsync().ConfigureAwait(false))
				.ToDictSafe(s => Tuple.Create(s.UnitId, s.ScoringGroupId), s => s.Score);
		}

		public async Task<Dictionary<string, AdditionalScore>> GetAdditionalScoresForUser(string courseId, Guid unitId, string userId)
		{
			return (await db.AdditionalScores
				.Where(s => s.CourseId == courseId && s.UnitId == unitId && s.UserId == userId)
				.ToListAsync())
				.ToDictSafe(s => s.ScoringGroupId, s => s);
		}

		/* Dictionary<(userId, scoringGroupId), additionalScore> */
		public async Task<Dictionary<Tuple<string, string>, AdditionalScore>> GetAdditionalScoresForUsers(string courseId, Guid unitId, IEnumerable<string> usersIds)
		{
			return (await db.AdditionalScores
				.Where(s => s.CourseId == courseId && s.UnitId == unitId && usersIds.Contains(s.UserId))
				.ToListAsync())
				.ToDictSafe(s => Tuple.Create(s.UserId, s.ScoringGroupId), s => s);
		}

		/* Dictionary<(userId, unitId, scoringGroupId), additionalScore> */
		public async Task<Dictionary<Tuple<string, Guid, string>, AdditionalScore>> GetAdditionalScoresForUsers(string courseId, IEnumerable<string> usersIds)
		{
			return (await db.AdditionalScores
				.Where(s => s.CourseId == courseId && usersIds.Contains(s.UserId))
				.ToListAsync())
				.ToDictSafe(s => Tuple.Create(s.UserId, s.UnitId, s.ScoringGroupId), s => s);
		}

		public async Task RemoveAdditionalScores(string courseId, Guid unitId, string userId, string scoringGroupId)
		{
			var scores = db.AdditionalScores.Where(s => s.CourseId == courseId && s.UnitId == unitId && s.UserId == userId && s.ScoringGroupId == scoringGroupId);
			db.AdditionalScores.RemoveRange(scores);
			await db.SaveChangesAsync();
		}
	}
}