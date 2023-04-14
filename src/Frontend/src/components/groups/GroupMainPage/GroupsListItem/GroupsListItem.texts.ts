import getPluralForm from "../../../../utils/getPluralForm";

export default {
	unknownName: 'Неизвестный',
	buildStudentsCountMessage: (studentsCount: number) => {
		const pluralForm = getPluralForm(studentsCount, 'студент', 'студента', 'студентов');
		return `${ studentsCount } ${ pluralForm }`;
	},
	buildTeachersList: (teachers: string[]) => {
		if(teachers.length === 0) {
			return '';
		}
		const plural = teachers.length === 1 ? 'Преподаватель' : 'Преподаватели';
		return `${ plural }: ${ teachers.join(', ') }`;
	},
	buildExcessTeachersMessage: (teachersExcess: number) => ` и ещё ${ teachersExcess }`,

	getToggleArchiveButtonText: (isArchived: boolean) => isArchived ? 'Восстановить' : 'Архивировать',
	deleteGroupButtonText: 'Удалить',

	getProgressStateText: (enabled: boolean) => enabled ? 'Ведомость включена' : 'Ведомость выключена',
	getReviewStateText: (enabled: boolean) => enabled ? 'Код-ревью включено' : 'Код-ревью выключено',
};
