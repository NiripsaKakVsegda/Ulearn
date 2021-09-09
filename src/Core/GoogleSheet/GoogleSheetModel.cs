using System;
using System.Collections.Generic;

namespace Ulearn.Core.GoogleSheet
{
    public class GoogleSheetModel
    {
        public readonly List<List<IGoogleSheetCell>> Cells;
        public readonly int ListId;

		public GoogleSheetModel(int listId)
		{
			ListId = listId;
			Cells = new List<List<IGoogleSheetCell>> { new() };
		}

		public void GoToNewLine() => Cells.Add(new List<IGoogleSheetCell>());

		public void AddCell(int row, string value) => Cells[row].Add(new StringGoogleSheetCell(value));

		public void AddCell(int row, double value) => Cells[row].Add(new NumberGoogleSheetCell(value));

		public void AddCell(int row, DateTime value) => Cells[row].Add(new DateGoogleSheetCell(value));
	}
}