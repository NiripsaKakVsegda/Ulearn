export default {
	getTheorySlidesPlural: (count: number) => count > 1 ? 'Источники' : 'Источник',
	getTheorySlideName: (title: string) => `Слайд «${ title }»`,
	showAnswerButton: 'Показать ответ'
};
