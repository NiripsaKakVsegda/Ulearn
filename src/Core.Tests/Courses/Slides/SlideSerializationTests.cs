using System;
using System.IO;
using NUnit.Framework;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Courses.Slides.Flashcards;

namespace Ulearn.Core.Tests.Courses.Slides
{
	[TestFixture]
	public class SlideSerializationTests
	{
		private const string testDataDirectory = "Courses/Slides/TestData/";

		[SetUp]
		public void SetUp()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
		}

		private static Slide DeserializeSlideFromXmlFile(string filename)
		{
			var slideFile = new DirectoryInfo(testDataDirectory).GetFile(filename);
			return slideFile.DeserializeXml<Slide>();
		}

		private static FlashcardSlide DeserializeFlashcardSlideFromXmlFile(string filename)
		{
			var slideFile = new DirectoryInfo(testDataDirectory).GetFile(filename);
			return slideFile.DeserializeXml<FlashcardSlide>();
		}

		[Test]
		public void DeserializeEmptySlide()
		{
			var slide = DeserializeSlideFromXmlFile("SimpleSlideWithoutBlocks.xml");

			Assert.IsNull(slide.Meta);
		}

		[Test]
		public void DeserializeSimpleSlide()
		{
			var slide = DeserializeSlideFromXmlFile("SimpleSlideWithoutBlocks.xml");

			Assert.AreEqual(Guid.Parse("AAFE3455-736E-48B2-BB38-A25AF5CABF4D"), slide.Id);
			Assert.AreEqual("Simple slide", slide.Title);
		}

		[Test]
		public void SerializeEmptySlide()
		{
			new Slide().XmlSerialize().DeserializeXml<Slide>();
		}

		[Test]
		public void SerializeSlideWithoutBlocks()
		{
			var slide = new Slide
			{
				Id = Guid.NewGuid(),
				Title = "Simple slide"
			};
			var deserializedSlide = slide.XmlSerialize().DeserializeXml<Slide>();

			Assert.AreEqual(slide.Id, deserializedSlide.Id);
			Assert.AreEqual(slide.Title, deserializedSlide.Title);
		}

		[Test]
		public void SerializeAndDeserializeSlideMeta()
		{
			var slide = DeserializeSlideFromXmlFile("SimpleSlideWithoutBlocksWithMeta.xml");

			Assert.AreEqual("path/to/image.png", slide.Meta.Image);
			Assert.AreEqual("Slide keywords", slide.Meta.Keywords);
			Assert.AreEqual("Slide description", slide.Meta.Description);
		}

		[Test]
		public void DeserializeSlideWithMarkdownBlocks()
		{
			var slide = DeserializeSlideFromXmlFile("SimpleSlideWithMarkdownBlocks.xml");

			Assert.AreEqual(4, slide.Blocks.Length);
			foreach (var block in slide.Blocks)
				Assert.AreEqual(typeof(MarkdownBlock), block.GetType());
			Assert.IsTrue(slide.Blocks[0].Hide);
			Assert.IsFalse(slide.Blocks[1].Hide);
			Assert.AreEqual(3, ((MarkdownBlock)slide.Blocks[3]).InnerBlocks.Length);
			Assert.AreEqual("\r\nПривет, это маркдаун блок.", ((MarkdownBlock)slide.Blocks[0]).Markdown);
		}

		[Test]
		public void DeserializeFlashcardSlideWithMarkdownBlocks()
		{
			var slide = DeserializeFlashcardSlideFromXmlFile("SimpleFlashcardSlide.xml");
			var flashcard = slide.FlashcardsList[0];

			var questionBlock = flashcard.Question.Blocks[0];
			var answerBlock = flashcard.Answer.Blocks[0];
			foreach (var block in flashcard.Answer.Blocks)
				Assert.AreEqual(typeof(MarkdownBlock), block.GetType());
			foreach (var block in flashcard.Question.Blocks)
				Assert.AreEqual(typeof(MarkdownBlock), block.GetType());
			Assert.AreEqual("\r\nI am question markdown block!", questionBlock.TryGetText());
			Assert.AreEqual("\r\nI am answer markdown block!", answerBlock.TryGetText());
		}

		[Test]
		public void DeserializeFlashcardSlideWithDifferentBlocks()
		{
			var slide = DeserializeFlashcardSlideFromXmlFile("SimpleFlashcardsSlideWithDifferentBlocks.xml");
			var flashcard = slide.FlashcardsList[0];

			var questionBlocks = flashcard.Question.Blocks;
			var answerBlocks = flashcard.Answer.Blocks;
			Assert.AreEqual(typeof(MarkdownBlock), questionBlocks[0].GetType());
			Assert.AreEqual(typeof(TexBlock), questionBlocks[1].GetType());
			Assert.AreEqual(typeof(CodeBlock), questionBlocks[2].GetType());
		}

		[Test]
		public void DeserializeSlideWithDifferentBlocks()
		{
			var slide = DeserializeSlideFromXmlFile("SimpleSlideWithDifferentBlocks.xml");

			Assert.AreEqual(6, slide.Blocks.Length);
			Assert.AreEqual(typeof(MarkdownBlock), slide.Blocks[0].GetType());
			Assert.AreEqual(typeof(YoutubeBlock), slide.Blocks[1].GetType());
			Assert.AreEqual(typeof(CodeBlock), slide.Blocks[2].GetType());
			Assert.AreEqual(typeof(TexBlock), slide.Blocks[3].GetType());
			Assert.AreEqual(typeof(ImageGalleryBlock), slide.Blocks[4].GetType());
			Assert.AreEqual(typeof(SelfCheckupsBlock), slide.Blocks[5].GetType());
			Assert.AreEqual("123456", ((YoutubeBlock)slide.Blocks[1]).VideoId);
			Assert.AreEqual(Language.CSharp, ((CodeBlock)slide.Blocks[2]).Language);
			CollectionAssert.AreEqual(new string[2]
			{
				"x = 1",
				"a_i = \\left{(}\\frac{n(n+1)}{2}\\right{)}"
			}, ((TexBlock)slide.Blocks[3]).TexLines);
			var imageGalleryBlock = (ImageGalleryBlock)slide.Blocks[4];
			CollectionAssert.AreEqual(new[]
			{
				"https://api.ulearn.me/courses/courseId/files/unit/image1.png",
				"https://api.ulearn.me/courses/courseId/files/unit/image2.png"
			}, imageGalleryBlock.GetAbsoluteImageUrls("https://api.ulearn.me/", "courseId", "unit"));
			CollectionAssert.AreEqual(new[]
			{
				"Проверь себя и найди это",
				"Не забудь подписаться"
			}, (slide.Blocks[5] as SelfCheckupsBlock).Checkups.ToArray());
		}
	}
}