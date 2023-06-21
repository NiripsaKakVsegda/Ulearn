using System.IO;
using System.Linq;
using CourseToolHotReloader.Dtos;
using CourseToolHotReloader.UpdateWorkers;
using FluentAssertions;
using NUnit.Framework;

namespace CourseToolHotReloader.Tests
{
	[TestFixture]
	public class CourseUpdateQueryTest
	{
		private CourseUpdateQuery courseUpdateQuery;

		[SetUp]
		public void Setup()
		{
			courseUpdateQuery = new CourseUpdateQuery();
		}

		[Test]
		public void CreateTest()
		{
			var a = CreateSimpleCourseUpdate("a");

			courseUpdateQuery.RegisterCreate(a);

			courseUpdateQuery.GetAllDeletedFiles().Should().BeEmpty();
			courseUpdateQuery.GetAllCourseUpdate().Count.Should().Be(1);
		}
		
		[Test]
		public void CreateDeleteCreateTest()
		{
			var a = CreateSimpleCourseUpdate("a");

			courseUpdateQuery.RegisterCreate(a);
			courseUpdateQuery.RegisterDelete(a);
			courseUpdateQuery.RegisterCreate(a);
			courseUpdateQuery.RegisterDelete(a);
			courseUpdateQuery.RegisterCreate(a);

			courseUpdateQuery.GetAllDeletedFiles().Should().BeEmpty();
			courseUpdateQuery.GetAllCourseUpdate().Count.Should().Be(1);
		}

		[Test]
		public void CreateModifyDeleteTest()
		{
			var a = CreateSimpleCourseUpdate("a");

			courseUpdateQuery.RegisterCreate(a);
			courseUpdateQuery.RegisterUpdate(a);
			courseUpdateQuery.RegisterDelete(a);

			courseUpdateQuery.GetAllDeletedFiles().Should().BeEmpty();
			courseUpdateQuery.GetAllCourseUpdate().Should().BeEmpty();
		}

		[Test]
		public void SimpleTest()
		{
			var a = CreateSimpleCourseUpdate("a");
			var b = CreateSimpleCourseUpdate("b");
			var c = CreateSimpleCourseUpdate("c");

			courseUpdateQuery.RegisterCreate(a);
			courseUpdateQuery.RegisterUpdate(a);
			courseUpdateQuery.RegisterUpdate(b);
			courseUpdateQuery.RegisterUpdate(c);
			courseUpdateQuery.RegisterDelete(c);

			Path.GetFileName(courseUpdateQuery.GetAllDeletedFiles().Single().FullPath).Should().Be("c");
			courseUpdateQuery.GetAllCourseUpdate().Count.Should().Be(2);
		}

		private static CourseUpdate CreateSimpleCourseUpdate(string name)
		{
			return new CourseUpdate(name);
		}
	}
}