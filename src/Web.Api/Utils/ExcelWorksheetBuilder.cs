using System;
using System.Collections.Generic;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Ulearn.Web.Api.Utils
{
	public class ExcelWorksheetBuilder : ISheetBuilder
	{
		private readonly ExcelWorksheet worksheet;
		private int currentRow;
		private int currentColumn;
		private readonly List<Action<ExcelStyle>> styleRules = new();
		private bool isLastStyleRuleForOneCellOnly = false;

		public int ColumnsCount;

		public ExcelWorksheetBuilder(ExcelWorksheet worksheet)
		{
			this.worksheet = worksheet;
			currentRow = 1;
			currentColumn = 1;
			ColumnsCount = 1;
		}

		public void AddCell<T>(T value, int colspan = 1)
		{
			if (colspan < 1)
				return;

			var cell = worksheet.Cells[currentRow, currentColumn];
			cell.Value = value;

			var range = worksheet.Cells[currentRow, currentColumn, currentRow, currentColumn + colspan - 1];
			if (colspan > 1)
				range.Merge = true;

			foreach (var styleRule in styleRules)
				styleRule(range.Style);
			if (isLastStyleRuleForOneCellOnly)
				PopStyleRule();

			currentColumn += colspan;
			ColumnsCount = Math.Max(ColumnsCount, currentColumn);
		}

		public void GoToNewLine()
		{
			currentRow += 1;
			currentColumn = 1;
		}

		public void AddStyleRule(Action<ExcelStyle> styleFunction)
		{
			styleRules.Add(styleFunction);
			isLastStyleRuleForOneCellOnly = false;
		}

		public void PopStyleRule()
		{
			styleRules.RemoveAt(styleRules.Count - 1);
			isLastStyleRuleForOneCellOnly = false;
		}

		public void AddStyleRuleForOneCell(Action<ExcelStyle> styleFunction)
		{
			AddStyleRule(styleFunction);
			isLastStyleRuleForOneCellOnly = true;
		}
	}
}