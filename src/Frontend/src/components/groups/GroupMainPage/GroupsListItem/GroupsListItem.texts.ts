import { GroupType } from "../../../../models/groups";
import getPluralForm from "../../../../utils/getPluralForm";

export default {
	unknownName: 'Неизвестный',
	buildStudentsCountMessage: (studentsCount: number, groupType: GroupType) => {
		const pluralForm = groupType === GroupType.SuperGroup
			? getPluralForm(
				studentsCount,
				'нераспределённый студент',
				'нераспределённых студента',
				'нераспределённых студентов'
			)
			: getPluralForm(
				studentsCount,
				'студент',
				'студента',
				'студентов'
			);
		return `${ studentsCount } ${ pluralForm }`;
	},
	buildTeachersList: (teachers: string[], isSuperGroup = false) => {
		if(teachers.length === 0) {
			return '';
		}
		const plural = isSuperGroup
			? 'Создатель'
			: teachers.length === 1
				? 'Преподаватель'
				: 'Преподаватели';
		return `${ plural }: ${ teachers.join(', ') }`;
	},
	buildExcessTeachersMessage: (teachersExcess: number) => ` и ещё ${ teachersExcess }`,

	getToggleArchiveButtonText: (isArchived: boolean) => isArchived ? 'Восстановить' : 'Архивировать',
	deleteGroupButtonText: 'Удалить',

	getProgressStateText: (enabled: boolean | undefined) => enabled ? 'Ведомость включена' : 'Ведомость выключена',
	getReviewStateText: (enabled: boolean | undefined) => enabled ? 'Код-ревью включено' : 'Код-ревью выключено',
};
