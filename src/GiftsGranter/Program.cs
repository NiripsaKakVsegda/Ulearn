using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Database.DataContexts;
using Vostok.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types.Enums;
using Ulearn.Core.Configuration;
using Ulearn.Core.Logging;
using Vostok.Logging.File;

namespace GiftsGranter
{
	internal class Program
	{
		private static ILog Log => LogProvider.Get().ForContext(typeof(Program));
		private readonly int maxGiftsPerRun;
		private readonly VisitsRepo repo;
		private readonly JObject settings;
		private readonly StaffClient staffClient;
		private readonly GiftsTelegramBot telegramBot;

		public Program(VisitsRepo repo, StaffClient staffClient, int maxGiftsPerRun, JObject settings, GiftsTelegramBot telegramBot)
		{
			this.repo = repo;
			this.staffClient = staffClient;
			this.maxGiftsPerRun = maxGiftsPerRun;
			this.settings = settings;
			this.telegramBot = telegramBot;
		}

		private static string GetConsolePassword()
		{
			var password = new StringBuilder();
			while (true)
			{
				var keyInfo = Console.ReadKey(true);
				if (keyInfo.Key == ConsoleKey.Enter)
				{
					Console.WriteLine();
					break;
				}

				if (keyInfo.Key == ConsoleKey.Backspace)
				{
					if (password.Length > 0)
					{
						Console.Write("\b\0\b");
						password.Length--;
					}

					continue;
				}

				if (!char.IsControl(keyInfo.KeyChar))
				{
					Console.Write('*');
					password.Append(keyInfo.KeyChar);
				}
			}

			return password.ToString();
		}

		private static void Main(string[] args)
		{
			var settings = JObject.Parse(File.ReadAllText("appsettings.json"));
			var staff = new StaffClient(settings["staff"]["clientAuth"].Value<string>());
			var hostLog = settings["hostLog"].ToObject<HostLogConfiguration>();
			var graphiteServiceName = settings["graphiteServiceName"].Value<string>();
			LoggerSetup.Setup(hostLog, graphiteServiceName);
			try
			{
				CreateCliCommands(settings, staff).Invoke(args);
			}
			finally
			{
				FileLog.FlushAll();
			}
		}

		private static RootCommand CreateCliCommands(JObject settings, StaffClient staff)
		{
			var root = new RootCommand();
			root.AddOption(new Option<int?>("-c", () => settings["maxGiftsPerRun"]!.Value<int>(), "Max gifts to grant per run per course"));
			root.Handler = CommandHandler.Create<int>(c => GrantGiftsCommand(settings, staff, c));

			var updateRefreshToken = new Command("update-staff-refresh-token");
			updateRefreshToken.AddAlias("r");
			updateRefreshToken.Handler = CommandHandler.Create(() => UpdateRefreshTokenCommand(settings, staff));
			root.AddCommand(updateRefreshToken);

			var testStaff = new Command("test-staff");
			testStaff.AddOption(new Option<int>("--userId"));
			testStaff.AddAlias("t");
			testStaff.Handler = CommandHandler.Create<int>((userId) => TestStaffCommand(settings, staff, userId));
			root.AddCommand(testStaff);

			return root;
		}

		private static void GrantGiftsCommand(JObject settings, StaffClient staff, int maxGiftsPerRun)
		{
			var telegramBot = new GiftsTelegramBot();
			try
			{
				Log.Info("UseGiftGrantsLimitPerRun\t" + maxGiftsPerRun);
				var db = new ULearnDb();
				var repo = new VisitsRepo(db);
				var courses = settings["courses"].Values<string>();
				var program = new Program(repo, staff, maxGiftsPerRun, settings, telegramBot);
				foreach (var courseId in courses)
					program.GrantGiftsForCourse(courseId);
			}
			catch (Exception e)
			{
				telegramBot.PostToChannel($"Error while grant staff gifts.\n\n{e}");
				Log.Error(e, "UnhandledException");
			}
		}

		private static void TestStaffCommand(JObject settings, StaffClient staff, int userId)
		{
			staff.UseRefreshToken(settings["staff"]["refreshToken"].Value<string>());
			if (userId == 0)
			{
				var peUser = staff.GetUser("S-1-5-21-1231152155-1323711836-1525454979-1552");
				Console.WriteLine("GetUser is OK");
				userId = peUser["id"].Value<int>();
			}

			var userGifts = staff.GetUserGifts(userId)["gifts"].Children();
			Console.WriteLine($"GetUserGifts is OK. Gifts count: {userGifts.Count()}");
		}

