using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Microsoft.EntityFrameworkCore;

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
		var checkup = await db.SelfCheckups
			.FirstOrDefaultAsync(s => s.UserId == userId && s.CourseId == courseId && s.SlideId == slideId && s.CheckupId == checkupId);

		if (checkup == null)
		{
			db.SelfCheckups.Add(new SelfCheckup
			{
				UserId = userId,
				CourseId = courseId,
				SlideId = slideId,
				CheckupId = checkupId,
				IsChecked = isChecked
			});
		}
		else
		{
			checkup.IsChecked = isChecked;
		}

		await db.SaveChangesAsync();
	}
}