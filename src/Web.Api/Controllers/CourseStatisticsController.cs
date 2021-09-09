using System.Text;
using System.Threading.Tasks;
using Database;
using Database.Repos.Users;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Web.Api.Models.Parameters.Analytics;
using Ulearn.Web.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OfficeOpenXml;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models.Responses.Analytics;
using Web.Api.Configuration;


namespace Ulearn.Web.Api.Controllers
{
	public class CourseStatisticsController : BaseController
	{
		private readonly UlearnConfiguration configuration;
		private readonly StatisticModelUtils statisticModelUtils;
		

		public CourseStatisticsController(ICourseStorage courseStorage, UlearnDb db, IUsersRepo usersRepo,
			IOptions<WebApiConfiguration> options, StatisticModelUtils statisticModelUtils)
			: base(courseStorage, db, usersRepo)
		{
			this.statisticModelUtils = statisticModelUtils;
			configuration = options.Value;
		}
		
		[HttpGet("course-statistics/export/{fileNameWithNoExtension}.json")]
		[Authorize(Policy = "Instructors", AuthenticationSchemes = "Bearer,Identity.Application")]
		public async Task<ActionResult> ExportCourseStatisticsAsJson([FromQuery] CourseStatisticsParams param, [FromRoute] string fileNameWithNoExtension)
		{
			if (param.CourseId == null)
				return NotFound();

			var model = await statisticModelUtils.GetCourseStatisticsModel(3000, UserId, param.CourseId, param.GroupsIds);
			var serializedModel = new CourseStatisticsModel(model).JsonSerialize(Formatting.Indented);

			return File(Encoding.UTF8.GetBytes(serializedModel), "application/json", $"{fileNameWithNoExtension}.json");
		}
		
		[HttpGet("course-statistics/export/{fileNameWithNoExtension}.xml")]
		[Authorize(Policy = "Instructors", AuthenticationSchemes = "Bearer,Identity.Application")]
		public async Task<ActionResult> ExportCourseStatisticsAsXml([FromQuery] CourseStatisticsParams param, [FromRoute] string fileNameWithNoExtension)
		{
			if (param.CourseId == null)
				return NotFound();
			var model = await statisticModelUtils.GetCourseStatisticsModel(3000, UserId, param.CourseId, param.GroupsIds);
			var serializedModel = new CourseStatisticsModel(model).XmlSerialize();
			return File(Encoding.UTF8.GetBytes(serializedModel), "text/xml", $"{fileNameWithNoExtension}.xml");
		}

		[HttpGet("course-statistics/export/{fileNameWithNoExtension}.xlsx")]
		[Authorize(Policy = "Instructors", AuthenticationSchemes = "Bearer,Identity.Application")]
		public async Task<ActionResult> ExportCourseStatisticsAsXlsx([FromQuery] CourseStatisticsParams param, [FromRoute] string fileNameWithNoExtension)
		{
			if (param.CourseId == null)
				return NotFound();

			var model = await statisticModelUtils.GetCourseStatisticsModel(3000, UserId, param.CourseId, param.GroupsIds);
			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
			var package = new ExcelPackage();
			var builder = new ExcelWorksheetBuilder(package.Workbook.Worksheets.Add(model.CourseTitle));
			statisticModelUtils.FillStatisticModelBuilder(
				builder,
				model,
				exportEmails: true
			);
			builder = new ExcelWorksheetBuilder(package.Workbook.Worksheets.Add("Только полные баллы"));
			statisticModelUtils.FillStatisticModelBuilder(
				builder,
				model,
				exportEmails: true,
				onlyFullScores: true
			);
			byte[] bytes;
			using (var stream = StaticRecyclableMemoryStreamManager.Manager.GetStream()) {
				await package.SaveAsAsync(stream);
				bytes = stream.ToArray();
			}
			
			return File(bytes, "application/vnd.ms-excel", $"{fileNameWithNoExtension}.xlsx");
		}
	}
}