using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos
{
	public interface IDeadLinesRepo
	{
		Task<List<DeadLine>> GetDeadLines(string courseId, HashSet<int> groupIds, Guid? unitId = null, Guid? slideId = null, Guid? userId = null);
		Task<List<DeadLine>> GetDeadLines(string courseId, int groupId, Guid? unitId = null, Guid? slideId = null, Guid? userId = null);
		Task<List<DeadLine>> GetDeadLinesForUser(string courseId, Guid userId, Guid? unitId = null, Guid? slideId = null);
		Task<DeadLine> AddDeadLine(string courseId, int groupId, Guid unitId, Guid? slideId, Guid? userId, DateTime date, int scorePercent);
		Task UpdateDeadLine(DeadLine deadLine);
		Task DeleteDeadLine(DeadLine deadLine);
		Task<DeadLine> GetDeadLineById(Guid deadLineId);
	}
}