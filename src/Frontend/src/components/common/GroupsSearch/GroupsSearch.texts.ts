import getPluralForm from "../../../utils/getPluralForm";

export default {
	archivedGroup: 'Архив',
	placeholder: 'Введите название группы...',
	noGroupsFound: 'Не найдено ни одной группы',
	getStudentsCount: (count: number) => {
		const plural = getPluralForm(count, 'студент', 'студента', 'студентов');
		return `${ count } ${ plural }`;
	},
};
