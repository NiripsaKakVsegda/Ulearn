import { Grouping, HistoryTimeSpan } from "../RevoewQueue.types";
import { DateSort } from "../../../models/instructor";

export default {
	timeSpanValues: {
		[HistoryTimeSpan.Day]: "За последние 24 часа",
		[HistoryTimeSpan.Week]: "За неделю",
		[HistoryTimeSpan.Month]: "За месяц",
		[HistoryTimeSpan.All]: "За всё время",
	} as Record<HistoryTimeSpan, string>,

	groupingValues: {
		[Grouping.NoGrouping]: "Без группировки",
		[Grouping.GroupExercises]: "По задаче",
		[Grouping.GroupStudents]: "По студенту"
	} as Record<Grouping, string>,

	sortValues: {
		[DateSort.Ascending]: "От старых к новым",
		[DateSort.Descending]: "От новых к старым"
	} as Record<DateSort, string>,

	showCommentsToggleText: 'Показать комментарии',

	filtersButton: "Фильтры",

	filtersTooltip: {
		grouping: "Группировка:",
		sort: "Сортировка:",
		unit: "Модуль:",
		slide: "Слайд:",
		students: "Студенты:",
		groups: "Группы:",

		allUnits: "все модули",
		allSlides: "все слайды",
		allStudents: 'все',
		myGroups: 'мои группы',
		groupingHint: {
			[Grouping.GroupExercises]: "Задания отсортированы в порядке появления в курсе",
			[Grouping.GroupStudents]: "Студенты остортированы в алфавитном порядке"
		},
		noStudentsSelected: 'Не выбрано ни одного студента',
		noGroupsSelected: 'Не выбрано ни одной группы',
	}
};
