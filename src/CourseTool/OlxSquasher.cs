using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Ulearn.Common.Extensions;

namespace uLearn.CourseTool
{
	public class OlxSquasher
	{
		public XDocument LoadItem(string type, string urlName, string olxDir)
		{
			return XDocument.Load(GetItemFilepath(type, urlName, olxDir));
		}

		private static string GetItemFilepath(string type, string urlName, string olxDir)
		{
			return Path.Combine(olxDir, type, urlName + ".xml");
		}

		public void SquashCourse(string olxDir)
		{
			var toRemove = new List<string>();
			foreach (var chapterFile in olxDir.PathCombine("chapter").GetFiles("*.xml"))
			{
				var chapterXml = XDocument.Load(chapterFile);
				SquashElements("sequential", chapterXml, olxDir, true, toRemove);
				SquashElements("vertical", chapterXml, olxDir, false, toRemove);
				SaveXml(chapterFile, chapterXml.Root);
			}

			foreach (var file in toRemove)
				File.Delete(file);
		}

		public void DesquashCourse(string olxDir)
		{
			foreach (var chapterFile in olxDir.PathCombine("chapter").GetFiles("*.xml"))
			{
				Console.WriteLine(chapterFile);
				var chapterXml = XDocument.Load(chapterFile);
				DesquashElements("sequential", "vertical", chapterXml, olxDir);
				SaveXml(chapterFile, chapterXml.Root);
			}
		}

		private void DesquashElements(string elementType, string subelementType, XDocument chapterXml, string olxDir)
		{
			foreach (var element in chapterXml.Root.EnsureNotNull().Elements(elementType))
			{
				if (!element.HasElements)
					continue;
				var urlNameAttr = element.Attribute("url_name").EnsureNotNull();
				var urlName = urlNameAttr.Value;
				Console.WriteLine("desquashing " + urlName);
				foreach (var subelement in element.Elements(subelementType))
				{
					var subUrlName = subelement.Attribute("url_name")?.Value;
					subelement.RemoveAttributes();
					subelement.SetAttributeValue("url_name", subUrlName);
				}

				urlNameAttr.Remove();
				var filename = olxDir.PathCombine(elementType).PathCombine(urlName + ".xml");
				SaveXml(filename, element);
				element.RemoveAll();
				element.SetAttributeValue("url_name", urlName);
			}
		}

		private static void SaveXml(string filename, XElement element)
		{
			var settings = new XmlWriterSettings() { OmitXmlDeclaration = true, Indent = true, Encoding = new UTF8Encoding(false) };
			using (var w = XmlWriter.Create(filename, settings))
				element.Save(w);
		}

		private void SquashElements(string elementType, XDocument courseXml, string olxDir, bool content, List<string> filesToRemove)
		{
			foreach (var element in courseXml.Root.EnsureNotNull().XPathSelectElements("//" + elementType))
			{
				var urlName = element.Attribute("url_name")?.Value;
				Console.WriteLine("squashing " + elementType + " " + urlName);
				if (element.HasElements || urlName == null)
					continue;
				var root = LoadItem(elementType, urlName, olxDir).Root.EnsureNotNull();
				//element.Add(root.Attributes());
				var obsoleteAttr = element.Attributes().Where(a => root.Attribute(a.Name) != null).ToList();
				foreach (var attr in obsoleteAttr)
					attr.Remove();
				element.Add(root.Attributes());
				if (content)
				{
					element.Add(root.Nodes());
					filesToRemove.Add(GetItemFilepath(elementType, urlName, olxDir));
				}
			}
		}
	}
}