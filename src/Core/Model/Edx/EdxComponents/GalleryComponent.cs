using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Ulearn.Core.Model.Edx.EdxComponents
{
	[XmlRoot("html")]
	public class GalleryComponent : Component
	{
		[XmlIgnore]
		public (FileInfo ImageFile, string RelativeToUnitDirectoryImagePath)[] Images;

		[XmlAttribute("filename")]
		public string Filename;

		[XmlIgnore]
		public override string SubfolderName => "html";

		public GalleryComponent()
		{
		}

		public GalleryComponent(string urlName, string displayName, string filename, (FileInfo ImageFile, string RelativeToUnitDirectoryImagePath)[] images)
		{
			UrlName = urlName;
			DisplayName = displayName;
			Filename = filename;
			Images = images;
		}

		public override void Save(string folderName)
		{
			base.Save(folderName);
			File.WriteAllText($"{folderName}/{SubfolderName}/{UrlName}.html", AsHtmlString());
		}

		public override void SaveAdditional(string folderName)
		{
			string PathToEdxFileName(string path) => path.Replace("\\", "/").Replace("/", "_").Replace(" ", "_");
			foreach (var (file, relativeToUnitDirectoryImagePath) in Images)
				File.Copy(file.FullName, $"{folderName}/static/{UrlName}_{PathToEdxFileName(relativeToUnitDirectoryImagePath)}", true);
			File.WriteAllText($"{folderName}/static/gallery_{UrlName}.html",
				File.ReadAllText($"{Utils.GetRootDirectory()}/templates/gallery.html")
					.Replace("{0}", string.Join("", Images.Select(t => "<li><img src='" + UrlName + "_" + PathToEdxFileName(t.RelativeToUnitDirectoryImagePath) + "' alt=''/></li>"))));
		}

		public override EdxReference GetReference()
		{
			return new HtmlComponentReference { UrlName = UrlName };
		}

		public override string AsHtmlString()
		{
			return File.ReadAllText($"{Utils.GetRootDirectory()}/templates/iframe.html")
				.Replace("{0}", "gallery_" + UrlName)
				.Replace("{1}", "(function (obj) { obj.style.height = '600px'; })(this);");
		}
	}
}