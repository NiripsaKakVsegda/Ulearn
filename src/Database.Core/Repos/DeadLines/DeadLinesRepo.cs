using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Database.Repos
{
	public class DeadLinesRepo : IDeadLinesRepo
	{
		private readonly UlearnDb db;

		public DeadLinesRepo(UlearnDb db)
		{
			this.db = db;
		}

		public async Task<List<DeadLine>> GetDeadLines(string courseId, int groupId)
		{
			return await GetDeadLines(courseId, new HashSet<int> { groupId });
		}

		public async Task<List<DeadLine>> GetDeadLines(string courseId, HashSet<int> groupIds)
		{
			return await db.DeadLines
				.Where(d => d.CourseId == courseId && groupIds.Contains(d.GroupId))
				.ToListAsync();
		}

		public async Task<DeadLine> AddDeadLine(string courseId, int groupId, Guid unitId, Guid? slideId, Guid? userId, DateTime date, int scorePercent)
		{
			var deadLine = new DeadLine
			{
				CourseId = courseId,
				GroupId = groupId,
				UnitId = unitId,
				SlideId = slideId,
				UserId = userId,
				Date = date,
				ScorePercent = scorePercent
			};

			db.DeadLines.Add(deadLine);
			await db.SaveChangesAsync();

			return deadLine;
		}

		public async Task UpdateDeadLine(DeadLine deadLine)
		{
			db.DeadLines.Update(deadLine);
			await db.SaveChangesAsync();
		}

		public async Task DeleteDeadLine(DeadLine deadLine)
		{
			db.DeadLines.Remove(deadLine);
			await db.SaveChangesAsync();
		}

		public async Task<DeadLine> GetDeadLineById(Guid deadLineId)
		{
			return await db.DeadLines.FindAsync(deadLineId);
		}

		public async Task<List<DeadLine>> GetDeadLines(string courseId, int groupId, Guid unitId, Guid? slideId, Guid? userId)
		{
			return await GetDeadLines(courseId, new HashSet<int> { groupId }, unitId, slideId, userId);
		}

		public async Task<List<DeadLine>> GetDeadLines(string courseId, HashSet<int> groupIds, Guid unitId, Guid? slideId, Guid? userId)
		{
			return await db.DeadLines.Where(d =>
					d.CourseId == courseId
					&& groupIds.Contains(d.GroupId)
					&& d.UnitId == unitId
					&& (d.SlideId == null || d.SlideId == slideId)
					&& (d.UserId == null || d.UserId == userId))
				.ToListAsync();
		}

		public async Task<List<DeadLine>> GetDeadLines(string courseId, Guid unitId, Guid? slideId, Guid userId)
		{
			var groups = db.Groups
				.Where(g => g.CourseId == courseId && !g.IsDeleted && !g.IsArchived)
				.Select(g => g.Id);
			var userIdAsString = userId.ToString();
			var userGroupsIds = await db.GroupMembers
				.Where(m => groups.Contains(m.GroupId) && m.UserId == userIdAsString)
				.Select(m => m.GroupId)
				.ToListAsync();
			
			return await GetDeadLines(courseId, userGroupsIds.ToHashSet(), unitId, slideId, userId);
		}

		public async Task<List<DeadLine>> GetDeadLinesForUser(string courseId, Guid userId)
		{
			return await db.DeadLines
				.Where(d => d.CourseId == courseId && (d.UserId == null || d.UserId == userId))
				.ToListAsync();
		}
	}
}