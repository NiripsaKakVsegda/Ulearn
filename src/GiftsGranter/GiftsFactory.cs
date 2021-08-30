using Newtonsoft.Json.Linq;

namespace GiftsGranter
{
	public class GiftsFactory
	{
		public static JObject CreateGift(int score, CourseSettings settings)
		{
			var title = score >= settings.masterScore ? settings.masterTitle : settings.passTitle;
			var subtitle = string.Format(
				settings.subtitle,
				Pluralize(score, "балл", "балла", "баллов"),
				Pluralize(settings.maxScore, "возможного", "возможных", "возможных"));
			return JObject.Parse(
				$@"{{
	""entry"": {{
		""$type"": ""gift"",
		""from"": {{
			""id"": ""1253"",
			""name"": ""Егоров Павел Владимирович"",
			""type"": 1
		}},
		""message"": ""{settings.message}"",
		""anonymously"": true,
		""giftImagePath"": ""{settings.giftImagePath}"",
		""title"": ""{title}"",
		""subtitle"": ""{subtitle}""
	}},
	""options"": {{
		""postAsOwner"": false
	}}
}}");
		}

		private static string Pluralize(int count, string балл, string балла, string баллов)
		{
			if (count % 10 == 0 || count % 10 >= 5 || count % 100 > 10 && count % 100 < 20)
				return count + " " + баллов;
			return count + " " + (count % 10 == 1 ? балл : балла);
		}
	}
}