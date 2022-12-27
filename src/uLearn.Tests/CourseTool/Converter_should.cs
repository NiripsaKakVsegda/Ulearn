using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ulearn.Core;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.Courses.Units;
using Ulearn.Core.CSharp;
using Ulearn.Core.Model.Edx;
using Ulearn.Core.Model.Edx.EdxComponents;
using uLearn.CSharp;

namespace uLearn.CourseTool
{
	[TestFixture]
	public class Converter_should
	{
		private Course course;

		private Slide aTextSlide;
		private ExerciseSlide exerciseSlide;

		private const string courseId = "TestCourse";
		private const string youtubeIdFromCourse = "ihyw2FdX4xs";
		private static readonly string slideIdWithVideoFromCourse = "6b4382d1-ff21-4297-b276-fc873fc3579e";
		private static readonly Guid slideIdFromCourse = Guid.Parse(slideIdWithVideoFromCourse);
		private readonly DirectoryInfo testCourseDirectory = new DirectoryInfo($@"CourseTool/TestData/{courseId}");
		private const string ulearnBaseUrlWeb = "https://localhost:44300";
		private const string ulearnBaseUrlApi = "https://localhost:8000";
		private const string ltiId = "edx";
		private const string testFolderName = "test";

		[SetUp]
		public void SetUp()
		{
			TearDown();
			Directory.SetCurrentDirectory(TestsHelper.TestDirectory);
			if (!Directory.Exists(testFolderName))
				Directory.CreateDirectory(testFolderName);
			var loader = new CourseLoader(new UnitLoader(new XmlSlideLoader()));
			var fullCoursePath = testCourseDirectory.FullName;
			if (!testCourseDirectory.Exists)
				throw new Exception($"{fullCoursePath} not exists");
			course = loader.Load(testCourseDirectory, courseId);
			var unit = course.GetUnitsNotSafe()[0];
			aTextSlide = new Slide(new MarkdownBlock("hello"))
			{
				Id = Guid.NewGuid(),
				Title = "title",
				Unit = unit
			};
			exerciseSlide = new ExerciseSlide(new CsProjectExerciseBlock(), new SingleFileExerciseBlock())
			{
				Id = slideIdFromCourse,
				Title = "title",
				Unit = unit
			};
		}

		[TearDown]
		public void TearDown()
		{
			Utils.DeleteDirectoryIfExists(testFolderName);
		}

		private EdxCourse ConvertForTestsCourseToEdx()
		{
			var config = new Config
			{
				Organization = "org",
				LtiId = ""
			};
			return Converter.ToEdxCourse(course, config, ulearnBaseUrlWeb, ulearnBaseUrlApi, testCourseDirectory);
		}

		private HashSet<string> GetDirectoryFiles(string directory)
		{
			var files = Directory.GetFiles(directory).Select(Path.GetFileName);
			var dirFiles = Directory.GetDirectories(directory).SelectMany(x => Directory.GetFiles(x).Select(Path.GetFileName));
			var hs = new HashSet<string>(files.Concat(dirFiles));
			return hs;
		}

		[Test]
		public void convert_saveAndLoadEdxCourse()
		{
			var edxCourse = ConvertForTestsCourseToEdx();

			var f1 = $"{testFolderName}/{1}";
			var f2 = $"{testFolderName}/{2}";
			edxCourse.Save(f1);
			EdxCourse.Load(f1).Save(f2);

			CollectionAssert.AreEqual(GetDirectoryFiles(f1), GetDirectoryFiles(f2));
		}

		[Test]
		public void convert_assign_SlideIds_to_EdxUrlNames()
		{
			// var edxCourse = ConvertForTestsCourseToEdx();
			// var ulearnSlideIds = course.GetSlidesNotSafe().Select(x => x.NormalizedGuid);
			// var edxVerticals = edxCourse.CourseWithChapters.Chapters[0].Sequentials
			// 	.SelectMany(x => x.Verticals)
			// 	.ToList();
			// foreach (var vertical in edxVerticals)
			// 	Console.WriteLine(vertical.DisplayName);
			// CollectionAssert.IsSubsetOf(ulearnSlideIds, edxVerticals.Select(x => x.UrlName));
		}

		[Test]
		public void convert_assign_VideoIds_accordingToPassedDictionary()
		{
			var edxCourse = ConvertForTestsCourseToEdx();
			Assert.AreEqual(youtubeIdFromCourse, edxCourse.GetVideoBySlideId(slideIdWithVideoFromCourse).NormalSpeedVideoId);
		}

