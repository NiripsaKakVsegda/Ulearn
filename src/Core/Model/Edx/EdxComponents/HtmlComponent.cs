using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Ulearn.Core.Model.Edx.EdxComponents
{
	public record StaticFileForEdx(FileInfo StaticFile, string EdxFileName);

	[XmlRoot("html")]
	public class HtmlComponent : Component
	{
		[XmlIgnore]
		public string HtmlContent;

		[XmlIgnore]
		public List<StaticFileForEdx> StaticFiles;

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

		public HtmlComponent(string urlName, string displayName, string filename, string htmlContent, List<StaticFileForEdx> staticFiles)
		{
			UrlName = urlName;
			DisplayName = displayName;
			Filename = filename;
			HtmlContent = htmlContent;
			StaticFiles = staticFiles;
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
				foreach (var (file, edxFileName) in StaticFiles.EmptyIfNull())
					File.Copy(file.FullName, $"{folderName}/static/{edxFileName}", overwrite: true);
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