using System;

namespace Ulearn.Core.GoogleSheet
{
    public class GoogleSheetModel
    {
        public readonly IGoogleSheetCell[,] Cells;
        public readonly int ListId;
        public readonly int Height;
        public readonly int Width;

		public GoogleSheetModel(int height, int width, int listId)
		{
			Height = height;
			Width = width;
			ListId = listId;
			Cells = new IGoogleSheetCell[height, width];
			for (int i = 0; i < height; i++)
				for (int j = 0; j < width; j++)
					Cells[i, j] = new StringGoogleSheetCell("");
		}

		public void AddCell(int row, int column, string value) => Cells[row, column] = new StringGoogleSheetCell(value);

		public void AddCell(int row, int column, double value) => Cells[row, column] = new NumberGoogleSheetCell(value);

		public void AddCell(int row, int column, DateTime value) => Cells[row, column] = new DateGoogleSheetCell(value);
	}
}