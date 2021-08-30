using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;


namespace Ulearn.Web.Api.Models.Parameters.Analytics
{
	public class StatisticsParams
	{
		[FromQuery(Name = "courseId")]
		public string CourseId { get; set; }

		[FromQuery(Name = "periodStart")]
		public string PeriodStart { get; set; }
		
		[FromQuery(Name = "periodFinish")]
		public string PeriodFinish { get; set; }

		private static readonly string[] dateFormats = { "dd.MM.yyyy" };

		public DateTime PeriodStartDate
		{
			get
			{
				var defaultPeriodStart = GetDefaultPeriodStart();
				if (string.IsNullOrEmpty(PeriodStart))
					return defaultPeriodStart;
				if (!DateTime.TryParseExact(PeriodStart, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
					return defaultPeriodStart;
				return result;
			}
		}

		private static DateTime GetDefaultPeriodStart()
		{
			return new DateTime(DateTime.Now.Year - 4, 1, 1);
		}

		public DateTime PeriodFinishDate
		{
			get
			{
				var defaultPeriodFinish = DateTime.Now.Date;
				if (string.IsNullOrEmpty(PeriodFinish))
					return defaultPeriodFinish;
				if (!DateTime.TryParseExact(PeriodFinish, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
					return defaultPeriodFinish;
				return result;
			}
		}
	}
	
	
	public class CourseStatisticsParams : StatisticsParams
	{
		/* Course statistics can't be filtered by dates */
		[BindNever]
		public new DateTime PeriodStartDate => DateTime.MinValue;
		
		[BindNever]
		public new DateTime PeriodFinishDate => DateTime.MaxValue.Subtract(TimeSpan.FromDays(2));
		
		[FromRoute(Name = "fileNameWithNoExtension")]
		public string FileNameWithNoExtension { get; set; }
		
		[FromQuery(Name = "groupsIds")]
		public List<string> GroupsIds { get; set; }

		[FromQuery(Name = "spreadsheetId")]
		public string SpreadsheetId { get; set; } = "1dlMITgyLknv-cRd0SleTTGetiNsSfdXAprjKGHU-FNI";

		[FromQuery(Name = "listId")]
		public int ListId { get; set; } = 0;
	}
}