		private static void UpdateRefreshTokenCommand(JObject settings, StaffClient staff)
		{
			Console.WriteLine("Username (example: KONTUR\\pe):");
			var username = Console.ReadLine();
			Console.WriteLine($"Password for {username}:");
			var password = GetConsolePassword();
			var refreshToken = staff.GetRefreshToken(username, password);
			Console.WriteLine($"RefreshToken: {refreshToken}");
		}

		private void GrantGiftsForCourse(string courseId)
		{
			Log.Info($"StartProcessingCourse\t{courseId}");
			GrantGifts(courseId);
			Log.Info($"DoneCourse\t{courseId}");
		}

		private void GrantGifts(string courseId)
		{
			var courseSettings = settings[courseId].ToObject<CourseSettings>();
			var passScore = courseSettings.passScore;
			var requiredSlides = courseSettings.requiredSlides;
			var rating = repo.GetCourseRating(courseId, passScore, requiredSlides);
			var konturRating = rating
				.Where(e => e.User.Logins.Any(login => login.LoginProvider == "Контур.Паспорт"))
				.ToList();
			var stabilizedKonturCompleted = konturRating.Where(e => e.LastVisitTime < DateTime.Now - TimeSpan.FromDays(1)).ToList();
			Log.Info($"TotalCompleted\t{rating.Count}");
			Log.Info($"KonturCompleted\t{konturRating.Count}");
			Log.Info($"StabilizedKonturCompleted\t{stabilizedKonturCompleted.Count}");
			EnsureHaveGifts(stabilizedKonturCompleted, courseSettings, courseId);
		}

		private void EnsureHaveGifts(List<RatingEntry> entries, CourseSettings courseSettings, string courseId)
		{
			var delayMs = settings["delayBetweenStaffRequests"].Value<int>();
			var granted = 0;
			foreach (var ratingEntry in entries.OrderByDescending(e => e.LastVisitTime))
			{
				if (granted >= maxGiftsPerRun)
				{
					Log.Info($"GiftGrantsLimitPerRunExceeded\t{maxGiftsPerRun}");
					return;
				}

				if (GrantGiftsIfNone(ratingEntry, courseSettings, courseId))
					granted++;
				Thread.Sleep(delayMs);
			}
		}

		private bool GrantGiftsIfNone(RatingEntry entry, CourseSettings courseSettings, string courseId)
		{
			try
			{
				var sid = entry.User.Logins.First(login => login.LoginProvider == "Контур.Паспорт").ProviderKey;
				var staffUserId = GetUserId(sid);
				var gifts = staffClient.GetUserGifts(staffUserId);
				var giftImagePath = courseSettings.giftImagePath;

				var hasComplexityGift = gifts["gifts"].Children().Any(gift => gift["imagePath"].Value<string>() == giftImagePath);
				if (!hasComplexityGift)
				{
					Log.Info($"NoGiftYet\t{entry.Score}\t{entry.User.VisibleName}");
					staffClient.GrantGift(staffUserId, entry.Score, courseSettings);
					Log.Info($"ComplexityGiftGrantedFor\t{entry.User.VisibleName}\t{entry.User.KonturLogin}");
					telegramBot.PostToChannel($"Granted gift for course {courseId}\n{entry.Score} points for user {entry.User.VisibleName} {entry.User.KonturLogin}");
					return true;
				}

				return false;
			}
			catch (Exception e)
			{
				var message = $"Can't grant gift to {entry.User.VisibleName} (kontur-login: {entry.User.KonturLogin} ulearn-username: {entry.User.UserName})";
				Log.Error(e, message);
				telegramBot.PostToChannel(message);
				telegramBot.PostToChannel($"```{e}```", ParseMode.Markdown);
				return false;
			}
		}

		private int GetUserId(string sid)
		{
			// TODO Use https://staff.skbkontur.ru/api/users/getbylogin?login=kontur\<konturlogin> if not succeeded
			return staffClient.GetUser(sid)["id"]!.Value<int>();
		}
	}
}