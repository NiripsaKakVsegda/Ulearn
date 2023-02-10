using System.Text;
using System.Text.RegularExpressions;
using Database.Models;
using Database.Repos;
using Ionic.Zip;
using Newtonsoft.Json;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;
using Vostok.Logging.Abstractions;
using Web.Api.Configuration;

namespace uLearn.Web.Core.Utils;

public class CertificateGenerator
{
	private readonly IUserQuizzesRepo userQuizzesRepo;
	private readonly IUserSolutionsRepo userSolutionsRepo;
	private readonly ISlideCheckingsRepo slideCheckingsRepo;
	private readonly IVisitsRepo visitsRepo;
	private readonly WebConfiguration configuration;
	private readonly string contentRootPath;
	private readonly IUnitsRepo unitsRepo;

	private static ILog log => LogProvider.Get().ForContext(typeof(CertificatesRepo));

	public const string TemplateIndexFile = "index.html";
	private readonly Regex templateParameterRegex = new(@"%([-a-z0-9_.]+)(\|(raw|in_quotes|in_html))?%", RegexOptions.Compiled | RegexOptions.IgnoreCase);

	private readonly HashSet<string> builtInParameters = new()
	{
		"user.last_name", "user.first_name", "user.name",
		"instructor.last_name", "instructor.first_name", "instructor.name",
		"course.id", "course.title",
		"date", "date.year", "date.month", "date.day",
		"certificate.id", "certificate.url",
		"score",
		"codereviews.passed", "codereviews.passed_maxscore",
		"quizzes.passed", "quizzes.passed_maxscore",
		"exercises.accepted",
	};

	public CertificateGenerator(
		IUserQuizzesRepo userQuizzesRepo,
		IUserSolutionsRepo userSolutionsRepo,
		ISlideCheckingsRepo slideCheckingsRepo,
		IVisitsRepo visitsRepo,
		WebConfiguration configuration,
		IHostEnvironment env,
		IUnitsRepo unitsRepo)
	{
		this.userQuizzesRepo = userQuizzesRepo;
		this.userSolutionsRepo = userSolutionsRepo;
		this.slideCheckingsRepo = slideCheckingsRepo;
		this.visitsRepo = visitsRepo;
		this.unitsRepo = unitsRepo;
		this.configuration = configuration;
		this.contentRootPath = env.ContentRootPath;
	}

	public FileInfo GetTemplateArchivePath(CertificateTemplate template)
	{
		return GetTemplateArchivePath(template.ArchiveName);
	}

	private DirectoryInfo GetCertificatesDirectory()
	{
		var certificatesDirectory = configuration.OldWebConfig["ulearn.certificatesDirectory"];
		if (string.IsNullOrEmpty(certificatesDirectory))
			certificatesDirectory = Path.Combine(contentRootPath, "wwwroot", "Certificates");

		var directory = new DirectoryInfo(certificatesDirectory);
		if (!directory.Exists)
			directory.Create();
		return directory;
	}

	public FileInfo GetTemplateArchivePath(string templateArchiveName)
	{
		return GetCertificatesDirectory().GetFile(templateArchiveName + ".zip");
	}

	public DirectoryInfo GetTemplateDirectory(CertificateTemplate template)
	{
		return GetTemplateDirectory(template.ArchiveName);
	}

	public DirectoryInfo GetTemplateDirectory(string templateArchiveName)
	{
		return GetCertificatesDirectory().GetSubdirectory(templateArchiveName);
	}

	public void EnsureCertificateTemplateIsUnpacked(CertificateTemplate template)
	{
		var certificateDirectory = GetTemplateDirectory(template);
		if (!certificateDirectory.Exists)
		{
			log.Info($"Нет директории с распакованным шаблоном сертификата, Id = {template.Id}");

			var certificateArchive = GetTemplateArchivePath(template);
			if (!certificateArchive.Exists)
				throw new Exception("Can\'t find certificate template");

			log.Info($"Распаковываю шаблон сертификата {template.Id}: \"{certificateArchive.FullName}\" в \"{certificateDirectory.FullName}\"");

			using (var zip = ZipFile.Read(certificateArchive.FullName, new ReadOptions { Encoding = Encoding.UTF8 }))
			{
				zip.ExtractAll(certificateDirectory.FullName, ExtractExistingFileAction.OverwriteSilently);
			}
		}
	}

