using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos
{
	public interface IDeadLinesRepo
	{
		Task<List<DeadLine>> GetDeadLines(string courseId, int groupId);
		Task<List<DeadLine>> GetDeadLines(string courseId, HashSet<int> groupIds);
		Task<List<DeadLine>> GetDeadLines(string courseId, int groupId, Guid unitId, Guid? slideId, Guid? userId);
		Task<List<DeadLine>> GetDeadLines(string courseId, HashSet<int> groupIds, Guid unitId, Guid? slideId, Guid? userId);
		Task<List<DeadLine>> GetDeadLines(string courseId, Guid unitId, Guid? slideId, Guid userId);
		Task<List<DeadLine>> GetDeadLinesForUser(string courseId, Guid userId);
		Task<DeadLine> AddDeadLine(string courseId, int groupId, Guid unitId, Guid? slideId, Guid? userId, DateTime date, int scorePercent);
		Task UpdateDeadLine(DeadLine deadLine);
		Task DeleteDeadLine(DeadLine deadLine);
		Task<DeadLine> GetDeadLineById(Guid deadLineId);
	}
}