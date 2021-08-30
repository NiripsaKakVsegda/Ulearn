using System;

namespace Ulearn.Core.GoogleSheet
{
	public interface IGoogleSheetCell
	{
	}

	public class StringGoogleSheetCell : IGoogleSheetCell
	{
		public readonly string Value;

		public StringGoogleSheetCell(string value)
		{
			Value = value;
		}
	}

	public class DateGoogleSheetCell : IGoogleSheetCell
	{
		public readonly DateTime Value;

		public DateGoogleSheetCell(DateTime value)
		{
			Value = value;
		}
	}

	public class NumberGoogleSheetCell : IGoogleSheetCell
	{
		public readonly double Value;

		public NumberGoogleSheetCell(double value)
		{
			Value = value;
		}
	}
}