	public IEnumerable<string> GetTemplateParameters(CertificateTemplate template)
	{
		EnsureCertificateTemplateIsUnpacked(template);

		var templateDirectory = GetTemplateDirectory(template);
		var indexFile = templateDirectory.GetFile(TemplateIndexFile);
		if (!indexFile.Exists)
		{
			log.Error($"Не нашёл файла {TemplateIndexFile} в шаблоне \"{template.Name}\" (Id = {template.Id}, {template.ArchiveName})");
			yield break;
		}

		var foundParameters = new HashSet<string>();

		var matches = templateParameterRegex.Matches(File.ReadAllText(indexFile.FullName));
		foreach (Match match in matches)
		{
			var parameter = match.Groups[1].Value;
			if (!foundParameters.Contains(parameter))
			{
				yield return parameter;
				foundParameters.Add(parameter);
			}
		}
	}

	public IEnumerable<string> GetTemplateParametersWithoutBuiltins(CertificateTemplate template)
	{
		return GetTemplateParameters(template).Where(p => !builtInParameters.Contains(p)).Distinct();
	}

	public IEnumerable<string> GetBuiltinTemplateParameters(CertificateTemplate template)
	{
		return GetTemplateParameters(template).Where(p => builtInParameters.Contains(p)).Distinct();
	}

	public async Task<string> GetTemplateBuiltinParameterForUser(CertificateTemplate template, Course course, ApplicationUser user, ApplicationUser instructor, string parameterName)
	{
		var mockCertificate = new Certificate
		{
			Id = Guid.Empty,
			User = user,
			UserId = user.Id,
			Instructor = instructor,
			InstructorId = instructor.Id,
			Template = template,
			TemplateId = template.Id,
			Timestamp = DateTime.Now,
		};
		return await SubstituteBuiltinParameters($"%{parameterName}|raw%", mockCertificate, course, "<адрес сертификата>");
	}

	public async Task<string> RenderCertificate(Certificate certificate, Course course, string certificateUrl)
	{
		var templateDirectory = GetTemplateDirectory(certificate.Template);
		var indexFile = templateDirectory.GetFile(TemplateIndexFile);
		var content = File.ReadAllText(indexFile.FullName);

		return await SubstituteParameters(content, certificate, course, certificateUrl);
	}

	private string SubstituteOneParameter(string content, string parameterName, string parameterValue)
	{
		if (parameterValue == null)
			parameterValue = "";

		content = content.Replace($"%{parameterName}|raw%", parameterValue);

		var htmlEncodedValue = parameterValue.Replace("&", "&amp;").Replace(">", "&gt;").Replace("<", "&lt;");
		content = content.Replace($"%{parameterName}|in_html%", htmlEncodedValue);
		content = content.Replace($"%{parameterName}%", htmlEncodedValue);

		var quotesEncodedValue = parameterValue.EncodeQuotes();
		content = content.Replace($"%{parameterName}|in_quotes%", quotesEncodedValue);

		return content;
	}

	private string SubstituteOneBoolParameter(string content, string parameterName, bool parameterValue)
	{
		return Regex.Replace(
			content,
			$"%{parameterName}\\|(?<true>[^|%]+)\\|(?<false>[^|%]+)%",
			m => parameterValue ? m.Groups["true"].Value : m.Groups["false"].Value
		);
	}

	private async Task<string> SubstituteParameters(string content, Certificate certificate, Course course, string certificateUrl)
	{
		var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(certificate.Parameters);
		foreach (var kv in parameters)
		{
			content = SubstituteOneParameter(content, kv.Key, kv.Value);
		}

		content = await SubstituteBuiltinParameters(content, certificate, course, certificateUrl);

		return content;
	}

