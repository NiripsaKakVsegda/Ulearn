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
			var width = googleSheetModel.Cells.Max(r => r.Count);
			WhiteWashSheet(spreadsheetId, googleSheetModel.ListId, width);
			var requests = RequestCreator.GetRequests(googleSheetModel);
			service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest { Requests = requests },
				spreadsheetId).Execute();
		}

		private void WhiteWashSheet(string spreadsheetId, int listId, int width)
		{
			var spreadsheet = service.Spreadsheets.Get(spreadsheetId).Execute();
			var requests = new List<Request>
			{
				new()
				{
					DeleteDimension = new DeleteDimensionRequest
					{
						Range = new DimensionRange
						{
							Dimension = "COLUMNS",
							StartIndex = 0,
							EndIndex = spreadsheet.Sheets.FirstOrDefault(e => e.Properties.SheetId == listId)?.Properties.GridProperties.ColumnCount - 1,
							SheetId = listId
						}
					}
				},
				new()
				{
					InsertDimension = new InsertDimensionRequest
					{
						Range = new DimensionRange
						{
							Dimension = "COLUMNS",
							StartIndex = 0,
							EndIndex = width,
							SheetId = listId
						}
					}
				}
			};
			service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest { Requests = requests },
				spreadsheetId).Execute();
		}
	}
}