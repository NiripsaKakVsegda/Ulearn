using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
	public class SingleGroup : GroupBase
	{
		[Required]
		public override GroupType GroupType => GroupType.SingleGroup;
		
		public int? SuperGroupId { get; set; }
		
		[Required]
		/* Если в курсе выключена ручная проверка, то можно включить её для этой группы */
		public bool IsManualCheckingEnabled { get; set; }

		[Required]
		/* Если опция выключена, то старые решения не будут отправлены на код-ревью в момент вступления в группу */
		public bool IsManualCheckingEnabledForOldSolutions { get; set; }

		[Required]
		/* Могут ли студенты этой группы видеть сводную таблицу прогресса по курсу всех студентов группы */
		public bool CanUsersSeeGroupProgress { get; set; }

		[Required]
		/* Значение по умолчанию для галочки «Не принимать больше код-ревью у этого студента по этой задаче» */
		public bool DefaultProhibitFutherReview { get; set; }
	}
}