using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace Ulearn.Core
{
	public class XmlValidator
	{
		private readonly XmlReaderSettings settings;

		public XmlValidator(string schemaPath)
		{
			var schemaSet = new XmlSchemaSet();

			using (var r = XmlReader.Create(schemaPath))
			{
				schemaSet.Add(XmlSchema.Read(r, null));
			}

			AppContext.SetSwitch("Switch.System.Xml.AllowDefaultResolver", true);
			schemaSet.XmlResolver = new XmlUrlResolver();

			schemaSet.CompilationSettings = new XmlSchemaCompilationSettings();
			schemaSet.Compile();

			settings = new XmlReaderSettings
			{
				CloseInput = true,
				ValidationType = ValidationType.Schema,
				Schemas = schemaSet,
				ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings |
								XmlSchemaValidationFlags.ProcessIdentityConstraints |
								XmlSchemaValidationFlags.ProcessInlineSchema |
								XmlSchemaValidationFlags.ProcessSchemaLocation
			};
		}

		public string ValidateSlideFile(FileInfo file)
		{
			var log = new List<string>();
			log.Add(file.Directory != null ? $"Ошибки в слайде {file.Directory.Name}/{file.Name}:" : $"Ошибки в слайде {file.FullName}:");

			void Action(object sender, ValidationEventArgs e)
			{
				var text = $"	[Line: {e.Exception?.LineNumber}, Column: {e.Exception?.LinePosition}]: {e.Message}";
				log.Add(text);
			}

			settings.ValidationEventHandler += Action;
			using (var validatingReader = XmlReader.Create(file.FullName, settings))
			{
				var x = new XmlDocument();
				x.Load(validatingReader);

				while (validatingReader.Read())
				{
				}
			}

			return log.Count > 1 ? string.Join("\n", log) : null;
		}

		public string ValidateSlidesFiles(List<FileInfo> files)
		{
			return string.Join("\n", files.Select(ValidateSlideFile).Where(x => x != null));
		}
	}
}