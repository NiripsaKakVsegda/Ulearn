using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Core.Courses.Slides.Quizzes.Blocks;
using Ulearn.Core.Courses.Units;
using Ulearn.Core.Extensions;
using Ulearn.Core.Model.Edx;
using Ulearn.Core.Model.Edx.EdxComponents;
using Component = Ulearn.Core.Model.Edx.EdxComponents.Component;

namespace Ulearn.Core.Courses.Slides
{
	[XmlRoot("slide", IsNullable = false, Namespace = "https://ulearn.me/schema/v2")]
	public class Slide : ISlide
	{
		[XmlAttribute("id")]
		public Guid Id { get; set; }

		[XmlIgnore]
		public string NormalizedGuid => Id.GetNormalizedGuid();

		[XmlAttribute("title")]
		public string Title { get; set; }

		[XmlElement("meta")]
		public SlideMetaDescription Meta { get; set; }

		[XmlAttribute("hide")]
		[DefaultValue(false)]
		public bool Hide { get; set; }

		[XmlElement("defaultIncludeCodeFile")]
		public string DefaultIncludeCodeFile { get; set; }

		/* If you add new block type, don't forget to:
		 * 1. Add it to AllowedBlockTypes of Slide, QuizSlide or ExerciseSlide
		 * 2. If it's a common block, add it inside of SpoilerBlock tags too
		 * 3. Add definition for new block type in BlockType.cs
		 */

		/* Common blocks */
		[XmlElement(typeof(YoutubeBlock))]
		[XmlElement("markdown", typeof(MarkdownBlock))]
		[XmlElement(typeof(CodeBlock))]
		[XmlElement(typeof(TexBlock))]
		[XmlElement(typeof(ImageGalleryBlock))]
		[XmlElement(typeof(IncludeCodeBlock))]
		[XmlElement(typeof(IncludeMarkdownBlock))]
		[XmlElement(typeof(IncludeImageGalleryBlock))]
		[XmlElement("html", typeof(HtmlBlock))]
		[XmlElement(typeof(SpoilerBlock))]
		/* Quiz blocks */
		[XmlElement(typeof(IsTrueBlock))]
		[XmlElement(typeof(ChoiceBlock))]
		[XmlElement(typeof(FillInBlock))]
		[XmlElement(typeof(OrderingBlock))]
		[XmlElement(typeof(MatchingBlock))]
		/* Exercise blocks */
		[XmlElement(typeof(CsProjectExerciseBlock))]
		[XmlElement(typeof(SingleFileExerciseBlock))]
		[XmlElement(typeof(UniversalExerciseBlock))]
		[XmlElement(typeof(PolygonExerciseBlock))]
		[XmlChoiceIdentifier(nameof(DefineBlockType))]
		public SlideBlock[] Blocks { get; set; }

		[XmlIgnore]
		public BlockType[] DefineBlockType;

		/* This property is extended by QuizSlide and ExerciseSlide */
		[XmlIgnore]
		protected virtual Type[] AllowedBlockTypes { get; } =
		{
			typeof(YoutubeBlock),
			typeof(MarkdownBlock),
			typeof(CodeBlock),
			typeof(TexBlock),
			typeof(ImageGalleryBlock),
			typeof(IncludeCodeBlock),
			typeof(IncludeMarkdownBlock),
			typeof(IncludeImageGalleryBlock),
			typeof(HtmlBlock),
			typeof(SpoilerBlock)
		};

		[XmlIgnore]
		public Unit Unit { get; set; }

		[XmlIgnore]
		public string SlideFilePathRelativeToCourse { get; set; }

		public virtual bool ShouldBeSolved => false;

		public virtual int MaxScore => 0;

		[XmlIgnore]
		public string LatinTitle => Title.ToLatin();

		[XmlIgnore]
		public string Url => LatinTitle + "_" + NormalizedGuid;

		[NotNull]
		[XmlIgnore]
		public virtual string ScoringGroup { get; protected set; } = "";

		public Slide()
		{
		}

