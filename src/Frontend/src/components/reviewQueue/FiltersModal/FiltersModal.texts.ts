import { StudentsFilter } from "../../../models/instructor";
import getPluralForm from "../../../utils/getPluralForm";

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
	} as Record<StudentsFilter, string>,

	error: {
		buildTooManyStudentsError: (maxCount: number) => {
			const plural = getPluralForm(maxCount, 'студента', 'студентов', 'студентов');
			return `Выберите не более ${ maxCount } ${ plural }`;
		},
		buildTooManyGroupsError: (maxCount: number) => {
			const plural = getPluralForm(maxCount, 'группы', 'групп', 'групп');
			return `Выберите не более ${ maxCount } ${ plural }`;
		},
	}
};
