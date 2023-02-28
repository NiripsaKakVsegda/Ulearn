using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using HtmlHelper = Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelper;
using SelectListItem = Microsoft.AspNetCore.Mvc.Rendering.SelectListItem;

namespace uLearn.Web.Core.Extensions;

public static class HtmlExtensions
{
	// See https://en.wikipedia.org/wiki/List_of_Internet_top-level_domains for TLD list, we selected some of them
	private const string tldWhiteList = @"arpa|adult|aero|audio|best|blog|camera|camp|center|club|dance|data|education|email|game|global|hosting|jobs|live|map|mobi|photo|pizza|sale|shop|travel|video|wiki";

	private const string urlRegexS = @"(
											(
											 (?<protocol>(https?|ftps?)://|mailto:)
											 (?<hostnameOrIpAddress>
											  (([-a-z0-9_]*\.)+([a-z]*))
											  |
											  ((?:(?:25[0-5]|2[0-4]\d|[01]\d\d|\d?\d)(?(\.?\d)\.)){4})
											 )
											 |
											 (?<usernameAndPassword>
											  [-a-z0-9_.+]+
											  (:[-a-z0-9_.]+)?
											  @
											 )?
											 (?<hostnameOrIpAddress>
											  (([-a-z0-9_]{2,}\.)+([a-z]{2,3}(?<!txt|cs|min|max|add|io|pow)|" + tldWhiteList + @"))
											  |
											  ((?:(?:25[0-5]|2[0-4]\d|[01]\d\d|\d?\d)(?(\.?\d)\.)){4})
											 )
                                            )
											(?<port>:\d+)?
											(?<relativePath>
											 [/?]
											 (
											  [-a-z0-9._?,;'/\+&%$#=~!@]*
											  (?<pathInPairedBrackets>
											   \(
												[-a-z0-9._?,;'/\+&%$#=~!@]*
											   \)
											  )?
											  [-a-z0-9._?,;'/\+&%$#=~!@]*
											  # not all chars can be last in url
											  (?<=[-a-z0-9_'/\+&%$#=~@)]|&[a-z]+;)
											 )?
											)?
											# not all chars can be next after url
											(?=$|\n|[^a-zA-Z0-9])
										   )
										   ";

	public static readonly Regex urlRegex = new(urlRegexS, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

	public static string EncodeMultiLineText(this IHtmlHelper helper, string text, bool keepFirstSpaces = false)
	{
		if (string.IsNullOrEmpty(text))
			return string.Empty;

		text = helper.Encode(text);

		if (keepFirstSpaces)
			text = Regex.Replace(text, @"^\s+", m => string.Concat(Enumerable.Repeat("&nbsp;", m.Value.Length)), RegexOptions.Multiline);

		return text.Replace("\n", "<br/>").Replace("\r", "");
	}

	private static bool IsEmailUrl(string url)
	{
		return url.Contains("@") && !url.Contains("/");
	}

	public static string HighlightLink(this IHtmlHelper helper, string url)
	{
		var fullUrl = url;
		if (!url.Contains("://") && !url.StartsWith("mailto:"))
		{
			if (IsEmailUrl(url))
				fullUrl = "mailto:" + url;
			else
				fullUrl = "http://" + url;
		}

		return $"<a href=\"{helper.Encode(HttpUtility.HtmlDecode(fullUrl))}\">{url}</a>"; // helper.AttributeEncode
	}

	/// <summary>
	/// Find urls in html-encoded text and replace it with links
	/// </summary>
	/// <param name="helper"></param>
	/// <param name="htmlEncodedText">HTML-encoded text</param>
	/// <returns>HTML string with links</returns>
	public static string HighlightLinks(this IHtmlHelper helper, string htmlEncodedText)
	{
		return urlRegex.Replace(htmlEncodedText, match => helper.HighlightLink(match.Groups[0].Value));
	}

	/// <summary>
	/// Creates `DropDownList` with item's data-attributes. Options are the same as for `SelectExtensions.DropdownList()`,
	/// but `selectList` is a list of `SelectListItemWithAttributes`.
	/// </summary>
	public static HtmlString DropDownListWithItemAttributes(
		this IHtmlHelper htmlHelper,
		string name,
		IEnumerable<SelectListItemWithAttributes> selectList,
		IDictionary<string, object> htmlAttributes
	)
	{
		selectList = selectList.ToList();
		var selectItems = selectList.ToDictionary(item => item.Value, item => item);
		var content = htmlHelper.DropDownList(name, selectList, htmlAttributes).ToHtmlString();
		if (content == null || content == "")
			return null;
		var selectDoc = XDocument.Parse(content);

		var selectElement = selectDoc.Element("select");
		if (selectElement != null)
		{
			var options = selectElement.XPathSelectElements("//option").ToList();

			foreach (var option in options)
			{
				var optionValue = option.Attribute("value");
				if (optionValue != null && selectItems.ContainsKey(optionValue.Value))
				{
					var listItem = selectItems[optionValue.Value];
					foreach (var attribute in HtmlHelper.AnonymousObjectToHtmlAttributes(listItem.HtmlAttributes))
						option.SetAttributeValue(attribute.Key, attribute.Value);
				}
			}
		}

		return new HtmlString(selectDoc.ToString());
	}

	public static HtmlString DropDownListWithItemAttributes(this IHtmlHelper htmlHelper, string name, IEnumerable<SelectListItemWithAttributes> selectList, object htmlAttributes)
	{
		return DropDownListWithItemAttributes(htmlHelper, name, selectList, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
	}

	/* This method shows ValidationSummary() iff at least one error exists. Standard ValidationSummary() sometimes shows empty <div>.
	See https://stackoverflow.com/a/12111297 for details. */
	public static IHtmlContent? UlearnValidationSummary(this IHtmlHelper htmlHelper, bool excludePropertyErrors)
	{
		var htmlString = htmlHelper.ValidationSummary(excludePropertyErrors);

		if (htmlString == null)
			return null;

		var content = htmlString.ToHtmlString();
		if (content == null || content == "")
			return null;

		var xmlElement = XElement.Parse(content);
		var lis = xmlElement.Element("ul")?.Elements("li").ToList();
		if (lis == null || lis.Count == 1 && lis.First().Value == "")
			return null;

		return htmlString;
	}
}

public static class HtmlContentExtensions
{
	public static string ToHtmlString(this IHtmlContent htmlContent)
	{
		if (htmlContent is HtmlString htmlString)
		{
			return htmlString.Value;
		}

		using (var writer = new StringWriter())
		{
			htmlContent.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
			return writer.ToString();
		}
	}
}

public class SelectListItemWithAttributes : SelectListItem
{
	public object HtmlAttributes = new();
}