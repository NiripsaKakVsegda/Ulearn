import getPluralForm from "../../../../../../utils/getPluralForm";

export default {
	copyStudentsHeader: 'Скопировать студентов',
	copyButtonText: 'Cкопировать',
	selectCourseInfo: 'Выберите курс, в который надо скопировать студентов',
	selectCoursePlaceholder: 'Курс',
	selectGroupInfo: 'Выберите группу',
	selectGroupPlaceholder: 'Курс',
	buildGroupsNotFoundMessage: (courseTitle: string) => `В курсе ${ courseTitle } нет доступных вам групп`,
	buildStudentsCountMessage: (studentsCount: number) => {
		const pluralForm = getPluralForm(studentsCount, 'студент', 'студента', 'студентов');
		return `${ studentsCount } ${ pluralForm }`;
	}
};
