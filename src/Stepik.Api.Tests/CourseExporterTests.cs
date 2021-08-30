using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Ulearn.Core.Courses;

namespace Stepik.Api.Tests
{
	[TestFixture]
	[Explicit("Be careful. This test deletes old content and fully rewrites your course on Stepik")]
	public class CourseExporterTests : StepikApiTests
	{
		private const int stepikCourseId = 2599;
		private const string stepikXQueueName = "urfu_cs1_testing";

		private CourseExporter courseExporter;

		[SetUp]
		public async Task SetUp()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);

			await InitializeClient();

			courseExporter = new CourseExporter(null, null, "", "", client.AccessToken);
		}

		[Test]
		[TestCase(@"..\..\..\..\..\courses\BasicProgramming\OOP\OOP\Slides\", "OOP")]
		public async Task TestExportCourseFromDirectory(string coursePath, string courseId)
		{
			var courseLoader = new CourseLoader();
			var stubCourse = courseLoader.Load(new DirectoryInfo(coursePath), courseId);
			await courseExporter.InitialExportCourse(stubCourse, new CourseInitialExportOptions(stepikCourseId, stepikXQueueName, new List<Guid>()));
		}
	}
}