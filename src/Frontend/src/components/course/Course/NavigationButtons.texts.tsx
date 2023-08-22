import getPluralForm from "src/utils/getPluralForm";

const texts = {
	next: "Далее",
	previous: "Назад",
	nextModule: "Следующий модуль",
	previousModule: "Предыдущий модуль",
	loading: 'Идет загрузка',
	nextSubmissionDisabledHint: 'Сначала оцените или сбросьте оценку',

	nextReviewLinkText: 'Следующее решение',
	returnToCheckingQueuePage: 'Вернуться',
	buildNextReviewText: (count: number, notAllLoaded = false): string => {
		if(count <= 0) {
			return 'Поздравляем, очередь пуста! Работ для проверки нет';
		}
		if(notAllLoaded) {
			return `Осталось ${ count }+ работ`;
		}

		return `${ getPluralForm(count, 'Осталась', 'Осталось', 'Осталось') } 
			${ count } ${ getPluralForm(count, 'работа', 'работы', 'работ') }`;
	},
};

export default texts;
