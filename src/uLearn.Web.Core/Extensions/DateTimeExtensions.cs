using Ulearn.Common.Extensions;

namespace uLearn.Web.Core.Extensions;

public static class DateTimeExtensions
{
	private const int secondsInHour = 60 * 60;
	private const int secondsInDay = 24 * secondsInHour;

	public static string ToPrettyString(this DateTime dateTime, bool withoutYearIfItsCurrent = false)
	{
		var utcTime = dateTime.ToUniversalTime();
		return $"{utcTime.ToDatePrettyString(withoutYearIfItsCurrent)} в {utcTime.ToShortTimeString()} (UTC)";
	}

	public static string ToDatePrettyString(this DateTime dateTime, bool withoutYearIfItsCurrent = false)
	{
		if (DateTime.Now.Year == dateTime.Year && withoutYearIfItsCurrent)
			return dateTime.ToString("d MMMM");
		return dateTime.ToLongDateString();
	}

	public static string ToAgoPrettyString(this DateTime from, bool showTimeForFarDate = false)
	{
		var diff = DateTime.Now.Subtract(from);
		if (diff.TotalDays > 10)
			return showTimeForFarDate ? from.ToPrettyString(true) : from.ToDatePrettyString(true);
		return diff.ToPrettyString();
	}

	public static string ToPrettyString(this TimeSpan timeSpan)
	{
		var daysAgo = (int)timeSpan.TotalDays;
		var secondsAgo = (int)timeSpan.TotalSeconds;

		switch (daysAgo)
		{
			case < 0:
				return null;
			case 0 when secondsAgo < 60:
				return "только что";
			case 0 when secondsAgo < secondsInHour:
			{
				var minutesAgo = secondsAgo / 60;
				return $"{minutesAgo.PluralizeInRussian(RussianPluralizationOptions.MinuteDative)} назад";
			}
			case 0 when secondsAgo < secondsInDay:
			{
				var hoursAgo = secondsAgo / secondsInHour;
				return $"{hoursAgo.PluralizeInRussian(RussianPluralizationOptions.Hour)} назад";
			}
			case 1:
				return "вчера";
			default:
				return $"{daysAgo.PluralizeInRussian(RussianPluralizationOptions.Day)} назад";
		}
	}

	public static DateTime MaxWith(this DateTime first, DateTime second)
	{
		return first > second ? first : second;
	}

	public static DateTime MinWith(this DateTime first, DateTime second)
	{
		return first < second ? first : second;
	}

	public static DateTime? MaxWith(this DateTime? first, DateTime? second)
	{
		if (!first.HasValue && !second.HasValue)
			return null;

		first ??= DateTime.MinValue;
		second ??= DateTime.MinValue;
		return first.Value.MaxWith(second.Value);
	}

	public static DateTime? MinWith(this DateTime? first, DateTime? second)
	{
		if (!first.HasValue && !second.HasValue)
			return null;

		first ??= DateTime.MinValue;
		second ??= DateTime.MinValue;
		return first.Value.MinWith(second.Value);
	}
}