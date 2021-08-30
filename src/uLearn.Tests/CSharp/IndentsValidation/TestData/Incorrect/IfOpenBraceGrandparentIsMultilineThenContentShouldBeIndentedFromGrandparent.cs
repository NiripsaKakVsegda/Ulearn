using System;
using System.Collections.Generic;

public class IfOpenBraceGrandparentIsMultilineThenContentShouldBeIndentedFromGrandparent
{

	public IfOpenBraceGrandparentIsMultilineThenContentShouldBeIndentedFromGrandparent(
		int a, 
		int b) {
	var a = 0;
	}

	public static void Main(
		string s1, string s2, 
		string s3) {

		try {
		}
		catch (
			Exception e) {
		throw;
		}

		while (
			true || 
			true) {
	var a = 0;
		}

		if (false && false &&
			true && false) {
Console.WriteLine();
		}

		for (
			var i = 0; 
			i < 10; 
			i++) {
var a = 0;
		}

		foreach (var i in 
			new[] {1,2,3}) {
		var a = 0;
		}

		(a, 
			b) => {
var c = 0;
		};

		new A(a, b,
			c, d) {
e = 42
		};

		new List<
			List<int>> {
	new List<int> {1},
		};

		new int[
			10] {
1
		};

		// exception: ComplexInitializerExpression contents are indented by parent
		var d = new Dictionary<int, int>() {
			{
			1, 2
			}
		};
		var e = new Dictionary<int, int>() {
			{
		1, 2
			}
		};
	}
}