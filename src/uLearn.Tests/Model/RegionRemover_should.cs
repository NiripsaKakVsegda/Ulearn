using System;
using System.Collections.Generic;
using System.IO;
using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using uLearn.Model.Blocks;

namespace uLearn.Model
{
	[TestFixture]
	public class RegionRemover_should
	{
		private static readonly DirectoryInfo dir = new DirectoryInfo("tests");

		private static string LoadCode(string file = "OverloadedMethods.cs")
		{
			return dir.GetFile(file).ContentAsUtf8().LineEndingsToUnixStyle();
		}

		private static string RemoveRegion(string region, bool onlyBody = false, string langId = "cs")
		{
			var code = LoadCode();
			IEnumerable<Label> notRemoved;
			code = new RegionRemover(langId).Remove(code, new[] { new Label { Name = region, OnlyBody = onlyBody } }, out notRemoved);
			Assert.IsEmpty(notRemoved);
			return code;
		}

		private static string RemoveRegions(string langId, params Label[] regions)
		{
			var code = LoadCode();
			IEnumerable<Label> notRemoved;
			code = new RegionRemover(langId).Remove(code, regions, out notRemoved);
			Assert.IsEmpty(notRemoved);
			return code;
		}


		[Test, UseReporter(typeof(DiffReporter))]
		public void RemoveMethods()
		{
			Approvals.Verify(RemoveRegion("Method"));
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public void RemoveRegion()
		{
			Approvals.Verify(RemoveRegion("methods_1_and_2"));
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public void RemoveMix()
		{
			Approvals.Verify(RemoveRegions("cs", new Label { Name = "methods_1_and_2" }, new Label { Name = "Main" }));
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public void RemoveMethodsBody()
		{
			Approvals.Verify(RemoveRegion("Method", true));
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public void RemovePriority()
		{
			Approvals.Verify(RemoveRegion("Region"));
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public void SingleMethodSolution()
		{
			var code = LoadCode();
			int index;
			code = new RegionRemover("cs").RemoveSolution(code, new Label { Name = "Main" }, out index);
			Approvals.Verify(String.Format("solution index: {0}\r\nCode:\r\n{1}", index, code));
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public void SingleMethodSolutionBody()
		{
			var code = LoadCode();
			int index;
			code = new RegionRemover("cs").RemoveSolution(code, new Label { Name = "Main", OnlyBody = true}, out index);
			Approvals.Verify(String.Format("solution index: {0}\r\nCode:\r\n{1}", index, code));
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public void SingleRegionSolution()
		{
			var code = LoadCode();
			int index;
			code = new RegionRemover("cs").RemoveSolution(code, new Label { Name = "methods_2_and_3" }, out index);
			Approvals.Verify(String.Format("solution index: {0}\r\nCode:\r\n{1}", index, code));
		}
	}
}