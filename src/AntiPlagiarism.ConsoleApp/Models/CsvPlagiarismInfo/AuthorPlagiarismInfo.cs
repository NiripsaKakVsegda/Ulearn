using AntiPlagiarism.ConsoleApp.Models.CsvPlagiarismInfo;
using Ulearn.Common;

namespace AntiPlagiarism.ConsoleApp.Models
{
	public class AuthorPlagiarismInfo : IPlagiarismInfo
	{
		[CsvHelper.Configuration.Attributes.Name("Автор решения")]
		public string AuthorName { get; set; }
		
		[CsvHelper.Configuration.Attributes.Name("Название задачи")]
		public string TaskTitle { get; set; }
		
		[CsvHelper.Configuration.Attributes.Name("Автор с наиболее похожим решением")]
		public string PlagiarismAuthorName { get; set; }
		
		[CsvHelper.Configuration.Attributes.Name("Уровень подозрительности")]
		public string SuspicionLevel { get; set; }
		
		[CsvHelper.Configuration.Attributes.Name("Степень схожести решений")]
		public string Weight { get; set; }
		
		[CsvHelper.Configuration.Attributes.Name("Язык решения")]
		public Language Language { get; set; }
	}
}