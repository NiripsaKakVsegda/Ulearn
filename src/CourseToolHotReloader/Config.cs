using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Vostok.Logging.Abstractions;

namespace CourseToolHotReloader
{
	public interface IConfig
	{
		string Path { get; set; }
		Dictionary<string, string> CourseIds { get; set; }
		string CourseId { get; set; }
		string JwtToken { get; set; }
		public string ApiUrl { get; set; }
		public string SiteUrl { get; set; }
		public bool SendFullArchive { get; set; }
		public List<string> ExcludeCriterias { get; set; }
		public void Flush();
		public bool PreviousSendHasError { get; set; }
	}

	internal class Config : IConfig
	{
		private readonly string pathToConfigFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config.json");
		private static ILog log => LogProvider.Get().ForContext(typeof(Config));

		public Config()
		{
			var fileConfigFormat = ReadOrCreateConfig();
			JwtToken = fileConfigFormat.JwtToken;
			ApiUrl = fileConfigFormat.ApiUrl;
			SiteUrl = fileConfigFormat.SiteUrl;
			CourseIds = fileConfigFormat.CourseIds;
			Path = Directory.GetCurrentDirectory();
		}

		public string ApiUrl { get; set; }
		public string SiteUrl { get; set; }
		public bool SendFullArchive { get; set; }
		public string Path { get; set; }
		public Dictionary<string, string> CourseIds { get; set; }
		public List<string> ExcludeCriterias { get; set; }
		public string JwtToken { get; set; }
		public bool PreviousSendHasError { get; set; }

		public string CourseId
		{
			get => CourseIds[Path];
			set => CourseIds[Path] = value;
		}

		private FileConfigFormat ReadOrCreateConfig()
		{
			if (!File.Exists(pathToConfigFile))
			{
				CreateNewConfigFile();
			}

			try
			{
				return ReadConfig();
			}
			catch (JsonException ex)
			{
				log.Error(ex, "ReadConfig error");
				ConsoleWorker.WriteError("Не удалось прочитать файл с настройками CourseToolHotReloader. config.json создан с настройками по умолчанию в папке с исполнямыми файлами CourseToolHotReloader");
				CreateNewConfigFile();
				return ReadConfig();
			}
		}

		private FileConfigFormat ReadConfig()
		{
			using var streamReader = File.OpenText(pathToConfigFile);
			var json = streamReader.ReadToEnd();
			var fileConfigFormat = JsonSerializer.Deserialize<FileConfigFormat>(json);
			fileConfigFormat.CourseIds = fileConfigFormat.CourseIds
				.ToDictionary(kvp => kvp.Key
						.Replace('\\', System.IO.Path.DirectorySeparatorChar)
						.Replace('/', System.IO.Path.DirectorySeparatorChar)
					, kvp => kvp.Value);
			return fileConfigFormat;
		}

		private void CreateNewConfigFile()
		{
			var fileConfigFormat = new FileConfigFormat();
			SaveConfigFile(fileConfigFormat);
		}

		public void Flush()
		{
			var fileConfigFormat = new FileConfigFormat
			{
				JwtToken = JwtToken,
				ApiUrl = ApiUrl,
				SiteUrl = SiteUrl,
				SendFullArchive = SendFullArchive,
				CourseIds = CourseIds
			};

			SaveConfigFile(fileConfigFormat);
		}

		private void SaveConfigFile(FileConfigFormat fileConfigFormat)
		{
			using var fileStream = File.Create(pathToConfigFile);
			var bytes = JsonSerializer.SerializeToUtf8Bytes(fileConfigFormat, new JsonSerializerOptions
			{
				Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
				WriteIndented = true
			});
			fileStream.Write(bytes, 0, bytes.Length);
		}


		private class FileConfigFormat
		{
			public FileConfigFormat()
			{
				JwtToken = null;
				ApiUrl = "https://api.ulearn.me";
				SiteUrl = "https://ulearn.me";
				SendFullArchive = false;
				CourseIds = new Dictionary<string, string>();
			}

			[JsonPropertyName("jwtToken")]
			public string JwtToken { get; set; }

			[JsonPropertyName("apiUrl")]
			public string ApiUrl { get; set; }

			[JsonPropertyName("siteUrl")]
			public string SiteUrl { get; set; }

			[JsonPropertyName("sendFullArchive")]
			public bool SendFullArchive { get; set; }

			[JsonPropertyName("courseIds")]
			public Dictionary<string, string> CourseIds { get; set; }
		}
	}
}