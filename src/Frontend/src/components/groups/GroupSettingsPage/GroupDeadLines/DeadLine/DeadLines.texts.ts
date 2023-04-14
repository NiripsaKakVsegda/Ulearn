import getPluralForm from "../../../../../utils/getPluralForm";

export default {
	saveButtonText: 'сохранить',

	saveDeadLine: 'Сохранить изменения',
	deleteDeadLine: 'удалить',
	cancelChanges: 'отменить изменения',
	copyDeadLineForNextUnit: 'скопировать для следующего модуля',

	saveBeforeAdding: 'Сначала сохраните созданный дедлайн',
	overlapConflict: 'Этот дедлайн конфликтует с другим. ' +
		'Он не применится, потому что есть дедлайн с наименьшим ограничением',

	incorrectDateFormatError: 'Некорректный формат даты',
	noDateError: 'Укажите дату',

	incorrectTimeFormatError: 'Некорректный формат времени',
	noTimeError: 'Укажите время',

	buildSelectedStudentsCountTitle: (count: number) => {
		const pluralChoose = getPluralForm(count, 'Выбран', 'Выбраны', 'Выбрано');
		const pluralStudents = getPluralForm(count, 'студент', 'студента', 'студентов');
		return `${ pluralChoose } ${ count } ${ pluralStudents }`;
	},
	allStudentsSelected: 'Выбраны все студенты',
};
