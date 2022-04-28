using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace Ulearn.Core.GoogleSheet
{
	public class GoogleApiClient
	{
		private readonly SheetsService service;

		public GoogleApiClient(string credentials)
		{
			service = new SheetsService(new BaseClientService.Initializer
			{
				HttpClientInitializer = GoogleCredential.FromStream(new MemoryStream(Encoding.UTF8.GetBytes(credentials))).CreateScoped(SheetsService.Scope.Spreadsheets),
			});
		}

		public void FillSpreadSheet(string spreadsheetId, GoogleSheetModel googleSheetModel)
		{
			var (width, height) = UpdateAndGetNewSizes(spreadsheetId, googleSheetModel);

			var requests = RequestCreator.GetRequests(googleSheetModel);

			service.Spreadsheets
				.BatchUpdate(new BatchUpdateSpreadsheetRequest { Requests = requests }, spreadsheetId)
				.Execute();
		}

		private (int width, int height) UpdateAndGetNewSizes(string spreadsheetId, GoogleSheetModel googleSheetModel, bool removeUnused = false)
		{
			var spreadsheet = service.Spreadsheets.Get(spreadsheetId).Execute();
			var listId = googleSheetModel.ListId;
			var sheet = spreadsheet.Sheets.FirstOrDefault(e => e.Properties.SheetId == googleSheetModel.ListId);

			if (sheet == null) throw new ArgumentException($"No sheets in spreadsheet {spreadsheetId}");
			
			var width = googleSheetModel.Cells.Max(r => r.Count);
			var height = googleSheetModel.Cells.Count;
			var oldWidth = sheet.Properties.GridProperties.ColumnCount - 1 ?? width;
			var oldHeight = sheet.Properties.GridProperties.RowCount - 1 ?? height;

			var requests = new List<Request>();

			if (width - oldWidth > 0)
				requests.Add(CreateInsertDimensionRequest("COLUMNS", listId, oldWidth, width));
			if (removeUnused && width - oldWidth < 0)
				requests.Add(CreateDeleteDimensionRequest("COLUMNS", listId, width, oldWidth));

			if (height - oldHeight > 0)
				requests.Add(CreateInsertDimensionRequest("ROWS", listId, oldHeight, height));
			if (removeUnused && height - oldHeight < 0)
				requests.Add(CreateDeleteDimensionRequest("ROWS", listId, height, oldHeight));
			
			if (requests.Count > 0)
				service.Spreadsheets
					.BatchUpdate(new BatchUpdateSpreadsheetRequest { Requests = requests }, spreadsheetId)
					.Execute();

			return (
				removeUnused ? width : Math.Max(width, oldWidth),
				removeUnused ? height : Math.Max(height, oldHeight)
			);
		}

		Request CreateInsertDimensionRequest(string dimensionName, int sheetId, int startIndex, int endIndex) => new()
		{
			InsertDimension = new InsertDimensionRequest
			{
				Range = new DimensionRange
				{
					Dimension = dimensionName,
					StartIndex = startIndex,
					EndIndex = endIndex,
					SheetId = sheetId
				}
			}
		};

		private Request CreateDeleteDimensionRequest(string dimensionName, int sheetId, int startIndex, int endIndex) => new()
		{
			DeleteDimension = new DeleteDimensionRequest
			{
				Range = new DimensionRange
				{
					Dimension = dimensionName,
					StartIndex = startIndex,
					EndIndex = endIndex,
					SheetId = sheetId
				}
			}
		};
	}
}