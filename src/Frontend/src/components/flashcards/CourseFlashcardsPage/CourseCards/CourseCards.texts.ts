import getPluralForm from "../../../../utils/getPluralForm";

export default {
	openUnitButton: 'Открыть модуль',
	emptyUnitCardText: 'Новые вопросы для самопроверки открываются по мере прохождения курса',
	getNewUserFlashcardsInfo: (count: number) => {
		const plural = getPluralForm(
			count,
			'новая карточка',
			'новых карточки',
			'новых карточек'
		);

		return `${ count } ${ plural }`;
	}
}