		public Slide(params SlideBlock[] blocks)
		{
			Blocks = blocks;
		}

		/// <summary>
		/// Validate slide. We guarantee that Validate() will be called after BuildUp() 
		/// </summary>
		public virtual void Validate(SlideLoadingContext context)
		{
			var slideLoadingContext = new SlideBuildingContext(context, this);
			foreach (var block in Blocks)
				block.Validate(slideLoadingContext);
		}

		/// <summary>
		/// Building slide and blocks, fill properties, initialize some values. Any work we need to do before work with slide.
		/// </summary>
		public virtual void BuildUp(SlideLoadingContext context)
		{
			Unit = context.Unit;
			SlideFilePathRelativeToCourse = context.SlideFilePathRelativeToCourse;
			if (Blocks == null)
				Blocks = new SlideBlock[0];

			/* Validate block types. We should do it before building up blocks */
			CheckBlockTypes();

			/* ... and build blocks */
			var slideLoadingContext = new SlideBuildingContext(context, this);
			Blocks = Blocks.SelectMany(b => b.BuildUp(slideLoadingContext, ImmutableHashSet<string>.Empty)).ToArray();

			DefineBlockTypes();
		}

		public void DefineBlockTypes()
		{
			if (Blocks != null)
				DefineBlockType = Blocks.Select(BlockTypeHelpers.GetBlockType).ToArray();
		}

		public void CheckBlockTypes()
		{
			foreach (var block in Blocks)
			{
				if (!AllowedBlockTypes.Any(type => type.IsInstanceOfType(block)))
					throw new CourseLoadingException(
						$"Недопустимый тип блока в слайде {SlideFilePathRelativeToCourse}: <{block.GetType().GetXmlType()}>. " +
						$"В этом слайде разрешены только следующие блоки: {string.Join(", ", AllowedBlockTypes.Select(t => $"<{t.GetXmlType()}>"))}"
					);
			}
		}

		public AbstractQuestionBlock FindBlockById(string id)
		{
			return Blocks.OfType<AbstractQuestionBlock>().FirstOrDefault(block => block.Id == id);
		}

		public override string ToString()
		{
			return $"Title: {Title}, Id: {NormalizedGuid}, MaxScore: {MaxScore}";
		}

		public IEnumerable<SlideBlock[]> GetBlockRangesWithSameVisibility(bool startNewRangeFromExerciseBlock)
		{
			if (Blocks.Length == 0)
				yield break;
			var range = new List<SlideBlock> { Blocks[0] };
			foreach (var block in Blocks.Skip(1))
			{
				if (block.Hide != range.Last().Hide || (startNewRangeFromExerciseBlock && block is AbstractExerciseBlock))
				{
					yield return range.ToArray();
					range.Clear();
				}

				range.Add(block);
			}

			yield return range.ToArray();
		}

		#region ExportToEdx

