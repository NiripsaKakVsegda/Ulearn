using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Stepik.Api
{
	[DataContract]
	public abstract class StepikApiObject
	{
		[DataMember(Name = "id")]
		public int? Id;
	}

	[DataContract]
	public class StepikApiAccessToken : StepikApiObject
	{
		[DataMember(Name = "token_type")]
		public string TokenType { get; set; }

		[DataMember(Name = "access_token")]
		public string AccessToken { get; set; }

		[DataMember(Name = "expires_in")]
		public int ExpiresIn { get; set; }

		[DataMember(Name = "refresh_token")]
		public string RefreshToken { get; set; }
	}

	/* https://stepik.org/api/stepics/1 */
	[DataContract]
	[StepikApiEndpoint("stepics")]
	public class StepikApiStepic : StepikApiObject
	{
		[DataMember(Name = "profile")]
		public int ProfileId { get; set; }

		[DataMember(Name = "user")]
		public int UserId { get; set; }
	}

	/* https://stepik.org/api/users/1 */
	[DataContract]
	[StepikApiEndpoint("users")]
	public class StepikApiUser : StepikApiObject
	{
		[DataMember(Name = "first_name")]
		public string FirstName;

		[DataMember(Name = "last_name")]
		public string LastName;

		[DataMember(Name = "full_name")]
		public string FullName;

		[DataMember(Name = "is_guest")]
		public bool IsGuest;
	}

	[DataContract]
	[StepikApiEndpoint("courses")]
	[StepikApiPut("course")]
	public class StepikApiCourse : StepikApiObject
	{
		public StepikApiCourse()
		{
			SectionsIds = new List<int>();
		}

		[DataMember(Name = "title")]
		public string Title;

		[DataMember(Name = "summary")]
		public string Summary;

		[DataMember(Name = "sections")]
		public List<int> SectionsIds;
	}

	[DataContract]
	[StepikApiEndpoint("sections")]
	[StepikApiPut("section")]
	public class StepikApiSection : StepikApiObject
	{
		public StepikApiSection()
		{
			UnitsIds = new List<int>();
		}

		[DataMember(Name = "title")]
		public string Title;

		[DataMember(Name = "description")]
		public string Description;

		[DataMember(Name = "units")]
		public List<int> UnitsIds;

		[DataMember(Name = "course")]
		public int CourseId;

		[DataMember(Name = "position")]
		public int Position;
	}

	[DataContract]
	[StepikApiEndpoint("units")]
	[StepikApiPut("unit")]
	public class StepikApiUnit : StepikApiObject
	{
		[DataMember(Name = "section")]
		public int SectionId;

		[DataMember(Name = "lesson")]
		public int LessonId;

		[DataMember(Name = "position")]
		public int Position;

		[DataMember(Name = "is_active")]
		public bool IsActive;

		[DataMember(Name = "has_progress")]
		public bool HasProgress;
	}

	[DataContract]
	[StepikApiEndpoint("lessons")]
	[StepikApiPut("lesson")]
	public class StepikApiLesson : StepikApiObject
	{
		public StepikApiLesson()
		{
			StepsIds = new List<int>();
		}

		[DataMember(Name = "steps")]
		public List<int> StepsIds;

		[DataMember(Name = "title")]
		public string Title;
	}

	[DataContract]
	[StepikApiEndpoint("steps")]
	public class StepikApiStep : StepikApiObject
	{
		[DataMember(Name = "lesson")]
		public int LessonId;

		[DataMember(Name = "block")]
		public StepikApiBlock Block;

		[DataMember(Name = "cost")]
		public int Cost;

		[DataMember(Name = "position")]
		public int Position;

		[DataMember(Name = "status")]
		public string Status;

		[DataMember(Name = "max_submissions_count")]
		public int MaxSubmissionsCount;

		[DataMember(Name = "solutions_unlocked_attempts")]
		public int SolutionsUnlockedAttempts;

		[DataMember(Name = "is_solutions_unlocked")]
		public bool IsSolutionsUnlocked;

		[DataMember(Name = "has_submissions_restrictions")]
		public bool HasSubmissionsRestrictions;
	}

	[DataContract]
	public class StepikApiBlock
	{
		[DataMember(Name = "name")]
		public string Name;

		[DataMember(Name = "text")]
		public string Text;

		[DataMember(Name = "video")]
		public StepikApiVideo Video;

		[DataMember(Name = "animation")]
		public string Animation;

		[DataMember(Name = "options")]
		public Dictionary<string, object> Options;

		[DataMember(Name = "source")]
		public StepikApiBlockSource Source;

		[IgnoreDataMember]
		public int Cost;
	}

	[DataContract]
	[StepikApiEndpoint("videos")]
	public class StepikApiVideo : StepikApiObject
	{
		[DataMember(Name = "filename")]
		public string Filename;

		[DataMember(Name = "status")]
		public string Status;

		[DataMember(Name = "upload_data")]
		public string UploadDate;

		[DataMember(Name = "duration")]
		public int Duration;

		[DataMember(Name = "thumbnail")]
		public string Thumbnail;

		[DataMember(Name = "urls")]
		public List<StepikApiVideoUrl> Urls;
	}

	[DataContract]
	public class StepikApiVideoUrl
	{
		[DataMember(Name = "quality")]
		public string Quality;

		[DataMember(Name = "url")]
		public string Url;
	}

	[DataContract]
	public class StepikApiBlockSource
	{
		/* For StepikApiExternalGraderBlockSource */
		[DataMember(Name = "is_text_enabled")]
		public bool IsTextEnabled;

		[DataMember(Name = "queue_name")]
		public string QueueName;

		[DataMember(Name = "grader_payload")]
		public StepikApiExternalGraderPayload GraderPayload;

		/* For StepikApiChoiceBlockSource */
		[DataMember(Name = "is_always_correct")]
		public bool IsAlwaysCorrect;

		[DataMember(Name = "is_html_enabled")]
		public bool IsHtmlEnabled;

		[DataMember(Name = "is_multiple_choice")]
		public bool IsMultipleChoice;

		[DataMember(Name = "is_options_feedback")]
		public bool IsOptionsFeedback;

		[DataMember(Name = "preserve_order")]
		public bool PreserveOrder;

		[DataMember(Name = "sample_size")]
		public int SampleSize;

		[DataMember(Name = "options")]
		public List<StepikApiChoiceOption> Options;
	}

	[DataContract]
	public class StepikApiExternalGraderBlockSource : StepikApiBlockSource
	{
		[DataMember(Name = "language")]
		public string Language;

		[DataMember(Name = "template")]
		public string Template;

		public StepikApiExternalGraderBlockSource()
		{
		}

		public StepikApiExternalGraderBlockSource(string courseId, Guid slideId, string queueName, string template, string language, bool isTextEnabled = true)
		{
			GraderPayload = new StepikApiExternalGraderPayload(courseId, slideId);
			QueueName = queueName;
			IsTextEnabled = isTextEnabled;
			Template = template;
			Language = language;
		}
	}

	[DataContract]
	public class StepikApiChoiceBlockSource : StepikApiBlockSource
	{
		public StepikApiChoiceBlockSource()
		{
		}

		public StepikApiChoiceBlockSource(IEnumerable<StepikApiChoiceOption> options)
			: this()
		{
			Options = options.ToList();
			SampleSize = Options.Count;

			IsAlwaysCorrect = false;
			IsHtmlEnabled = true;
			IsMultipleChoice = false;
			IsOptionsFeedback = false;
			PreserveOrder = false;
		}
	}

	[DataContract]
	public class StepikApiChoiceOption
	{
		public StepikApiChoiceOption()
		{
		}

		public StepikApiChoiceOption(string text, bool isCorrect, string feedback)
		{
			Text = text;
			IsCorrect = isCorrect;
			Feedback = feedback ?? "";
		}

		[DataMember(Name = "text")]
		public string Text;

		[DataMember(Name = "is_correct")]
		public bool IsCorrect;

		[DataMember(Name = "feedback", IsRequired = true)]
		public string Feedback;
	}

	[DataContract]
	public class StepikApiExternalGraderPayload
	{
		public StepikApiExternalGraderPayload()
		{
		}

		public StepikApiExternalGraderPayload(string courseId, Guid slideId)
		{
			CourseId = courseId;
			SlideId = slideId;
		}

		[DataMember(Name = "course_id")]
		public string CourseId;

		[DataMember(Name = "slide_id")]
		public Guid SlideId;
	}

	[DataContract]
	[StepikApiEndpoint("step-sources")]
	[StepikApiPut("stepSource")]
	public class StepikApiStepSource : StepikApiObject
	{
		public StepikApiStepSource()
		{
			Cost = 0;
			Status = "ready";
			MaxSubmissionsCount = 3;
			SolutionsUnlockedAttempts = 3;
			IsSolutionsUnlocked = false;
			HasSubmissionsRestrictions = false;
		}

		public StepikApiStepSource(StepikApiStep step)
		{
			Id = step.Id;
			LessonId = step.LessonId;
			Position = step.Position;
			Status = step.Status;
			Block = step.Block;
			MaxSubmissionsCount = step.MaxSubmissionsCount;
			SolutionsUnlockedAttempts = step.SolutionsUnlockedAttempts;
			IsSolutionsUnlocked = step.IsSolutionsUnlocked;
			HasSubmissionsRestrictions = step.HasSubmissionsRestrictions;
			Cost = step.Cost;
		}

		[DataMember(Name = "lesson")]
		public int LessonId;

		[DataMember(Name = "position")]
		public int Position;

		[DataMember(Name = "status")]
		public string Status;

		[DataMember(Name = "block")]
		public StepikApiBlock Block;

		[DataMember(Name = "max_submissions_count")]
		public int MaxSubmissionsCount;

		[DataMember(Name = "solutions_unlocked_attempts")]
		public int SolutionsUnlockedAttempts;

		[DataMember(Name = "is_solutions_unlocked")]
		public bool IsSolutionsUnlocked;

		[DataMember(Name = "has_submissions_restrictions")]
		public bool HasSubmissionsRestrictions;

		[DataMember(Name = "cost")]
		public int Cost;
	}

	public class StepikApiEndpointAttribute : Attribute
	{
		public string Endpoint { get; private set; }

		public StepikApiEndpointAttribute(string endpoint)
		{
			Endpoint = endpoint;
		}
	}

	/* For PUT or POST queries to /api/lessons/ we should send JSON-encoded data in following format:
	 * {
	 *    'key_name': {
	 *       ... (object) ...
	 *     }
	 * }
	 */
	public class StepikApiPutAttribute : Attribute
	{
		public string JsonKeyName { get; private set; }

		public StepikApiPutAttribute(string jsonKeyName)
		{
			JsonKeyName = jsonKeyName;
		}
	}

	public static class StepikApiObjectExtensions
	{
		public static string GetApiEndpoint(this StepikApiObject obj)
		{
			return obj.GetType().GetApiEndpoint();
		}

		public static string GetApiEndpoint(this Type type)
		{
			var attribute = type.GetCustomAttribute<StepikApiEndpointAttribute>();
			if (attribute == null)
				throw new StepikApiException($"{type.Name} has no attribute [StepikApiEndpoint()]");
			return attribute.Endpoint;
		}

		public static string GetApiPutJsonKeyName(this StepikApiObject obj)
		{
			return obj.GetType().GetApiPutJsonKeyName();
		}

		public static string GetApiPutJsonKeyName(this Type type)
		{
			var attribute = type.GetCustomAttribute<StepikApiPutAttribute>();
			if (attribute == null)
				throw new StepikApiException($"{type.Name} has no attribute [StepikApiPut()]");
			return attribute.JsonKeyName;
		}
	}
}