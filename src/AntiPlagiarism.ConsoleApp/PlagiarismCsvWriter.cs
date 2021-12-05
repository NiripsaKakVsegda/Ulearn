using System.Collections.Generic;
using System.Globalization;
using System.IO;
using AntiPlagiarism.ConsoleApp.Models;
using CsvHelper;
using Ulearn.Common.Extensions;
using Vostok.Logging.Abstractions;

namespace AntiPlagiarism.ConsoleApp
{
	public class PlagiarismCsvWriter
	{
		private readonly string csvFile;
		
		public PlagiarismCsvWriter(string path)
		{
			csvFile = path.PathCombine("plagiarisms.csv");
		}

		public void WritePlagiarism(List<PlagiarismInfo> plagiarisms)
		{
			using var stream = new StreamWriter(csvFile);
			using var csv = new CsvWriter(stream, CultureInfo.InvariantCulture);
			csv.WriteRecords(plagiarisms);
			
			ConsoleWorker.WriteLine($"Информация о плагиате записана в файл {csvFile}");
		}
	}
}