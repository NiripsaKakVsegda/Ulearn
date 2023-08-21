import getPluralForm from "../../../utils/getPluralForm";

export default {
	getSubmissionsCountInfo: (count: number) => {
		const plural = getPluralForm(count, 'решение', 'решения', 'решений');
		return `${ count } ${ plural }`;
	},
	checkAllButton: 'Проверить все',
	buildLockedByInfo(name?: string) {
		return `Проверяет ${ name }`;
	}
};
