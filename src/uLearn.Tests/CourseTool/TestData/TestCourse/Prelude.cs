// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;

public class SlideTestBase { }
public class Assert {
	public static void AreEqual(object expected, object actual)
	{
		if (ReferenceEquals(expected, actual))
			return;
		if (!Equals(expected, actual))
			throw new Exception(string.Format("Expected '{0}' but was '{1}'", expected, actual));
	}
}