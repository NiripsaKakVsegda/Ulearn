using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using CourseToolHotReloader.Infrastructure;

namespace CourseToolHotReloader.Configs;

public interface IConfig
{
	string ApiUrl { get; }
	string SiteUrl { get; }
	ISet<CourseInfo> Courses { get; }
	string? JwtToken { get; set; }
	string? UserId { get; set; }
	CourseInfo? LastUpdatedCourse { get; set; }
	public bool IsErrorNotificationsEnabled { get; set; }
	Result<None> Flush();
}

internal class Config : IConfig
{
	public string ApiUrl { get; set; }
	public string SiteUrl { get; set; }
	public ISet<CourseInfo> Courses { get; }
	public CourseInfo? LastUpdatedCourse { get; set; }
	public bool IsErrorNotificationsEnabled { get; set; }
	public string? UserId { get; set; }
	public string? JwtToken { get; set; }

	private static readonly string pathToConfigFile = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
		"Ulearn",
		"config.json"
	);

	public Config(string apiUrl, string siteUrl, IEnumerable<CourseInfo>? courseInfos = null)
	{
		ApiUrl = apiUrl;
		SiteUrl = siteUrl;
		IsErrorNotificationsEnabled = false;
		Courses = courseInfos.EmptyIfNull().ToHashSet();
	}

	private Config(FileConfigFormat fileConfigFormat)
	{
		ApiUrl = fileConfigFormat.ApiUrl;
		SiteUrl = fileConfigFormat.SiteUrl;
		JwtToken = fileConfigFormat.JwtToken;
		IsErrorNotificationsEnabled = fileConfigFormat.IsErrorNotificationsEnabled;
		Courses = fileConfigFormat.Courses
			.Select(info => new CourseInfo
				{
					CourseId = info.CourseId,
					Path = info.Path,
					ExcludeCriterias = new List<string>(),
					CourseState = CourseState.NotSent,
					SendFullArchive = info.SendFullArchive
				}
			)
			.ToHashSet();
	}

	public static Result<IConfig> ReadOrCreateConfig()
	{
		return ReadOrCreateFileConfigFormat()
			.Then(fileConfigFormat => (IConfig)new Config(fileConfigFormat))
			.RefineError("Ошибка создания конфигурации")
			.OnFail(ConsoleWorker.WriteError);
	}

	public Result<None> Flush()
	{
		var fileConfigFormat = new FileConfigFormat
		{
			JwtToken = JwtToken,
			ApiUrl = ApiUrl,
			SiteUrl = SiteUrl,
			IsErrorNotificationsEnabled = IsErrorNotificationsEnabled,
			Courses = Courses.Select(courseInfo => new CourseInfoConfigFormat
			{
				Path = courseInfo.Path,
				CourseId = courseInfo.CourseId,
				SendFullArchive = courseInfo.SendFullArchive
			}).ToList()
		};

		return Result.OfAction(() => SaveConfigFile(fileConfigFormat), e => e.GetMessage())
			.RefineError("Не удалось сохранить конфиг файл.")
			.OnFail(ConsoleWorker.WriteError);
	}

	private static Result<FileConfigFormat> ReadOrCreateFileConfigFormat()
	{
		if (!File.Exists(pathToConfigFile))
		{
			var createResult = CreateNewConfigFile().RefineError("Ошибка во время создания файла конфигурации");
			if (!createResult.IsSuccess)
				return Result.Fail<FileConfigFormat>(createResult.Error, createResult.Exception);
		}

		var result = Result.Of(ReadFileConfigFormat)
			.RefineError("Не удалось прочитать файл с настройками CourseToolHotReloader. " +
						"config.json создан с настройками по умолчанию в папке с исполняемыми файлами CourseToolHotReloader");

		if (result.IsSuccess)
			return result;

		var createNewResult = CreateNewConfigFile().RefineError("Ошибка во время создания файла конфигурации");
		if (!createNewResult.IsSuccess)
			return Result.Fail<FileConfigFormat>(createNewResult.Error, createNewResult.Exception);

		return Result.Of(ReadFileConfigFormat);
	}

	private static FileConfigFormat ReadFileConfigFormat()
	{
		using var streamReader = File.OpenText(pathToConfigFile);
		var json = streamReader.ReadToEnd();
		var fileConfigFormat = JsonSerializer.Deserialize<FileConfigFormat>(json)!;
		foreach (var course in fileConfigFormat.Courses)
			course.Path = course.Path
				.Replace('\\', Path.DirectorySeparatorChar)
				.Replace('/', Path.DirectorySeparatorChar);
		return fileConfigFormat;
	}

	private static Result<None> CreateNewConfigFile()
	{
		var fileConfigFormat = new FileConfigFormat();
		return Result.OfAction(() => SaveConfigFile(fileConfigFormat));
	}

	private static void SaveConfigFile(FileConfigFormat fileConfigFormat)
	{
		var fileInfo = new FileInfo(pathToConfigFile);
		fileInfo.Directory?.Create();
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
			IsErrorNotificationsEnabled = false;
			Courses = new List<CourseInfoConfigFormat>();
		}

		[JsonPropertyName("jwtToken")]
		public string? JwtToken { get; set; }

		[JsonPropertyName("apiUrl")]
		public string ApiUrl { get; set; }

		[JsonPropertyName("siteUrl")]
		public string SiteUrl { get; set; }

		[JsonPropertyName("enableErrorNotifications")]
		public bool IsErrorNotificationsEnabled { get; set; }

		[JsonPropertyName("courses")]
		public List<CourseInfoConfigFormat> Courses { get; set; }
	}

	private class CourseInfoConfigFormat
	{
		[JsonPropertyName("path")]
		public string Path { get; set; } = null!;

		[JsonPropertyName("courseId")]
		public string CourseId { get; set; } = null!;

		[JsonPropertyName("sendFullArchive")]
		public bool SendFullArchive { get; set; }
	}
}