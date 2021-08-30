using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.Courses.Units;
using uLearn.CSharp;

namespace uLearn
{
	[TestFixture]
	public class Lesson_ShouldDeserialize
	{
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			Directory.SetCurrentDirectory(TestsHelper.TestDirectory);
		}

		[Test]
		public void SimpleMdBlock()
		{
			var blocks = DeserializeBlocks("<markdown>asd</markdown>");
			blocks.Should().BeEquivalentTo(new MarkdownBlock("asd"));
		}

		[Test]
		public void SeveralMdBlocks()
		{
			var blocks = DeserializeBlocks("<markdown>1</markdown><markdown>2</markdown>");
			blocks.Should().BeEquivalentTo(new MarkdownBlock("1"), new MarkdownBlock("2"));
		}

		[Test]
		public void MixedContentInsideMd()
		{
			var blocks = DeserializeBlocks(@"<markdown>1<code language=""csharp"">2</code>3</markdown>");
			blocks.Should().BeEquivalentTo(new MarkdownBlock("1"), new CodeBlock("2", Language.CSharp), new MarkdownBlock("3"));
		}

		[Test]
		public void OtherTypesOfBlocksAreOK()
		{
			var blocks = DeserializeBlocks(@"<markdown>1</markdown><code language=""javascript"">2</code><youtube>sdfsdfsdf</youtube>");
			blocks.Should().BeEquivalentTo(new MarkdownBlock("1"), new CodeBlock("2", Language.JavaScript), new YoutubeBlock("sdfsdfsdf"));
		}

		[Test]
		public void InstructorNotesInsideMd()
		{
			var blocks = DeserializeBlocks(@"<markdown>1<note>secret</note>2<code language=""javascript"">3</code>4</markdown>");
			blocks.Should().BeEquivalentTo(new MarkdownBlock("1"), new MarkdownBlock("secret") { Hide = true }, new MarkdownBlock("2"), new CodeBlock("3", Language.JavaScript), new MarkdownBlock("4"));
		}

		[Test]
		public void UserCodeFilePath()
		{
			var ex = DeserializeLesson("<exercise.csproj>" +
										"<userCodeFile>user-code-name-111</userCodeFile>" +
										"</exercise.csproj>").Blocks.Single();
			((CsProjectExerciseBlock)ex).UserCodeFilePath.Should().Be("user-code-name-111");
		}

		[Test]
		public void ExerciseHints()
		{
			var ex = DeserializeLesson("<exercise.csproj>" +
										"<hint>Hint 1</hint>" +
										"<hint>Hint 2</hint>" +
										"</exercise.csproj>").Blocks.Single() as CsProjectExerciseBlock;
			if (ex == null)
				Assert.Fail("Can't parse exercise with hints");
			ex.Hints.Should().HaveCount(2);
			ex.Hints.Should().Contain(new List<string> { "Hint 1", "Hint 2" });
			ex.HintsMd.Should().HaveSameCount(ex.Hints);
		}

		private static Slide DeserializeLesson(string blocksXml)
		{
			var input = $@"
<slide xmlns='https://ulearn.me/schema/v2'>
{blocksXml}
</slide>";
			File.WriteAllText("temp.xml", input);
			var fileInfo = new FileInfo("temp.xml");
			return fileInfo.DeserializeXml<Slide>();
		}

		private static SlideBlock[] DeserializeBlocks(string blocksXml)
		{
			var buildUpContext = new SlideBuildingContext("Test", new Unit(null, "."), CourseSettings.DefaultSettings, new DirectoryInfo("."), new DirectoryInfo("Unit1"), null);
			var blocks = DeserializeLesson(blocksXml).Blocks;
			return blocks.SelectMany(b => b.BuildUp(buildUpContext, ImmutableHashSet<string>.Empty)).ToArray();
		}
	}
}