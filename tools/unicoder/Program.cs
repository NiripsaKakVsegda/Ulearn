using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ude;
using Ude.Core;

namespace unicoder
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Usage: unicoder <path> <file-pattern>");
			var files = Directory.GetFiles(args[0], args[1], SearchOption.AllDirectories);
			foreach (var file in files)
			{
				ConvertToUtf8WithBOM(file);
			}
		}

		private static void ConvertToUtf8WithBOM(string file)
		{
			var bytes = File.ReadAllBytes(file);
			var asciiOnly = bytes.All(c => c <= 127);
			if (asciiOnly) return;
			var preamble = Encoding.UTF8.GetPreamble();
			var isUtf8WithBom = bytes.Take(preamble.Length).SequenceEqual(preamble);
			if (isUtf8WithBom) return;

			var dd = new UTF8Prober();
			var utf8DetectionResult = dd.HandleData(bytes, 0, bytes.Length);
			
			var encoding = Encoding.UTF8;
			if (utf8DetectionResult == ProbingState.NotMe) encoding = Encoding.GetEncoding(1251);
			else return;
			Console.WriteLine("Converting {0}. {1}", file, encoding.EncodingName);
			var content = File.ReadAllText(file, encoding);
			var firstNonAscii = content.Zip(Enumerable.Range(0, int.MaxValue), Tuple.Create)
				.FirstOrDefault(t => t.Item1 > 127);
			if (firstNonAscii != null)
			{
				var index = Math.Max(0, firstNonAscii.Item2 - 5);
				var len = Math.Min(content.Length - index, 35);
				Console.WriteLine("  non ascii text {0}", content.Substring(index, len).Replace('\r', ' ').Replace('\n', ' '));
			}
			File.WriteAllText(file, content, Encoding.UTF8);
		}
	}
}
