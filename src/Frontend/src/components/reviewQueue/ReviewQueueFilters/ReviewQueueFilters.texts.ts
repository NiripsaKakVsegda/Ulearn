import { DateSort } from "../../../models/instructor";
import { Grouping, HistoryTimeSpan } from "../RevoewQueue.types";

export default {
	timeSpanValues: {
		[HistoryTimeSpan.Day]: "За последние 24 часа",
		[HistoryTimeSpan.Week]: "За неделю",
		[HistoryTimeSpan.Month]: "За месяц",
		[HistoryTimeSpan.All]: "За всё время",
	} as Record<HistoryTimeSpan, string>,

	grouping: 'Группировка:',
	groupingValues: {
		[Grouping.NoGrouping]: "Без группировки",
		[Grouping.GroupExercises]: "По задаче",
		[Grouping.GroupStudents]: "По студенту"
	} as Record<Grouping, string>,

	sort: 'Сортировка:',
	sortValues: {
		[DateSort.Ascending]: "От старых к новым",
		[DateSort.Descending]: "От новых к старым"
	} as Record<DateSort, string>,

	showCommentsToggleText: 'Показать комментарии',

	filtersButton: "Фильтры",
};