	private async Task<string> SubstituteBuiltinParameters(string content, Certificate certificate, Course course, string certificateUrl)
	{
		content = ReplaceBasicBuiltinParameters(content, certificate, course, certificateUrl);

		/* Replace %score% for total course score */
		var userScore = (await visitsRepo.GetScoresForSlides(course.Id, certificate.UserId))
			.Sum(p => p.Value);
		content = SubstituteOneParameter(content, "score", userScore.ToString());

		/* Replace %codereviews.*% */
		content = await ReplaceCodeReviewsBuiltinParameters(content, certificate, course);
		/* Replace %quizzes.*% */
		content = await ReplaceQuizzesBuiltinParameters(content, certificate, course);

		var visibleUnits = await unitsRepo.GetPublishedUnitIds(course);
		var acceptedSolutionsCount = userSolutionsRepo
			.GetAllAcceptedSubmissionsByUser(course.Id, course.GetSlides(false, visibleUnits).Select(s => s.Id), certificate.UserId)
			.Select(s => s.SlideId)
			.Distinct()
			.Count();
		content = SubstituteOneParameter(content, "exercises.accepted", acceptedSolutionsCount.ToString());

		return content;
	}

	private string ReplaceBasicBuiltinParameters(string content, Certificate certificate, Course course, string certificateUrl)
	{
		content = SubstituteOneParameter(content, "user.first_name", certificate.User.FirstName);
		content = SubstituteOneParameter(content, "user.last_name", certificate.User.LastName);
		content = SubstituteOneParameter(content, "user.name", certificate.User.VisibleName);

		content = SubstituteOneParameter(content, "instructor.first_name", certificate.Instructor.FirstName);
		content = SubstituteOneParameter(content, "instructor.last_name", certificate.Instructor.LastName);
		content = SubstituteOneParameter(content, "instructor.name", certificate.Instructor.VisibleName);

		content = SubstituteOneParameter(content, "course.id", course.Id);
		content = SubstituteOneParameter(content, "course.title", course.Title);

		content = SubstituteOneParameter(content, "date", certificate.Timestamp.ToLongDateString());
		content = SubstituteOneParameter(content, "date.day", certificate.Timestamp.Day.ToString());
		content = SubstituteOneParameter(content, "date.month", certificate.Timestamp.Month.ToString("D2"));
		content = SubstituteOneParameter(content, "date.year", certificate.Timestamp.Year.ToString());

		content = SubstituteOneParameter(content, "certificate.id", certificate.Id.ToString());
		content = SubstituteOneParameter(content, "certificate.url", certificateUrl);

		content = SubstituteOneBoolParameter(content, "by_gender", !certificate.User.Gender.HasValue || certificate.User.Gender == Gender.Male);

		return content;
	}

	private async Task<string> ReplaceQuizzesBuiltinParameters(string content, Certificate certificate, Course course)
	{
		var passedQuizzesCount = (await userQuizzesRepo.GetPassedSlideIdsAsync(course.Id, certificate.UserId)).Count;
		var scoredMaximumQuizzesCount = (await userQuizzesRepo.GetPassedSlideIdsWithMaximumScoreAsync(course.Id, certificate.UserId)).Count;

		content = SubstituteOneParameter(content, "quizzes.passed", passedQuizzesCount.ToString());
		content = SubstituteOneParameter(content, "quizzes.passed_maxscore", scoredMaximumQuizzesCount.ToString());
		return content;
	}

	private async Task<string> ReplaceCodeReviewsBuiltinParameters(string content, Certificate certificate, Course course)
	{
		var visibleUnits = await unitsRepo.GetPublishedUnitIds(course);
		var slideAndPercents = slideCheckingsRepo.GetPassedManualExerciseCheckingsAndPercents(course, certificate.UserId, visibleUnits);
		var codeReviewsCount = slideAndPercents.Count;
		var codeReviewsFullCount = slideAndPercents.Count(s => s.Percent == 100);

		content = SubstituteOneParameter(content, "codereviews.passed", codeReviewsCount.ToString());
		content = SubstituteOneParameter(content, "codereviews.passed_maxscore", codeReviewsFullCount.ToString());
		return content;
	}
}