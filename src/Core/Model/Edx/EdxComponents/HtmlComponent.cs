using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Ulearn.Core.Model.Edx.EdxComponents
{
	[XmlRoot("html")]
	public class HtmlComponent : Component
	{
		[XmlIgnore]
		public string HtmlContent;

		[XmlIgnore]
		public string CourseDirectory;

		[XmlIgnore]
		public List<string> LocalFilesPathsRelativeToCourse;

		[XmlAttribute("filename")]
		public string Filename;

		[XmlIgnore]
		public override string SubfolderName
		{
			get { return "html"; }
		}

		[XmlIgnore]
		public Component[] Subcomponents;

		public HtmlComponent()
		{
		}

		public HtmlComponent(string urlName, string displayName, string filename, string htmlContent)
		{
			UrlName = urlName;
			DisplayName = displayName;
			Filename = filename;
			HtmlContent = htmlContent;
		}

		public HtmlComponent(string urlName, string displayName, string filename, string htmlContent, string courseDirectory, string unitDirectoryRelativeToCourse, List<string> localFilesPathsRelativeToCourse)
		{
			UrlName = urlName;
			DisplayName = displayName;
			Filename = filename;
			HtmlContent = htmlContent;
			CourseDirectory = courseDirectory;
			LocalFilesPathsRelativeToCourse = localFilesPathsRelativeToCourse;
		}

		public override void Save(string folderName)
		{
			base.Save(folderName);
			File.WriteAllText(string.Format("{0}/{1}/{2}.html", folderName, SubfolderName, UrlName), HtmlContent);
		}

		public override void SaveAdditional(string folderName)
		{
			if (Subcomponents != null)
				foreach (var subcomponent in Subcomponents)
					subcomponent.SaveAdditional(folderName);
			try
			{
				foreach (var localFilesPathsRelativeToCourse in LocalFilesPathsRelativeToCourse ?? new List<string>())
					File.Copy(
						Path.Combine(CourseDirectory, localFilesPathsRelativeToCourse),
						string.Format("{0}/static/{1}_{2}", folderName, UrlName, localFilesPathsRelativeToCourse.Replace("/", "_")),
						overwrite: true);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public override EdxReference GetReference()
		{
			return new HtmlComponentReference { UrlName = UrlName };
		}

		public override string AsHtmlString()
		{
			return HtmlContent;
		}

		public static HtmlComponent Load(string folderName, string urlName, EdxLoadOptions options)
		{
			return Load<HtmlComponent>(folderName, "html", urlName, options,
				c => { c.HtmlContent = File.ReadAllText(string.Format("{0}/html/{1}.html", folderName, c.Filename)); });
		}
	}
}