using Ulearn.Common;

namespace AntiPlagiarism.ConsoleApp.Models
{
	public class PlagiarismInfo
	{
		[CsvHelper.Configuration.Attributes.Name("Автор решения")]
		public string AuthorName { get; set; }
		
		[CsvHelper.Configuration.Attributes.Name("Название задачи")]
		public string TaskTitle { get; set; }
		
		[CsvHelper.Configuration.Attributes.Name("Автор с наиболее похожим решением")]
		public string PlagiarismAuthorName { get; set; }
		
		[CsvHelper.Configuration.Attributes.Name("Уровень схожести")]
		public string SuspicionLevel { get; set; }
		
		[CsvHelper.Configuration.Attributes.Name("Процент схожести решений")]
		public string Weight { get; set; }
		
		[CsvHelper.Configuration.Attributes.Name("Язык решения")]
		public Language Language { get; set; }
		
		[CsvHelper.Configuration.Attributes.Name("Количество списываний студента")]
		public int PlagiarismCount { get; set; }
		
		[CsvHelper.Configuration.Attributes.Ignore]
		public string Code { get; set; }
		
		[CsvHelper.Configuration.Attributes.Ignore]
		public string PlagiarismCode { get; set; }
	}
}