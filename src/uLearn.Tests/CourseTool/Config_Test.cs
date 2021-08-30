using System.IO;
using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Ulearn.Common.Extensions;
using uLearn.CSharp;

namespace uLearn.CourseTool
{
	[TestFixture]
	public class Config_Test
	{
		[OneTimeSetUp]
		public void SetUp()
		{
			Approvals.RegisterDefaultNamerCreation(() => new RelativeUnitTestFrameworkNamer());
		}

		[Test]
		[UseReporter(typeof(DiffReporter))]
		public void ConfigTemplate_IsOk()
		{
			var path = Path.Combine(TestsHelper.TestDirectory, "templates", "config.xml");
			var config = new FileInfo(path).DeserializeXml<Config>();
			config.IgnoredUlearnSlides = new[] { "1", "2" };
			Approvals.VerifyXml(config.XmlSerialize());
		}
	}
}