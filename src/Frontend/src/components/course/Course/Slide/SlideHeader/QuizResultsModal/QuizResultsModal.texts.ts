import getPluralForm from "../../../../../../utils/getPluralForm";

export default {
	studentsSubmissions: 'Выгрузка результатов',
	getResultsInfo: 'Выберите группу для которой хотите получить выгрузку результатов. Формат файла: tsv.',
	selectGroupHint: 'Вам доступны только те группы, в которых вы являетесь преподавателем.',
	selectGroupPlaceholder: 'Выберите группу',
	buildStudentsCountMessage: (studentsCount: number) => {
		const pluralForm = getPluralForm(studentsCount, 'студент', 'студента', 'студентов');
		return `${ studentsCount } ${ pluralForm }`;
	},
	noGroupsMessage: 'В курсе нет доступных вам групп',
	noStudentsMessage: 'В выбранной группе нет ни одного студента',
	downloadResults: 'Скачать',
	closeModal: 'Закрыть',
};
