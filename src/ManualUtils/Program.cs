using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AntiPlagiarism.Web.Database;
using AntiPlagiarism.Web.Database.Models;
using Database;
using Database.Di;
using Database.Models;
using Database.Repos;
using Database.Repos.Users;
using Ionic.Zip;
using ManualUtils.AntiPlagiarism;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Helpers;
using Ulearn.Core.Logging;
using Ulearn.Web.Api.Utils.Courses;
using Ulearn.Web.Api.Utils.LTI;
using Vostok.Logging.Abstractions;
using Vostok.Logging.File;
using Vostok.Logging.Microsoft;

namespace ManualUtils
{
	internal class Program
	{
		public static async Task Main(string[] args)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			Console.OutputEncoding = Encoding.UTF8;
			var configuration = ApplicationConfiguration.Read<UlearnConfiguration>();
			LoggerSetup.Setup(configuration.HostLog, configuration.GraphiteServiceName);
			try
			{
				var optionsBuilder = new DbContextOptionsBuilder<UlearnDb>()
					.UseLazyLoadingProxies()
					.UseNpgsql(configuration.Database, o => o.SetPostgresVersion(13, 2));
				var db = new UlearnDb(optionsBuilder.Options);
				var aOptionsBuilder = new DbContextOptionsBuilder<AntiPlagiarismDb>()
					.UseLazyLoadingProxies()
					.UseNpgsql(configuration.Database, o => o.SetPostgresVersion(13, 2));
				var adb = new AntiPlagiarismDb(aOptionsBuilder.Options);
				var serviceProvider = ConfigureDI(adb, db);

				var courseManager = serviceProvider.GetService<ISlaveCourseManager>();
				await courseManager.UpdateCoursesAsync();
				await courseManager.UpdateTempCoursesAsync();

				await Run(adb, db, serviceProvider);
			}
			finally
			{
				await FileLog.FlushAllAsync();
			}
		}

		private static IServiceProvider ConfigureDI(AntiPlagiarismDb adb, UlearnDb db)
		{
			var services = new ServiceCollection();
			services.AddLogging(builder => builder.AddVostok(LogProvider.Get()));

			services.AddSingleton(db);
			services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<UlearnDb>();
			services.AddDatabaseServices(false);

			services.AddSingleton(SlaveCourseManager.CourseStorageInstance);
			services.AddSingleton(SlaveCourseManager.CourseStorageUpdaterInstance);
			services.AddSingleton<ISlaveCourseManager, SlaveCourseManager>();
			services.AddSingleton<ExerciseStudentZipsCache>();

			services.AddScoped<TempCourseRemover>();

			services.AddSingleton(adb);

			return services.BuildServiceProvider();
		}

		private static async Task Run(AntiPlagiarismDb adb, UlearnDb db, IServiceProvider serviceProvider)
		{
			// await serviceProvider.GetService<IUsersRepo>().CreateUlearnBotUserIfNotExists();
			// FillLanguageToAntiplagiarism.FillLanguage(adb);
			// GenerateUpdateSequences();
			// CompareColumns();
			// await ResendLti(serviceProvider);
			// await FindExternalSolutionsPlagiarism.UploadSolutions();
			// await FindExternalSolutionsPlagiarism.GetRawResults();
			// await FindExternalSolutionsPlagiarism.PrepareResults();
			// await UpdateExerciseVisits(serviceProvider, "fpIntroduction");
			//
			// Users.PrintCourseAdmins(db);
			// await ScoresUpdater.UpdateTests(serviceProvider, "java-rtf");
			// GetMostSimilarSubmission(adb);
			// ParsePairWeightsFromLogs();
			// GetBlackAndWhiteLabels(db, adb);
			// ParseTaskWeightsFromLogs(serviceProvider);
			// CampusRegistration(db);
			// GetIps(db);
			// FillAntiplagFields.FillClientSubmissionId(adb);
			// await XQueueRunAutomaticChecking(db);
			// TextBlobsWithZeroByte(db);
			// UpdateCertificateArchives(db);
			// GetVKByEmail(serviceProvider);
			// TestStagingZipsEncodings();
			// ConvertZipsToCourseXmlInRoot();
			// await UploadStagingToDb(serviceProvider);
			// await UploadStagingFromDbAndExtractToCourses(serviceProvider);
			// await SetCourseIdAndSlideIdInLikesAndPromotes(db);
			// await SetNewFieldsInReview(db, serviceProvider);
			// await UploadCourseVersions(serviceProvider);
			// await RemoveVersionsWithoutFile(serviceProvider);
			// await RemoveDuplicateExerciseManualCheckings(serviceProvider);
			// await RemoveTempCourse(serviceProvider);
			// await AddVersionFilesToCoursesOnDisk(serviceProvider);
			// await CreateNewVersionFromZip(serviceProvider, "test4", @"C:\Users\vorkulsky\Downloads\test4.zip");
			// await SetCourseNamesToVersions(serviceProvider, false);
			// await UnifyVisits(serviceProvider);
			// await BuildFavouriteReviewsFromReviews(serviceProvider);
			// await FixCheckedManualCheckingsWithoutScoreAndPercent(serviceProvider);
			// await SetPercentByScore(serviceProvider);
			// await ScoresUpdater.UpdateVisitsForExercises(serviceProvider, new DateTime(2021, 8, 15));
		}

