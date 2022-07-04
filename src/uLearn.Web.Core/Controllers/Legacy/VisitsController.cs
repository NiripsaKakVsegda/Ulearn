using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using Database.DataContexts;
using Database.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using uLearn.Web.FilterAttributes;

namespace uLearn.Web.Controllers;

[ULearnAuthorize]
public class VisitsController : Controller
{
	private readonly VisitsRepo visitsRepo = new VisitsRepo(new ULearnDb());

	[HttpPost]
	public async Task<ActionResult> Upload()
	{
		var visitsAsText = new StreamReader(Request.InputStream).ReadToEnd();
		var visitsDictionary = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(visitsAsText);
		var userId = User.Identity.GetUserId();
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