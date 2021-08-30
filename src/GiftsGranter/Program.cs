using System;
using System.Collections.Generic;
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
		private static ILog log => LogProvider.Get().ForContext(typeof(Program));
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
			var hostLog = settings["hostLog"].ToObject<HostLogConfiguration>();
			var graphiteServiceName = settings["graphiteServiceName"].Value<string>();
			LoggerSetup.Setup(hostLog, graphiteServiceName);
			try
			{
				var staff = new StaffClient(settings["staff"]["clientAuth"].Value<string>());
				if (args.Contains("-r"))
				{
					Console.WriteLine("Username (example: KONTUR\\pe):");
					var username = Console.ReadLine();
					Console.WriteLine($"Password for {username}:");
					var password = GetConsolePassword();
					var refreshToken = staff.GetRefreshToken(username, password);
					Console.WriteLine($"RefreshToken: {refreshToken}");
					return;
				}

				var telegramBot = new GiftsTelegramBot();
				try
				{
					staff.UseRefreshToken(settings["staff"]["refreshToken"].Value<string>());
					var maxGiftsPerRun = args.Length > 0 ? int.Parse(args[0]) : settings["maxGiftsPerRun"].Value<int>();
					log.Info("UseGiftGrantsLimitPerRun\t" + maxGiftsPerRun);
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
					log.Error(e, "UnhandledException");
				}
			}
			finally
			{
				FileLog.FlushAll();
			}
		}

		private void GrantGiftsForCourse(string courseId)
		{
			log.Info($"StartProcessingCourse\t{courseId}");
			GrantGifts(courseId);
			log.Info($"DoneCourse\t{courseId}");
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
			log.Info($"TotalCompleted\t{rating.Count}");
			log.Info($"KonturCompleted\t{konturRating.Count}");
			log.Info($"StabilizedKonturCompleted\t{stabilizedKonturCompleted.Count}");
			EnsureHaveGifts(stabilizedKonturCompleted, courseSettings, courseId);
		}

		private void EnsureHaveGifts(List<RatingEntry> entries, CourseSettings courseSettings, string courseId)
		{
			var delayMs = settings["delayBetweenStaffRequests"].Value<int>();
			var granted = 0;
			foreach (var ratingEntry in entries)
			{
				if (granted >= maxGiftsPerRun)
				{
					log.Info($"GiftGrantsLimitPerRunExceeded\t{maxGiftsPerRun}");
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
					log.Info($"NoGiftYet\t{entry.Score}\t{entry.User.VisibleName}");
					staffClient.GrantGift(staffUserId, entry.Score, courseSettings);
					log.Info($"ComplexityGiftGrantedFor\t{entry.User.VisibleName}\t{entry.User.KonturLogin}");
					telegramBot.PostToChannel($"Granted gift for course {courseId}\n{entry.Score} points for user {entry.User.VisibleName} {entry.User.KonturLogin}");
					return true;
				}

				return false;
			}
			catch (Exception e)
			{
				var message = $"Can't grant gift to {entry.User.VisibleName} (kontur-login: {entry.User.KonturLogin} ulearn-username: {entry.User.UserName})";
				log.Error(e, message);
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