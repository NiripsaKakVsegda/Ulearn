using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using Database.Extensions;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Database.Repos;

public class SelfCheckupsRepo : ISelfCheckupsRepo
{
	private readonly UlearnDb db;

	public SelfCheckupsRepo(UlearnDb db)
	{
		this.db = db;
	}

	public async Task<List<SelfCheckup>> GetSelfCheckups(string userId, string courseId, Guid slideId)
	{
		return await db.SelfCheckups
			.Where(s => s.UserId == userId && s.CourseId == courseId && s.SlideId == slideId)
			.ToListAsync();
	}

	public async Task AddOrUpdateSelfCheckup(string userId, string courseId, Guid slideId, string checkupId, bool isChecked)
	{
		var checkup = new SelfCheckup
		{
			UserId = userId,
			CourseId = courseId,
			SlideId = slideId,
			CheckupId = checkupId,
			IsChecked = isChecked
		};
		
		var executionStrategy = new NpgsqlRetryingExecutionStrategy(db, 3);
		await executionStrategy.ExecuteAsync(async () =>
		{
			using var ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromSeconds(30), TransactionScopeAsyncFlowOption.Enabled);
			db.AddOrUpdate(checkup, s => s.UserId == userId && s.CourseId == courseId && s.SlideId == slideId && s.CheckupId == checkupId);
			await db.SaveChangesAsync();
			ts.Complete();
		});
	}
}