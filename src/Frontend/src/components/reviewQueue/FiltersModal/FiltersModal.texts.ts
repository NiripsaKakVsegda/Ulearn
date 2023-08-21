import { StudentsFilter } from "../../../models/instructor";

export default {
	modalHeader: 'Фильтры',
	buttons: {
		apply: 'Применить',
		reset: 'Сбросить',
		cancel: 'Отменить'
	},
	filters: {
		unit: 'Модуль',
		slide: 'Слайд',
		studentsFilter: 'Фильтр пользователей',
		students: 'Стдуденты',
		groups: 'Группы'
	},
	allUnits: 'Все модули',
	allSlides: 'Все слайды',
	studentsFilerValues: {
		[StudentsFilter.MyGroups]: "Мои группы",
		[StudentsFilter.All]: "Все студенты",
		[StudentsFilter.StudentIds]: "По студентам",
		[StudentsFilter.GroupIds]: "По группам",
	} as Record<StudentsFilter, string>
};
