using System.IO;
using NUnit.Framework;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Units;

namespace Ulearn.Core.Tests.Courses
{
	[TestFixture]
	public class CourseLoaderTests
	{
		private const string testDataDirectory = "Courses/TestData/";

		private CourseLoader loader;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			loader = new CourseLoader(new UnitLoader(new XmlSlideLoader()));
		}

		[SetUp]
		public void SetUp()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
		}

		private Course LoadCourseFromDirectory(string directory, string courseId)
		{
			var courseDirectory = new DirectoryInfo(testDataDirectory).GetSubdirectory(directory);
			return loader.Load(courseDirectory, courseId);
		}

		[Test]
		public void LoadSimpleCourse()
		{
			const string courseId = "SimpleCourse";
			const string courseDirectory = courseId;
			var course = LoadCourseFromDirectory(courseDirectory, courseId);

			Assert.AreEqual(2, course.GetUnitsNotSafe().Count);
			Assert.AreEqual(Language.CSharp, course.Settings.DefaultLanguage);
			Assert.AreEqual("Simple Course", course.Title);
			CollectionAssert.AreEqual(new[] { new PreludeFile(Language.Html, "Prelude.html"), }, course.Settings.Preludes);
		}

		[Test]
		[Explicit("Для проверки загрузки конкретного курса")]
		[TestCase(@"..\..\..\..\..\..\Courses\Courses\Testing", "Testing")]
		public void LoadCourseFromPath(string path, string courseId)
		{
			LoadCourseFromDirectory(path, courseId);
		}
	}
}