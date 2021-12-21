namespace AntiPlagiarism.ConsoleApp.Models.CsvPlagiarismInfo
{
	public class BriefPlagiarismInfo : IPlagiarismInfo
	{
		[CsvHelper.Configuration.Attributes.Name("Автор решений")]
		public string AuthorName { get; set; }
		
		[CsvHelper.Configuration.Attributes.Name("Количество подозрительных решений")]
		public int TotalSuspicionCount { get; set; }
		
		[CsvHelper.Configuration.Attributes.Name("Из них с сильным уровнем сходства")]
		public int StrongSuspicionCount { get; set; }
	}
}