		private static void GenerateUpdateSequences()
		{
			var lines = File.ReadAllLines(@"C:\git\Ulearn-postgres\tools\pgloader\files\01_create_tables.sql");
			var tableAndIdRegex = new Regex(@"ALTER TABLE ([\w\.\""]+) ALTER COLUMN ([\w\.\""]+) ADD GENERATED BY DEFAULT AS IDENTITY");
			var sequenceIdRegex = new Regex(@"SEQUENCE NAME ([\w\.\""]+)(\s|$)");
			var parsed = new List<Tuple<string, string, string>>();
			foreach (var line in lines)
			{
				var match = tableAndIdRegex.Match(line);
				if (match.Success)
				{
					var table = match.Groups[1].Value;
					var id = match.Groups[2].Value;
					parsed.Add(Tuple.Create(table, id, (string)null));
				}

				var sequenceMatch = sequenceIdRegex.Match(line);
				if (sequenceMatch.Success)
				{
					var sequenceId = sequenceMatch.Groups[1].Value;
					parsed[parsed.Count - 1] = Tuple.Create(parsed[parsed.Count - 1].Item1, parsed[parsed.Count - 1].Item2, sequenceId);
				}
			}

			var strings = parsed.Select(p => $@"SELECT setval('{p.Item3}', COALESCE((SELECT MAX({p.Item2})+1 FROM {p.Item1}), 1), false);" + "\n").ToList();
			File.WriteAllLines(@"C:\git\Ulearn-postgres\tools\pgloader\files\update_sequences.sql", strings);
		}

		private static async Task ResendLti(IServiceProvider serviceProvider)
		{
			var ltiConsumersRepo = serviceProvider.GetService<ILtiConsumersRepo>();
			var visitsRepo = serviceProvider.GetService<VisitsRepo>();
			var courseStorage = serviceProvider.GetService<ICourseStorage>();
			var db = serviceProvider.GetService<UlearnDb>();

			// current 288064
			var ltiRequests = await db.LtiRequests.Where(r => r.RequestId > 285417).OrderByDescending(r => r.RequestId).ToListAsync();

			var i = 0;
			foreach (var ltiRequest in ltiRequests)
			{
				i++;
				Console.WriteLine($"{i} requestId {ltiRequest.RequestId}");
				try
				{
					var course = courseStorage.GetCourse(ltiRequest.CourseId);
					var slide = course.GetSlideByIdNotSafe(ltiRequest.SlideId);
					var score = await visitsRepo.GetScore(ltiRequest.CourseId, ltiRequest.SlideId, ltiRequest.UserId);
					await LtiUtils.SubmitScore(slide, ltiRequest.UserId, score, ltiRequest.Request, ltiConsumersRepo);
					Console.WriteLine($"{i} requestId {ltiRequest.RequestId} score {score}");
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}

		private static async Task UpdateExerciseVisits(IServiceProvider serviceProvider, string courseId)
		{
			var courseStorage = serviceProvider.GetService<ICourseStorage>();
			var course = courseStorage.GetCourse(courseId);
			var visitsRepo = serviceProvider.GetService<IVisitsRepo>();
			var db = serviceProvider.GetService<UlearnDb>();
			var slides = course.GetSlidesNotSafe().OfType<ExerciseSlide>().ToList();
			foreach (var slide in slides)
			{
				var slideVisits = db.Visits.Where(v => v.CourseId == courseId && v.SlideId == slide.Id && v.IsPassed).ToList();
				foreach (var visit in slideVisits)
				{
					await visitsRepo.UpdateScoreForVisit(courseId, slide, visit.UserId);
				}
			}
		}

		private static async Task XQueueRunAutomaticChecking(UlearnDb db)
		{
			var userSolutionsRepo = new UserSolutionsRepo(db, new TextsRepo(db), new WorkQueueRepo(db), null);
			var time = new DateTime(2021, 1, 30);
			var submissions = await db.UserExerciseSubmissions.Where(s =>
					s.AutomaticChecking.DisplayName == "XQueue watcher Stepik.org"
					&& (s.AutomaticChecking.Status == AutomaticExerciseCheckingStatus.Waiting || s.AutomaticChecking.Status == AutomaticExerciseCheckingStatus.RequestTimeLimit)
					&& s.AutomaticChecking.Timestamp > time)
				.OrderBy(c => c.Timestamp)
				.ToListAsync();
			var i = 0;
			foreach (var submission in submissions)
			{
				Console.WriteLine($"{i} from {submissions.Count} {submission.Id}");
				await userSolutionsRepo.RunAutomaticChecking(submission.Id, submission.Sandbox, TimeSpan.FromSeconds(25), false, -10);
				Thread.Sleep(1000);
				var result = await db.UserExerciseSubmissions
					.Include(s => s.AutomaticChecking)
					.Where(s => s.Id == submission.Id).FirstOrDefaultAsync();
				Console.WriteLine($"IsRightAnswer: {result.AutomaticCheckingIsRightAnswer}");
				Console.WriteLine($"Status: {result.AutomaticChecking.Status}");
				i++;
			}
		}

		private static void GetMostSimilarSubmission(AntiPlagiarismDb adb)
		{
			//var lines = File.ReadLines("pairweights.txt");
			//var jsons = AntiplagiarismLogsParser.GetWeightsOfSubmissionPairs(lines).Select(JsonConvert.SerializeObject);
			//File.WriteAllLines("result.txt", jsons);
			var bestPairWeight = File.ReadLines("result.txt").Select(JsonConvert.DeserializeObject<BestPairWeight>);
			var now = DateTime.UtcNow;
			var mostSimilarSubmissions = bestPairWeight.Select(s => new MostSimilarSubmission
			{
				SubmissionId = s.Submission,
				SimilarSubmissionId = s.Other,
				Weight = s.Weight,
				Timestamp = now
			}).ToList();

			var exist = adb.MostSimilarSubmissions.Select(s => s.SubmissionId).ToList().ToHashSet();
			var i = 0;
			foreach (var mostSimilarSubmission in mostSimilarSubmissions)
			{
				if (exist.Contains(mostSimilarSubmission.SubmissionId))
					continue;
				adb.MostSimilarSubmissions.Add(mostSimilarSubmission);
				if (i % 1000 == 0)
				{
					adb.SaveChanges();
					Console.WriteLine(i);
				}

				i++;
			}

			adb.SaveChanges();
		}

		private static void GetBlackAndWhiteLabels(UlearnDb db, AntiPlagiarismDb adb)
		{
			var lines = File.ReadLines("pairweights.txt");
			var jsons = PlagiarismInstructorDecisions.GetBlackAndWhiteLabels(db, adb,
				lines.Select(JsonConvert.DeserializeObject<BestPairWeight>));
			File.WriteAllLines("blackAndWhiteLabels.txt", jsons);
		}

		private static void ParsePairWeightsFromLogs()
		{
			var lines = File.ReadLines("pairweights.txt");
			var jsons = AntiplagiarismLogsParser.GetWeightsOfSubmissionPairs(lines).Select(JsonConvert.SerializeObject);
			File.WriteAllLines("result.txt", jsons);
		}

		private static void ParseTaskWeightsFromLogs(IServiceProvider serviceProvider)
		{
			var lines = File.ReadLines("weights.txt");
			var jsons = AntiplagiarismLogsParser.GetWeightsForStatistics(serviceProvider, lines);
			File.WriteAllLines("result.txt", jsons);
		}

		private static void CampusRegistration(UlearnDb db)
		{
			ManualUtils.CampusRegistration.Run(db, courseId: "Campus1920", groupId: 803, slideWithRegistrationQuiz: new Guid("67bf45bd-bebc-4bde-a705-c16763b94678"), false);
		}

		private static void GetIps(UlearnDb db)
		{
			// Для получения городов см. geoip.py
			// Где взять GeoLite2-City.mmdb читай в GeoLite2-City.mmdb.readme.txt
			var courses = new[] { "BasicProgramming", "BasicProgramming2", "Linq", "complexity", "CS2" };
			GetIpAddresses.Run(db, lastMonthCount: 13, courses, isNotMembersOfGroups: true, onlyRegisteredFrom: true);
		}

		private static void CompareColumns()
		{
			var postgres = File.ReadAllLines(@"C:\Users\vorkulsky\Downloads\postgres.csv")
				.Select(s => s.Split('\t'))
				.Select(p => (Table: p[0], Column: p[1]))
				.GroupBy(p => p.Table)
				.ToDictionary(p => p.Key, p => p.Select(t => t.Column).ToHashSet());
			var sqlserver = File.ReadAllLines(@"C:\Users\vorkulsky\Downloads\sql server.txt")
				.Select(s => s.Split('\t'))
				.Select(p => (Table: p[1], Column: p[0]))
				.GroupBy(p => p.Table)
				.ToDictionary(p => p.Key, p => p.Select(t => t.Column).ToHashSet());
			foreach (var table in sqlserver.Keys)
			{
				if (!postgres.ContainsKey(table))
					continue;
				postgres[table].SymmetricExceptWith(sqlserver[table]);
				if (postgres[table].Count > 0)
					Console.WriteLine($"{table}:" + string.Join(", ", postgres[table]));
			}
		}

		private static void TextBlobsWithZeroByte(UlearnDb db)
		{
			int i = 0;
			var hashes = new List<string>();
			foreach (var text in db.Texts.AsNoTracking())
			{
				if (text.Text.Contains('\0'))
				{
					Console.WriteLine(i);
					hashes.Add(text.Hash);
					i++;
				}
			}

			i = 0;
			foreach (var hash in hashes)
			{
				var temp = db.Texts.Find(hash);
				temp.Text = temp.Text.Replace("\0", "");
				Console.WriteLine("s" + i);
				i++;
				db.SaveChanges();
			}
		}

		private static void UpdateCertificateArchives(UlearnDb db)
		{
			var directory = new DirectoryInfo("Templates");
			foreach (var file in directory.EnumerateFiles())
			{
				var guid = file.Name.Split(".")[0];
				var content = file.ReadAllContent();
				var a = db.CertificateTemplateArchives.Find(guid);
				if (a != null)
				{
					a.Content = content;
					db.SaveChanges();
				}
				else
				{
					var id = db.CertificateTemplates.FirstOrDefault(t => t.ArchiveName == guid).Id;
					db.CertificateTemplateArchives.Add(new CertificateTemplateArchive
					{
						ArchiveName = guid,
						Content = content,
						CertificateTemplateId = id
					});
					db.SaveChanges();
				}

				Console.WriteLine(guid);
			}
		}

		private static async Task GetVKByEmail(IServiceProvider serviceProvider)
		{
			var emails = File.ReadAllLines("emails.txt").Select(l => l.Trim());
			var db = serviceProvider.GetService<UlearnDb>();

			foreach (var email in emails)
			{
				string vk = null;
				var user = db.Users.FirstOrDefault(u => u.Email == email);
				if (user != null)
				{
					var vkLogin = db.UserLogins.FirstOrDefault(l => l.LoginProvider == "ВКонтакте" && l.UserId == user.Id);
					if (vkLogin != null)
					{
						vk = $"https://vk.com/id{vkLogin.ProviderKey}";
					}
				}

				Console.WriteLine($"{email}\t{vk}");
			}
		}

		// Результат, дял всех архивов подходит использовать 866 в ReadOptions. Кажется, применяется utf-8, где нужно.
		// А вот AlternateEncoding заставляет использовать 866 везде.
		private static void TestStagingZipsEncodings()
		{
			var charactersRegex = new Regex(@"^[\\\/\w\s\d\--_\.#№'+\(\),=]*$");
			var files = new DirectoryInfo(@"C:\Users\vorkulsky\Desktop\Courses.Staging").GetFiles();
			var encoding = ZipUtils.Cp866;
			foreach (var file in files)
			{
				using (var zip = ZipFile.Read(file.FullName, new ReadOptions { Encoding = encoding }))
				{
					//zip.AlternateEncoding = encoding;
					var names = zip.Entries.Select(e => e.FileName).ToList();
					foreach (var name in names)
					{
						if (!charactersRegex.IsMatch(name))
							Console.WriteLine($"{file.Name} {name}");
					}
				}
			}
		}

		private static void ConvertZipsToCourseXmlInRoot()
		{
			var mainDirectory = CourseManager.CoursesDirectory;
			var stagingDirectory = mainDirectory.GetSubdirectory("Courses.Staging");
			var versionsDirectory = mainDirectory.GetSubdirectory("Courses.Versions");

			var newMainDirectory = mainDirectory.Parent.CreateSubdirectory("courses.new");
			newMainDirectory.EnsureExists();
			newMainDirectory.ClearDirectory();
			var newStagingDirectory = newMainDirectory.GetSubdirectory("Courses.Staging");
			newStagingDirectory.EnsureExists();
			var newVersionsDirectory = newMainDirectory.GetSubdirectory("Courses.Versions");
			newVersionsDirectory.EnsureExists();
			var newCoursesDirectory = newMainDirectory.GetSubdirectory("Courses");
			newCoursesDirectory.EnsureExists();

			ProcessZips(stagingDirectory, newStagingDirectory);
			ProcessZips(versionsDirectory, newVersionsDirectory);

			foreach (var file in newStagingDirectory.GetFiles("*.zip"))
			{
				using (var zip = ZipFile.Read(file.FullName, new ReadOptions { Encoding = ZipUtils.Cp866 }))
				{
					zip.ExtractAll(Path.Combine(newCoursesDirectory.FullName, file.Name.Replace(".zip", "")), ExtractExistingFileAction.OverwriteSilently);
				}
			}
		}

		private static void ProcessZips(DirectoryInfo oldDirectory, DirectoryInfo newDirectory)
		{
			foreach (var file in oldDirectory.GetFiles("*.zip"))
			{
				try
				{
					Console.WriteLine($"Start process {file.Name}");
					using (var stream = ZipUtils.GetZipWithFileWithNameInRoot(file.FullName, "course.xml"))
					using (var fileStream = File.Create(Path.Combine(newDirectory.FullName, file.Name)))
						stream.CopyTo(fileStream);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error on {file.Name} " + ex.Message);
				}
			}
		}

		private static async Task UploadStagingToDb(IServiceProvider serviceProvider)
		{
			var stagingDirectory = CourseManager.CoursesDirectory.GetSubdirectory("Courses.Staging");

			var db = serviceProvider.GetService<UlearnDb>();
			var coursesRepo = serviceProvider.GetService<ICoursesRepo>();

			var publishedCourseVersions = await coursesRepo.GetPublishedCourseVersions();

			foreach (var publishedCourseVersion in publishedCourseVersions)
			{
				var versionId = publishedCourseVersion.Id;
				var fileInDb = await coursesRepo.GetVersionFile(versionId);
				var zip = stagingDirectory.GetFile($"{fileInDb.CourseId}.zip");
				if (!zip.Exists)
				{
					Console.WriteLine($"{fileInDb.CourseId}.zip does not exist");
					return;
				}

				var content = await zip.ReadAllContentAsync();
				if (fileInDb.File.Length != content.Length)
				{
					Console.WriteLine($"Upload course {fileInDb.CourseId}.zip uploaded");
					fileInDb.File = content;
					db.SaveChanges();
				}
			}
		}

		/*private static async Task UploadStagingFromDbAndExtractToCourses(IServiceProvider serviceProvider)
		{
			var db = serviceProvider.GetService<UlearnDb>();
			var courseManager = serviceProvider.GetService<ISlaveCourseManager>();
			var coursesRepo = serviceProvider.GetService<ICoursesRepo>();

			var publishedCourseVersions = await coursesRepo.GetPublishedCourseVersions();

			foreach (var publishedCourseVersion in publishedCourseVersions)
			{
				var fileInDb = await coursesRepo.GetVersionFile(publishedCourseVersion.Id);
				var stagingCourseFile = courseManager.GetStagingCourseFile(fileInDb.CourseId);
				await File.WriteAllBytesAsync(stagingCourseFile.FullName, fileInDb.File);
				var versionCourseFile = courseManager.GetCourseVersionFile(fileInDb.CourseVersionId);
				if (!versionCourseFile.Exists)
					await File.WriteAllBytesAsync(versionCourseFile.FullName, fileInDb.File);
				var unpackDirectory = courseManager.GetExtractedCourseDirectory(fileInDb.CourseId);
				using (var zip = ZipFile.Read(stagingCourseFile.FullName, new ReadOptions { Encoding = ZipUtils.Cp866 }))
				{
					zip.ExtractAll(unpackDirectory.FullName, ExtractExistingFileAction.OverwriteSilently);
					foreach (var f in unpackDirectory.GetFiles("*", SearchOption.AllDirectories).Cast<FileSystemInfo>().Concat(unpackDirectory.GetDirectories("*", SearchOption.AllDirectories)))
						f.Attributes &= ~FileAttributes.ReadOnly;
				}
			}
		}*/

		private static async Task SetCourseIdAndSlideIdInLikesAndPromotes(UlearnDb db)
		{
			var likes = await db.SolutionLikes.Include(s => s.Submission).ToListAsync();
			var i = 0;
			foreach (var like in likes)
			{
				i++;
				like.CourseId = like.Submission.CourseId;
				like.SlideId = like.Submission.SlideId;
				if (i % 1000 == 0)
					db.SaveChanges();
			}

			db.SaveChanges();

			var promotes = await db.AcceptedSolutionsPromotes.Include(s => s.Submission).ToListAsync();
			foreach (var promote in promotes)
			{
				promote.CourseId = promote.Submission.CourseId;
				promote.SlideId = promote.Submission.SlideId;
			}

			db.SaveChanges();
		}

		private static async Task SetNewFieldsInReview(UlearnDb db, IServiceProvider serviceProvider)
		{
			var reviewsIds = await db.ExerciseCodeReviews.Where(r => r.CourseId == "").Select(r => r.Id).ToListAsync();
			Console.WriteLine("Count " + reviewsIds.Count);
			var i = 0;
			foreach (var group in reviewsIds.GroupBy(r => r / 2000))
			{
				using (var scope = serviceProvider.CreateScope())
				{
					var scopedDb = (UlearnDb)scope.ServiceProvider.GetService(typeof(UlearnDb));
					var ids = group.ToList();
					i += ids.Count;
					var reviews = await scopedDb.ExerciseCodeReviews
						.Include(s => s.ExerciseChecking)
						.Include(s => s.Submission)
						.Where(s => ids.Contains(s.Id))
						.ToListAsync();
					foreach (var review in reviews)
					{
						review.CourseId = review.Submission?.CourseId ?? review.ExerciseChecking?.CourseId ?? "";
						review.SlideId = (review.Submission?.SlideId ?? review.ExerciseChecking?.SlideId) ?? default;
						review.SubmissionAuthorId = review.Submission?.UserId ?? review.ExerciseChecking?.UserId;
					}

					scopedDb.SaveChanges();
				}

				Console.WriteLine($"{i} / {reviewsIds.Count}");
			}
		}

		/*private static async Task UploadCourseVersions(IServiceProvider serviceProvider)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var db = scope.ServiceProvider.GetService<UlearnDb>();
				var versions = await db.CourseVersions.Select(v => v).OrderBy(c => c.Id).ToListAsync();
				var versionsWithFiles = (await db.CourseVersionFiles.Select(v => v.CourseVersionId).ToListAsync()).ToHashSet();
				versions = versions.Where(v => !versionsWithFiles.Contains(v.Id)).ToList();
				Console.WriteLine("Versions without file " + versions.Count);

				var courseManager = scope.ServiceProvider.GetService<ISlaveCourseManager>();
				var i = 0;
				foreach (var version in versions)
				{
					var file = courseManager.GetCourseVersionFile(version.Id);
					if (!file.Exists)
					{
						Console.WriteLine($"{version.Id} {version.CourseId} file not found");
						continue;
					}
					db.CourseVersionFiles.Add(new CourseVersionFile
					{
						CourseVersionId = version.Id,
						CourseId = version.CourseId,
						File = await ReadAllContentAsync(file)
					});
					db.SaveChanges();
					i++;
				}
				Console.WriteLine($"Uploaded {i}");
			}
		}*/

		private static async Task<byte[]> ReadAllContentAsync(FileInfo file)
		{
			byte[] result;
			using (var stream = File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				result = new byte[stream.Length];
				await stream.ReadAsync(result, 0, (int)stream.Length);
			}

			return result;
		}

		private static async Task RemoveVersionsWithoutFile(IServiceProvider serviceProvider)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var db = scope.ServiceProvider.GetService<UlearnDb>();
				var versionsWithFiles = (await db.CourseVersionFiles.Select(v => v.CourseVersionId).ToListAsync()).ToHashSet();
				var versionsWithoutFiles = (await db.CourseVersions.Where(v => !versionsWithFiles.Contains(v.Id))
					.Select(v => v.Id).ToListAsync()).ToHashSet();
				Console.WriteLine(string.Join("\n", versionsWithoutFiles));
				foreach (var wf in versionsWithoutFiles)
				{
					var cv = await db.CourseVersions.FindAsync(wf);
					db.CourseVersions.Remove(cv);
				}

				await db.SaveChangesAsync();
			}
		}

		/* private static async Task RemoveDuplicateExerciseManualCheckings(IServiceProvider serviceProvider)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var db = scope.ServiceProvider.GetService<UlearnDb>();
				var doubles = await db!.ManualExerciseCheckings
					.GroupBy(c => c.SubmissionId)
					.Select(g => new { SubmissionId = g.Key, Count = g.Count() })
					.Where(p => p.Count > 1)
					.ToListAsync();
				Console.WriteLine($"Doubles count {doubles.Count}");
				var i = 0;
				foreach (var d in doubles)
				{
					var submissionId = d.SubmissionId;
					var checkings = await db.ManualExerciseCheckings
						.Where(c => c.SubmissionId == submissionId)
						.ToListAsync();
					var bestChecking = checkings.Any(c => c.IsChecked)
						? checkings.Where(c => c.IsChecked).MaxBy(c => c.Timestamp)
						: checkings.MaxBy(c => c.Timestamp);
					var notBestCheckings = checkings.Where(c => c.Id != bestChecking.Id).ToList();
					db.ManualExerciseCheckings.RemoveRange(notBestCheckings);
					db.SaveChanges();
					i++;
					Console.WriteLine($"{i}/{doubles.Count}");
				}
			}
		}*/

		/*private static async Task UpdateManualCheckingIds(IServiceProvider serviceProvider)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var db = scope.ServiceProvider.GetService<UlearnDb>();
				var pairs = await db.ManualExerciseCheckings
					.Where(c => c.Id != c.SubmissionId)
					.Select(c => new { c.Id, c.SubmissionId })
					.OrderByDescending(s => s.Id).ToListAsync();
				Console.WriteLine($"Ids count {pairs.Count}");
				var i = 0;
				var rand = new Random();
				foreach (var pair in pairs)
				{
					try
					{
						await db.Database.ExecuteSqlRawAsync($@"UPDATE public.""ManualExerciseCheckings"" SET ""Id"" = {pair.SubmissionId} where ""Id"" = {pair.Id};");
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error on id {pair.Id} {pair.SubmissionId}");
						await db.Database.ExecuteSqlRawAsync($@"UPDATE public.""ManualExerciseCheckings"" SET ""Id"" = {10000000 + rand.Next(0, 10000000)} where ""Id"" = {pair.SubmissionId};");
					}
					i++;
					if (i % 1000 == 0)
						Console.WriteLine($"{i}/{pairs.Count}");
				}
				Console.WriteLine("All");
				db.SaveChanges();
			}
		}*/

		private static async Task RemoveTempCourse(IServiceProvider serviceProvider)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var tempCourseRemover = scope.ServiceProvider.GetService<TempCourseRemover>();
				await tempCourseRemover.RemoveTempCourseWithAllData("ulearn-testing", "71eefd72-7972-4a0a-b04e-645f7f04c9a5");
			}
		}

		private static async Task AddVersionFilesToCoursesOnDisk(IServiceProvider serviceProvider)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var coursesRepo = scope.ServiceProvider.GetService<ICoursesRepo>();
				var tempCoursesRepo = scope.ServiceProvider.GetService<ITempCoursesRepo>();
				var courseManager = scope.ServiceProvider.GetService<ISlaveCourseManager>();
				var publishedVersions = await coursesRepo.GetPublishedCourseVersions();
				var tempCourses = await tempCoursesRepo.GetAllTempCourses();
				var courseAndTokens =
					publishedVersions.Select(v => (CourseId: v.CourseId, Token: new CourseVersionToken(v.Id)))
						.Concat(tempCourses.Select(v => (CourseId: v.CourseId, Token: new CourseVersionToken(v.LoadingTime)))).ToList();
				foreach (var (courseId, token) in courseAndTokens)
				{
					var dir = courseManager.GetExtractedCourseDirectory(courseId);
					if (!dir.Exists)
						continue;
					var oldToken = CourseVersionToken.Load(dir);
					if (oldToken.Version != default(Guid))
						continue;
					await token.Save(dir);
				}
			}
		}

		private static async Task CreateNewVersionFromZip(IServiceProvider serviceProvider, string courseId, string zipPath)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var usersRepo = scope.ServiceProvider.GetService<IUsersRepo>();
				var coursesRepo = scope.ServiceProvider.GetService<ICoursesRepo>();
				var ulearnBotUser = await usersRepo.GetUlearnBotUser();
				var versionId = Guid.NewGuid();
				var content = await new FileInfo(zipPath).ReadAllContentAsync();
				var publishedCourseVersion = await coursesRepo.GetPublishedCourseVersion(courseId);
				await coursesRepo.AddCourseVersion(courseId, publishedCourseVersion.CourseName, versionId, ulearnBotUser.Id, null, null, null, null, content);
				await coursesRepo.MarkCourseVersionAsPublished(versionId);
			}
		}

		private static async Task SetCourseNamesToVersions(IServiceProvider serviceProvider, bool removeVersionIfCourseError)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var db = scope.ServiceProvider.GetService<UlearnDb>();
				var coursesRepo = scope.ServiceProvider.GetService<ICoursesRepo>();
				var courseManager = scope.ServiceProvider.GetService<ISlaveCourseManager>();
				var allCourseVersions = await db.CourseVersions.ToListAsync();
				foreach (var courseVersion in allCourseVersions)
				{
					var courseVersionFile = await coursesRepo.GetVersionFile(courseVersion.Id);
					using (var tempDirectory = await courseManager.ExtractCourseVersionToTemporaryDirectory(courseVersion.CourseId, new CourseVersionToken(courseVersion.Id), courseVersionFile.File))
					{
						var (course, exception) = courseManager.LoadCourseFromDirectory(courseVersion.CourseId, tempDirectory.DirectoryInfo);
						if (exception != null)
						{
							if (removeVersionIfCourseError)
								await coursesRepo.DeleteCourseVersion(courseVersion.CourseId, courseVersion.Id);
							Console.WriteLine($"Error on {courseVersion.CourseId} {courseVersion.Id}");
							continue;
						}

						courseVersion.CourseName = course.Title;
						await db.SaveChangesAsync();
						Console.WriteLine($"{courseVersion.CourseId} {courseVersion.Id} {courseVersion.CourseName}");
					}
				}
			}
		}

		private static async Task UnifyVisits(IServiceProvider serviceProvider)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var db = scope.ServiceProvider.GetService<UlearnDb>();
				var visitsRepo = scope.ServiceProvider.GetService<IVisitsRepo>();
				var visitsCount = db.Visits.Count();
				db.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));
				var dubles = await db.Visits
					.GroupBy(v => new { v.CourseId, v.SlideId, v.UserId })
					.Select(g => new { g.Key.CourseId, g.Key.SlideId, g.Key.UserId, Count = g.Count() })
					.Where(t => t.Count > 1)
					.ToListAsync();
				db.Database.SetCommandTimeout(TimeSpan.FromMinutes(0.5));
				Console.WriteLine($"{dubles.Count} / {visitsCount}");
				var i = 0;
				foreach (var duble in dubles)
				{
					//Console.WriteLine($"{duble.CourseId} {duble.SlideId} {duble.UserId} {duble.Count}");
					var actualVisit = await visitsRepo.FindVisit(duble.CourseId, duble.SlideId, duble.UserId);
					var allVisits = await db.Visits.Where(v => v.CourseId == duble.CourseId && v.SlideId == duble.SlideId && v.UserId == duble.UserId).ToListAsync();
					var visitsForRemove = allVisits.Where(v => v.Id != actualVisit.Id);
					db.Visits.RemoveRange(visitsForRemove);
					i++;
					if (i % 100 == 0)
					{
						await db.SaveChangesAsync();
						Console.WriteLine($"{i} / {dubles.Count}");
					}
				}

				await db.SaveChangesAsync();
			}
		}

		private static async Task BuildFavouriteReviewsFromReviews(IServiceProvider serviceProvider)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var db = scope.ServiceProvider.GetService<UlearnDb>();
				var courseRolesRepo = scope.ServiceProvider.GetService<ICourseRolesRepo>();
				var courseStorage = scope.ServiceProvider.GetService<ICourseStorage>();
				var courses = courseStorage.GetCourses().ToList();

				var updateDbCounter = 0;
				var updateDbMaxCount = 1000;
				var courseCounter = 0;

				foreach (var course in courses)
				{
					//using (var transaction = db.Database.BeginTransaction())
					//{
						courseCounter++;
						Console.WriteLine($@"BuildFavouriteReviews: checking course {course.Id}, its {courseCounter} out of {courses.Count} ");
						var slides = course.GetSlidesNotSafe().OfType<ExerciseSlide>().ToList();
						var instructorIds = await courseRolesRepo.GetListOfUsersWithCourseRole(CourseRoleType.Instructor, course.Id, false);
						var slideCounter = 0;
						foreach (var slide in slides)
						{
							slideCounter++;
							Console.WriteLine($@"BuildFavouriteReviews: checking slide {slide.Id}, its {slideCounter} out of {slides.Count} ");

							var textToFavoriteReview = new Dictionary<string, FavouriteReview>();
							foreach (var instructorId in instructorIds)
							{
								var slideTopReviewsForInstructor = db.ExerciseCodeReviews
									.Where(r => r.CourseId == course.Id && r.SlideId == slide.Id && r.AuthorId == instructorId && !r.HiddenFromTopComments && !r.IsDeleted)
									.ToList();

								if (!slideTopReviewsForInstructor.Any())
									continue;

								var userTopReviews = slideTopReviewsForInstructor
									.GroupBy(r => r.Comment)
									.OrderByDescending(g => g.Count())
									.ThenByDescending(g => g.Max(r => r.AddingTime))
									.Take(5)
									.Select(g => g.Key)
									.ToList();

								foreach (var review in userTopReviews)
								{
									if (!textToFavoriteReview.TryGetValue(review, out var favouriteReview))
									{
										favouriteReview = new FavouriteReview { CourseId = course.Id, SlideId = slide.Id, Text = review, };
										textToFavoriteReview[review] = favouriteReview;
										db.FavouriteReviews.Add(favouriteReview);
										updateDbCounter++;
									}

									var favouriteReviewByUser = new FavouriteReviewByUser
									{
										CourseId = course.Id,
										SlideId = slide.Id,
										UserId = instructorId,
										Timestamp = DateTime.Now,
										FavouriteReview = favouriteReview,
									};
									db.FavouriteReviewsByUsers.Add(favouriteReviewByUser);
									updateDbCounter++;

									if (updateDbCounter >= updateDbMaxCount)
									{
										await db.SaveChangesAsync();
										updateDbCounter = 0;
									}
								}
							}
						}

						await db.SaveChangesAsync();
						//transaction.Commit();
					//}
				}
			}
		}

		private static async Task FixCheckedManualCheckingsWithoutScoreAndPercent(IServiceProvider serviceProvider)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var db = scope.ServiceProvider.GetService<UlearnDb>();
				var slideCheckingsRepo = scope.ServiceProvider.GetService<ISlideCheckingsRepo>();

				using (var transaction = db.Database.BeginTransaction())
				{
					var checkings = await db.ManualExerciseCheckings
						.Where(c => c.IsChecked && c.Score == null && c.Percent == null)
						.ToListAsync();
					Console.WriteLine($"{checkings.Count}");

					foreach (var checking in checkings)
					{
						checking.IsChecked = false;
					}

					await db.SaveChangesAsync();

					foreach (var checking in checkings)
					{
						var lastChecking = await db.ManualExerciseCheckings
							.Where(c => c.UserId == checking.UserId && c.CourseId == checking.CourseId && c.SlideId == checking.SlideId)
							.OrderByDescending(c => c.Submission.Timestamp)
							.FirstOrDefaultAsync();
						await slideCheckingsRepo.MarkManualExerciseCheckingAsCheckedBeforeThis(lastChecking);
					}

					await transaction.CommitAsync();
				}
			}
		}

		public record SlideAndBorders(string CourseId, string SlideId, int ScoreWithCodeReview, int PassedTestsScore);

		private static async Task SetPercentByScore(IServiceProvider serviceProvider)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var db = scope.ServiceProvider.GetService<UlearnDb>();
				var courseStorage = scope.ServiceProvider.GetService<ICourseStorage>();

				var tasksWithScore = (await db.ManualExerciseCheckings
					.Where(c => c.IsChecked && c.Score != null && c.Percent == null)
					.Select(c => new { c.CourseId, c.SlideId, c.UserId })
					.Distinct()
					.ToListAsync())
					.OrderBy(c => c.CourseId)
					.ThenBy(c => c.SlideId)
					.ThenBy(c => c.UserId)
					.ToList();
				Console.WriteLine($"tasks to process count {tasksWithScore.Count}");

				var updateDbCounter = 0;
				var updateDbMaxCount = 1000;
				var tasksCount = 0;

				foreach (var triple in tasksWithScore)
				{
					var courseId = triple.CourseId;
					var slideId = triple.SlideId;
					var userId = triple.UserId;

					var others = new List<SlideAndBorders>
					{
						new SlideAndBorders("BasicProgramming", "617d8f67-491f-4172-8ed4-e7c230c2120f", 25, 5),
						new SlideAndBorders("BasicProgramming", "3de364ff-38e9-411e-ac97-b4e4d24cdf26", 50, 10),
						new SlideAndBorders("BasicProgramming", "616826fa-d344-4292-9ab4-ec5c8ea83e1e", 50, 10),
						new SlideAndBorders("BasicProgramming", "d91838bd-0b73-4da5-81ec-39ff935ced35", 50, 10),
						new SlideAndBorders("BasicProgramming", "8e0ccef8-59a4-4bf1-8610-8351c487a339", 50, 10),
						new SlideAndBorders("BasicProgramming2", "10e4d5e2-6f7e-48c8-8d09-db133cdf0271", 50, 10),
						new SlideAndBorders("BasicProgramming2", "61d2af1e-c7d8-468f-82e0-e69170998423", 50, 10),
						new SlideAndBorders("BasicProgramming2", "30c04e83-f962-412e-8866-0ab981e5a71c", 100, 20),
						new SlideAndBorders("BasicProgramming2", "43d5bcce-7071-4111-87fa-b5887053a6da", 50, 10),
						new SlideAndBorders("BasicProgramming2", "85d068c7-ab40-4c9f-912a-c1405f5da5a4", 50, 10),
						new SlideAndBorders("izhtestingcasting", "5906398c-9b9a-4119-ad2a-c54e8b50cec2", 4, 0),
						new SlideAndBorders("JavaTech", "36f0bbb6-bff5-4049-8dc0-7b7c774a5a6f", 10, 0),
						new SlideAndBorders("JavaTech", "22775cad-22e5-4034-83ba-8f9f9f2e3195", 5, 0),
						new SlideAndBorders("JavaTech", "ba0de434-d87d-4c3c-9610-005a1e6aa404", 10, 0),
						new SlideAndBorders("TestingCasting", "1cc77436-e31f-4e25-a52b-6805f04117e5", 3, 0),
						new SlideAndBorders("TestingCasting", "5906398c-9b9a-4119-ad2a-c54e8b50cec2", 8, 0),
					};

					var scoreWithCodeReview = 0;
					var passedTestsScore = 0;
					var found = false;
					foreach (var other in others)
					{
						if (string.Equals(courseId, other.CourseId, StringComparison.OrdinalIgnoreCase) && slideId == new Guid(other.SlideId))
						{
							scoreWithCodeReview = other.ScoreWithCodeReview;
							passedTestsScore = other.PassedTestsScore;
							found = true;
						}
					}
					if (!found)
					{
						var course = courseStorage.GetCourse(courseId);
						if (course == null)
							continue;

						var slide = course.FindSlideByIdNotSafe(slideId) as ExerciseSlide;
						if (slide == null)
							continue;

						scoreWithCodeReview = slide.Scoring.ScoreWithCodeReview;
						passedTestsScore = slide.Scoring.PassedTestsScore;
					}

					var checkings = await db.ManualExerciseCheckings
						.Where(c => c.CourseId == courseId && c.SlideId == slideId && c.UserId == userId && c.IsChecked && (c.Score != null || c.Percent != null))
						.OrderBy(c => c.Submission.Timestamp)
						.ToListAsync();

					int ScoreToPercent(int s) => Math.Min(100, (int)Math.Ceiling(s == 0 ? 0 : s * 100m / scoreWithCodeReview));
					var maxScore = 0;
					var automaticScore = passedTestsScore;
					foreach (var checking in checkings)
					{
						if (checking.Score != null)
						{
							maxScore = Math.Max(maxScore, checking.Score.Value);
							var finalScoreForNowWithAutomatic = automaticScore + maxScore;
							var percent = ScoreToPercent(finalScoreForNowWithAutomatic);
							checking.Percent = percent;
							updateDbCounter++;
						}
						else if (checking.Percent != null)
						{
							// До этого были проверки со score, потому что иначе для этой задачи и пользователя бы не запрашивали
							var finalScoreFromScoreWithAutomatic = automaticScore + maxScore;
							var percentFromScore = ScoreToPercent(finalScoreFromScoreWithAutomatic);
							if (percentFromScore > checking.Percent)
							{
								checking.Percent = percentFromScore;
								updateDbCounter++;
							}
						}
					}

					if (updateDbCounter > updateDbMaxCount)
					{
						await db.SaveChangesAsync();
						updateDbCounter = 0;
					}

					tasksCount++;
					if (tasksCount % 100 == 0)
						Console.WriteLine($"{tasksCount}/{tasksWithScore.Count}");
				}
				await db.SaveChangesAsync();
			}
		}
	}
}