		[Test]
		public void patch_updates_YoutubeIds()
		{
			var ulearnVideoGuid = Utils.NewNormalizedGuid();
			var edxCourse = ConvertForTestsCourseToEdx();
			var olxPath = $"{testFolderName}/{course.Id}";
			edxCourse.Save(olxPath);

			const string youtubeId = "QWFuk3ymXxc";
			var videoComponents = new List<VideoComponent> { new VideoComponent(ulearnVideoGuid, "", youtubeId) };

			new OlxPatcher(olxPath).PatchComponents(edxCourse, videoComponents);

			Assert.That(File.ReadAllText($"{olxPath}/video/{ulearnVideoGuid}.xml").Contains(youtubeId));
		}

		[Test]
		public void patch_putsNewVideos_toExistingUnsortedChapter()
		{
			var videoGuid = Utils.NewNormalizedGuid();
			var videoGuid2 = Utils.NewNormalizedGuid();
			var edxCourse = ConvertForTestsCourseToEdx();
			var olxPath = $"{testFolderName}/{course.Id}";
			edxCourse.Save(olxPath);

			var patcher = new OlxPatcher(olxPath);

			var videoComponents = new List<VideoComponent>
			{
				new VideoComponent(videoGuid, "", "QWFuk3ymXxc"),
				new VideoComponent(videoGuid, "", "w8_GlqSkG-U"),
			};

			patcher.PatchComponents(edxCourse, videoComponents);

			videoComponents = new List<VideoComponent>
			{
				new VideoComponent(videoGuid, "", "QWFuk3ymXxc"),
				new VideoComponent(videoGuid2, "", "w8_GlqSkG-U"),
				new VideoComponent(Utils.NewNormalizedGuid(), "", "qTnKi67AAlg"),
			};

			patcher.PatchComponents(edxCourse, videoComponents);

			var edxCourse2 = EdxCourse.Load(olxPath);
			Assert.AreEqual("Unsorted", edxCourse2.CourseWithChapters.Chapters[1].DisplayName);
			Assert.AreEqual(2, edxCourse2.CourseWithChapters.Chapters[1].Sequentials.Length);
		}

		[Test]
		public void patch_doesNotCreateUnsortedChapter_ifNoNewSlides()
		{
			var edxCourse = ConvertForTestsCourseToEdx();
			var olxPath = $"{testFolderName}/{course.Id}";
			edxCourse.Save(olxPath);

			new OlxPatcher(olxPath).PatchVerticals(edxCourse, course.GetSlidesNotSafe()
				.Select(x => x.ToVerticals(
					course.Id,
					ulearnBaseUrlWeb,
					ulearnBaseUrlApi,
					ltiId,
					testCourseDirectory
				).ToArray()));

			var edxCourse2 = EdxCourse.Load(olxPath);
			Assert.IsFalse(edxCourse2.CourseWithChapters.Chapters.Any(c => c.DisplayName == "Unsorted"));
		}

		[Test]
		public void patch_createsUnsortedChapter_withNewSlides()
		{
			var edxCourse = ConvertForTestsCourseToEdx();
			var olxPath = $"{testFolderName}/{course.Id}";
			edxCourse.Save(olxPath);

			new OlxPatcher(olxPath).PatchVerticals(edxCourse, new[] { aTextSlide }
				.Select(x => x.ToVerticals(
					course.Id,
					ulearnBaseUrlWeb,
					ulearnBaseUrlApi,
					ltiId,
					testCourseDirectory
				).ToArray()));

			var edxCourse2 = EdxCourse.Load(olxPath);
			Assert.AreEqual("Unsorted", edxCourse2.CourseWithChapters.Chapters.Last().DisplayName);
		}

		[Test]
		public void patch_updatesOrdinarySlide_withExerciseSlide()
		{
			var edxCourse = ConvertForTestsCourseToEdx();
			var olxPath = $"{testFolderName}/{course.Id}";
			edxCourse.Save(olxPath);

			var slidesCount = edxCourse.CourseWithChapters.Chapters[0].Sequentials[0].Verticals.Length;

			new OlxPatcher(olxPath).PatchVerticals(edxCourse, new[] { exerciseSlide }
				.Select(x => x.ToVerticals(
					course.Id,
					ulearnBaseUrlWeb,
					ulearnBaseUrlApi,
					ltiId,
					testCourseDirectory
				).ToArray()));

			var edxCourse2 = EdxCourse.Load(olxPath);
			var patchedSlidesCount = edxCourse2.CourseWithChapters.Chapters[0].Sequentials[0].Verticals.Length;
			Assert.AreEqual(slidesCount, patchedSlidesCount);
		}
	}
}