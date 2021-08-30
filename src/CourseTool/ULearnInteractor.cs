using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Ionic.Zip;
using Ulearn.Common;

namespace uLearn.CourseTool
{
	internal static class ULearnInteractor
	{
		private const string rvTokenSignal = "__RequestVerificationToken";
		private static readonly Regex getInputs = new Regex("<input .*?>");
		private const string testRvToken = "name=\"" + rvTokenSignal + "\"";
		private static readonly Regex getValue = new Regex("value=\"(.*?)\"");

		private static CookieAwareWebClient Login(string ulearnBaseUrlWeb, string login, string password)
		{
			var signinUrl = ulearnBaseUrlWeb + "/Login";
			var loginUrl = ulearnBaseUrlWeb + "/Login";
			var client = new CookieAwareWebClient();

			var rvToken = GetRequestVerificationToken(client, signinUrl);
			client.UploadValues(loginUrl, new NameValueCollection
			{
				{ "UserName", login },
				{ "Password", password },
				{ "RememberMe", "false" },
				{ rvTokenSignal, rvToken }
			});

			return client;
		}

		private static string GetRequestVerificationToken(WebClient client, string url)
		{
			var formPage = Encoding.UTF8.GetString(client.DownloadData(url));

			var matches = getInputs.Matches(formPage);
			var match = matches.Cast<Match>().FirstOrDefault(m => m.Value.Contains(testRvToken));
			return match == null ? null : getValue.Match(match.Value).Groups[1].Value;
		}

		public static void Download(string baseDir, bool force, Config config, string ulearnBaseUrlWeb, Credentials credentials)
		{
			var fileName = config.ULearnCourseId + ".zip";
			var fileFullName = Path.Combine(baseDir, fileName);
			var downloadUrl = string.Format("{0}/Unit/DownloadPackage?courseId={1}", ulearnBaseUrlWeb, HttpUtility.UrlEncode(config.ULearnCourseId));

			var client = Login(ulearnBaseUrlWeb, credentials.Email, credentials.GetPassword());
			client.TryDownloadFile(downloadUrl, fileFullName);
			Console.Out.WriteLine("Package downloaded to {0}", fileFullName);

			var dir = new DirectoryInfo(baseDir);
			using (var zip = ZipFile.Read(fileFullName, new ReadOptions { Encoding = ZipUtils.Cp866 }))
			{
				var courseDir = dir.CreateSubdirectory(config.ULearnCourseId);
				Directory.Delete(courseDir.FullName, force);
				courseDir.Create();
				zip.ExtractAll(courseDir.FullName, ExtractExistingFileAction.OverwriteSilently);
			}

			new FileInfo(fileFullName).Delete();
		}

		public static void Upload(string baseDir, Config config, string ulearnBaseUrlWeb, Credentials credentials)
		{
			var fileFullName = Path.Combine(baseDir, config.ULearnCourseId + ".zip");
			var uploadUrl = ulearnBaseUrlWeb + "/Unit/UploadCourse?courseId=" + config.ULearnCourseId;
			var courseDir = Path.Combine(baseDir, config.ULearnCourseId);

			if (File.Exists(fileFullName))
				File.Delete(fileFullName);
			using (var zip = new ZipFile(fileFullName, Encoding.UTF8))
			{
				zip.AddDirectory(courseDir);
				zip.Save();
			}

			var client = Login(ulearnBaseUrlWeb, credentials.Email, credentials.GetPassword());
			client.UploadFile(uploadUrl, fileFullName);
		}
	}
}