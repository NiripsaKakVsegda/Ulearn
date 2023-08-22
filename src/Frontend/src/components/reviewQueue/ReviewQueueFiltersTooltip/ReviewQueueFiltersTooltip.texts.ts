import { Grouping, HistoryTimeSpan } from "../RevoewQueue.types";
import { DateSort } from "../../../models/instructor";

const timeSpanPrefix = 'Работы проверенные за ';
export default {
	timeSpanValues: {
		[HistoryTimeSpan.Day]: timeSpanPrefix + "последние 24 часа",
		[HistoryTimeSpan.Week]: timeSpanPrefix + "неделю",
		[HistoryTimeSpan.Month]: timeSpanPrefix + "месяц",
		[HistoryTimeSpan.All]: timeSpanPrefix + "всё время",
	} as Record<HistoryTimeSpan, string>,

	keys: {
		grouping: "Группировка:",
		sort: "Сортировка:",
		unit: "Модуль:",
		slide: "Слайд:",
		students: "Студенты:",
		groups: "Группы:",
		student: "Студент:",
	},

	values: {
		grouping: {
			[Grouping.NoGrouping]: "без группировки",
			[Grouping.GroupExercises]: "по задаче",
			[Grouping.GroupStudents]: "по студенту"
		} as Record<Grouping, string>,

		sort: {
			[DateSort.Ascending]: "от старых к новым",
			[DateSort.Descending]: "от новых к старым"
		} as Record<DateSort, string>,

		allUnits: "все модули",
		allSlides: "все слайды",

		allStudents: 'все',
		noStudentsSelected: 'Не выбрано ни одного студента',

		myGroups: 'мои группы',
		noGroupsSelected: 'Не выбрано ни одной группы',
	},

	groupingHint: {
		[Grouping.GroupExercises]: "Задания отсортированы в порядке появления в курсе",
		[Grouping.GroupStudents]: "Студенты остортированы в алфавитном порядке"
	},
};
