using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos;

public interface ISelfCheckupsRepo
{
	Task<List<SelfCheckup>> GetSelfCheckups(string userId, string courseId, Guid slideId);
	Task AddOrUpdateSelfCheckup(string userId, string courseId, Guid slideId, string checkupId, bool isChecked);
}