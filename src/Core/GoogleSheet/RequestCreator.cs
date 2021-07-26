using System;
using System.Collections.Generic;
using Google.Apis.Sheets.v4.Data;

namespace Ulearn.Core.GoogleSheet
{
	public static class RequestCreator
	{
		public static List<Request> GetRequests(GoogleSheetModel googleSheetModel)
		{
			var requests = new List<Request>();
			for (var i = 0; i < googleSheetModel.Height; i++)
			{
				var values = new List<CellData>();
				for (var j = 0; j < googleSheetModel.Width; j++)
					values.Add(CreateCellData(googleSheetModel.Cells[i, j]));
				requests.Add(CreateRowUpdateRequest(googleSheetModel.ListId, values, i));
			}
			return requests;
		}

		private static Request CreateRowUpdateRequest(int listId, IList<CellData> values, int rowIndex, string fields = "*")
		{
			return new()
			{
				UpdateCells = new UpdateCellsRequest
				{
					Start = new GridCoordinate
					{
						SheetId = listId,
						RowIndex = rowIndex,
					},
					Rows = new List<RowData> { new() { Values = values } },
					Fields = fields
				}
			};
		}

		private static CellData CreateCellData(IGoogleSheetCell googleSheetCell)
		{
			var data = new CellData
			{
				UserEnteredValue = new ExtendedValue(),
				UserEnteredFormat = new CellFormat(),
			};
			data = googleSheetCell switch
			{
				StringGoogleSheetCell stringCell => CreateCellDataForString(stringCell.Value, data),
				DateGoogleSheetCell dateCell => CreateCellDataForDate(dateCell.Value, data),
				NumberGoogleSheetCell numberCell => CreateCellDataForNumber(numberCell.Value, data),
				_ => data
			};
			return data;
		}

		private static CellData CreateCellDataForDate(DateTime value, CellData cellData)
		{
			cellData.UserEnteredValue.NumberValue = GetDateValue(value);
			cellData.UserEnteredFormat.NumberFormat =
				new NumberFormat { Pattern = "dd.MM.yyyy HH:mm:ss", Type = "DATE_TIME" };
			return cellData;
		}

		private static CellData CreateCellDataForNumber(double value, CellData cellData)
		{
			cellData.UserEnteredValue.NumberValue = value;
			cellData.UserEnteredFormat.NumberFormat = new NumberFormat { Pattern = "####0.0#", Type = "NUMBER" };
			return cellData;
		}

		private static CellData CreateCellDataForString(string value, CellData cellData)
		{
			cellData.UserEnteredValue.StringValue = value;
			return cellData;
		}
		
		private static double? GetDateValue(DateTime dateTimeInUtc)
		{
			const int secondInDay = 60 * 60 * 24;
			var dateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeInUtc, TimeZoneInfo.FindSystemTimeZoneById(
				"West Asia Standard Time")); // Время по Екатеринбургу
			var startTime = new DateTime(1899, 11, 30, 0, 0, 0, 0, DateTimeKind.Utc);
			var tsInterval = dateTime.Subtract(startTime);
			var time = Convert.ToDouble(tsInterval.TotalSeconds) / secondInDay;
			return time;
		}
	}
}