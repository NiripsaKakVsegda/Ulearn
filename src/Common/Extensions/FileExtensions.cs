using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ulearn.Common.Helpers;

namespace Ulearn.Common.Extensions
{
	public static class FileExtensions
	{
		public static void RemoveXmlDeclaration(this FileInfo file)
		{
			var text = File.ReadAllText(file.FullName);
			const string xmlDeclaration = @"<?xml version=""1.0"" encoding=""utf-8""?>";
			var startIndex = text.StartsWith(xmlDeclaration) ? xmlDeclaration.Length : 0;
			while (startIndex < text.Length && char.IsWhiteSpace(text[startIndex]))
				startIndex++;
			File.WriteAllText(file.FullName, text.Substring(startIndex));
		}

		public static string ContentAsUtf8(this FileInfo file)
		{
			return File.ReadAllText(file.FullName, Encoding.UTF8);
		}

		public static byte[] ReadAllContent(this FileInfo file)
		{
			return File.ReadAllBytes(file.FullName);
		}

		public static async Task<byte[]> ReadAllContentAsync(this FileInfo file)
		{
			byte[] result;
			using (var stream = File.Open(file.FullName, FileMode.Open))
			{
				result = new byte[stream.Length];
				await stream.ReadAsync(result, 0, (int)stream.Length);
			}

			return result;
		}

		public static T DeserializeJson<T>(this string content)
		{
			return JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			});
		}

		public static string GetContent(this DirectoryInfo di, string filepath)
		{
			return di.GetBytes(filepath).AsUtf8();
		}

		public static byte[] GetBytes(this DirectoryInfo di, string filepath)
		{
			var fileInfo = di.GetFile(filepath);
			if (fileInfo.Exists)
				return File.ReadAllBytes(fileInfo.FullName);
			throw new Exception("Can't find file " + filepath + " in " + di.FullName);
		}

		public static bool IsInDirectory(this FileSystemInfo fileOrDirectory, DirectoryInfo directory)
		{
			return fileOrDirectory.FullName.ToLower().StartsWith(directory.FullName.ToLower());
		}

		public static IEnumerable<FileInfo> GetFilesByMask(this DirectoryInfo directory, string mask)
		{
			if (mask.Contains(".."))
				throw new ArgumentException($"{nameof(mask)} can not contain \"..\"", nameof(mask));

			/* Replace slashes / to backslashes \ on Windows */
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				mask = mask.Replace('/', '\\');

			return GlobSearcher.Glob(Path.Combine(directory.FullName, mask)).Select(path => new FileInfo(path));
		}
	}
}