		private static IEnumerable<Vertical> OrdinarySlideToVerticals(EdxComponentBuilderContext context, Dictionary<string, string> videoGuids, string ltiId)
		{
			var slide = context.Slide;
			var componentIndex = 0;
			var components = new List<Component>();
			while (componentIndex < slide.Blocks.Length)
			{
				var blocks = slide.Blocks.Skip(componentIndex).TakeWhile(x => !(x is YoutubeBlock) && !(x is AbstractExerciseBlock)).ToList();
				if (blocks.Count != 0)
				{
					var innerComponents = new List<Component>();
					foreach (var block in blocks)
					{
						if (!block.Hide)
						{
							try
							{
								var component = block.ToEdxComponent(context with { ComponentIndex = componentIndex });
								innerComponents.Add(component);
							}
							catch (NotImplementedException ex)
							{
								Console.WriteLine($"{block.GetType().Name} block NotSupportedException");
							}
						}

						componentIndex++;
					}

					if (innerComponents.Any())
					{
						var displayName = componentIndex == blocks.Count ? slide.Title : "";
						var header = displayName == "" ? "" : "<h2>" + displayName + "</h2>";
						var slideComponent = new HtmlComponent
						{
							DisplayName = displayName,
							UrlName = slide.NormalizedGuid + componentIndex,
							Filename = slide.NormalizedGuid + componentIndex,
							Source = header + string.Join("", innerComponents.Select(x => x.AsHtmlString())),
							Subcomponents = innerComponents.ToArray()
						};
						components.Add(slideComponent);
					}
				}

				if (componentIndex >= slide.Blocks.Length)
					break;

				var exerciseBlock = slide.Blocks[componentIndex] as AbstractExerciseBlock;
				var otherComponent = exerciseBlock != null
					? ((ExerciseSlide)slide).GetExerciseComponent(componentIndex == 0 ? slide.Title : "Упражнение", slide, componentIndex, string.Format(context.UlearnBaseUrlWeb + SlideUrlFormat, context.CourseId, slide.Id), ltiId)
					: ((YoutubeBlock)slide.Blocks[componentIndex]).GetVideoComponent(componentIndex == 0 ? slide.Title : "", slide, componentIndex, videoGuids);

				components.Add(otherComponent);
				componentIndex++;
			}

			var exBlock = slide.Blocks.OfType<AbstractExerciseBlock>().FirstOrDefault();
			if (exBlock == null)
				yield return new Vertical(slide.NormalizedGuid, slide.Title, components.ToArray());
			else
			{
				var exerciseSlide = (ExerciseSlide)slide;
				yield return new Vertical(slide.NormalizedGuid, slide.Title, components.ToArray(), EdxScoringGroupsHack.ToEdxName(exerciseSlide.ScoringGroup), exerciseSlide.MaxScore);
			}
		}

		private static IEnumerable<Vertical> QuizToVerticals(string courseId, QuizSlide slide, string slideUrl, string ltiId)
		{
			var ltiComponent =
				new LtiComponent(slide.Title, slide.NormalizedGuid + "-quiz", string.Format(slideUrl, courseId, slide.Id), ltiId, true, slide.MaxScore, false);
			yield return new Vertical(slide.NormalizedGuid, slide.Title, new Component[] { ltiComponent }, EdxScoringGroupsHack.ToEdxName(slide.ScoringGroup), slide.MaxScore);
		}

		protected const string SlideUrlFormat = "/Course/{0}/LtiSlide?slideId={1}";

		public IEnumerable<Vertical> ToVerticals(string courseId, string ulearnBaseUrlApi, string ulearnBaseUrlWeb, Dictionary<string, string> videoGuids, string ltiId, DirectoryInfo courseDirectory)
		{
			var slideUrl = ulearnBaseUrlWeb + SlideUrlFormat;
			try
			{
				if (this is QuizSlide quizSlide)
				{
					return QuizToVerticals(courseId, quizSlide, slideUrl, ltiId).ToList();
				}

				return OrdinarySlideToVerticals(new EdxComponentBuilderContext("", courseId, this, 0, ulearnBaseUrlApi, ulearnBaseUrlWeb, courseDirectory), videoGuids, ltiId).ToList();
			}
			catch (Exception e)
			{
				throw new Exception($"Slide {this}. {e.Message}", e);
			}
		}

		#endregion
	}

	public record EdxComponentBuilderContext(string DisplayName, string CourseId, Slide Slide, int ComponentIndex,
		string UlearnBaseUrlApi, string UlearnBaseUrlWeb, DirectoryInfo CourseDirectory);

	[JsonConverter(typeof(StringEnumConverter), true)]
	public enum SlideType
	{
		[XmlEnum("lesson")]
		Lesson,

		[XmlEnum("quiz")]
		Quiz,

		[XmlEnum("exercise")]
		Exercise,

		[XmlEnum("flashcards")]
		Flashcards
	}

	class EdxScoringGroupsHack
	{
		public static string ToEdxName(string scoringGroup)
		{
			return scoringGroup == "homework" ? "Практика" : "Упражнения";
		}
	}
}