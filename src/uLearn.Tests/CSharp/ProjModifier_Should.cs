using System.IO;
using ApprovalTests;
using FluentAssertions;
using Microsoft.Build.Evaluation;
using NUnit.Framework;
using Ulearn.Core;
using Ulearn.Core.Helpers;

namespace uLearn.CSharp
{
	[TestFixture]
	public class ProjModifier_Should
	{
		private Project CreateTestProject() => new Project(TestProjectFilename, null, null, new ProjectCollection());
		private string TestProjectFilename => Path.Combine(TestsHelper.TestDirectory, "CSharp", "TestProject", "ProjDir", "test.csproj");

		[OneTimeSetUp]
		public void SetUp()
		{
			Approvals.RegisterDefaultNamerCreation(() => new RelativeUnitTestFrameworkNamer());
		}

		[Test]
		public void NotChangeFile_OnModify()
		{
			ProjModifier.ModifyCsproj(
				new FileInfo(TestProjectFilename),
				p => ProjModifier.PrepareForChecking(p, "AAA", new string[0]));
			Assert.AreNotEqual("AAA", CreateTestProject().GetProperty("StartupObject"));
		}

		[Test]
		public void ReplaceLinksWithItems()
		{
			MsBuildLocationHelper.InitPathToMsBuild();
			var project = CreateTestProject();
			var copies = ProjModifier.ReplaceLinksWithItemsAndReturnWhatToCopy(project);
			copies.Should().HaveCount(1);
			var writer = new StringWriter();
			foreach (var fileToCopy in copies)
			{
				writer.WriteLine("Copy " + fileToCopy.SourceFile + " to " + fileToCopy.DestinationFile);
			}

			project.Save(writer);
			Approvals.Verify(writer.ToString());
			project.Save(Path.Combine(project.DirectoryPath, "res.csproj"));
		}
	}
}