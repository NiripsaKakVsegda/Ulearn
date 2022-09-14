using Database.Models;
using Database.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Ulearn.Common.Extensions;
using uLearn.Web.Core.Authorization;

namespace uLearn.Web.Core.Controllers;

[Authorize(Policy = UlearnAuthorizationBuilder.StudentsPolicyName)]
public class VisitsController : Controller
{
	private readonly IVisitsRepo visitsRepo;

	public VisitsController(IVisitsRepo visitsRepo)
	{
		this.visitsRepo = visitsRepo;
	}

	[HttpPost]
	public async Task<ActionResult> Upload()
	{
		var visitsAsText = await new StreamReader(Request.Body).ReadToEndAsync();
		var visitsDictionary = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(visitsAsText);
		var userId = User.GetUserId();
		var visits = new List<Visit>();
		foreach (var visit in visitsDictionary)
		{
			/* visit.Key is "<courseId> <slideId>" */
			var splittedVisit = visit.Key.Split(' ');
			var courseId = splittedVisit[0];
			var slideId = Guid.Parse(splittedVisit.Length > 1 ? splittedVisit[1] : splittedVisit[0]);
			visits.Add(new Visit
			{
				UserId = userId,
				CourseId = courseId,
				SlideId = slideId,
				Timestamp = visit.Value
			});
		}

		await visitsRepo.AddVisits(visits);
		return null;
	}
}