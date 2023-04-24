using System;
using System.Collections.Generic;

namespace Ulearn.Web.Api.Utils.SuperGroup;

public class GoogleSheetFormatException : Exception
{
	public List<int> RawsIndexes { get; set; }
}