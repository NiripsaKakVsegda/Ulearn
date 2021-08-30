using System.IO;
using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using uLearn.Model.Blocks;

namespace uLearn.Model
{
	[TestFixture]
	public class RegionsExtractor_should
	{
		private static readonly DirectoryInfo dir = new DirectoryInfo("tests");

		private static RegionsExtractor GetExtractor(string file = "OverloadedMethods.cs", string langId = "cs")
		{
			var code = dir.GetFile(file).ContentAsUtf8();
			return new RegionsExtractor(code, langId);
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public static void ExtractRegion()
		{
			var extractor = GetExtractor();
			Approvals.Verify(extractor.GetRegion(new Label { Name = "methods_1_and_2" }));
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public static void ExtractMethod()
		{
			var extractor = GetExtractor();
			Approvals.Verify(extractor.GetRegion(new Label { Name = "Method" }));
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public static void ExtractMany()
		{
			var extractor = GetExtractor();
			Approvals.VerifyAll(extractor.GetRegions(new[]
			{
				new Label { Name = "Method" },
				new Label { Name = "Main", OnlyBody = true },
				new Label { Name = "methods_1_and_2" },
				new Label { Name = "methods_2_and_3" }
			}), "regions");
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public static void ExtractPriority()
		{
			var extractor = GetExtractor();
			Approvals.Verify(extractor.GetRegion(new Label { Name = "Region" }));
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public static void ExtractManyNoCs()
		{
			var extractor = GetExtractor(langId: "");
			Approvals.VerifyAll(extractor.GetRegions(new[]
			{
				new Label { Name = "Method" },
				new Label { Name = "Main", OnlyBody = true },
				new Label { Name = "methods_1_and_2" },
				new Label { Name = "methods_2_and_3" }
			}), "regions");
		}
	}
}