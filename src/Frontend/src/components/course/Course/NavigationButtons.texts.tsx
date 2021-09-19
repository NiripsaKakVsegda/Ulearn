import React from "react";
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
	buildNextReviewText: (count: number): React.ReactText =>
		count > 0
			? `${ getPluralForm(count, 'Осталась', 'Осталось', 'Осталось') } ${ count } ${ getPluralForm(count,
				'работа', 'работ', 'работ') }`
			: 'Поздравляем, очередь пуста! Работ для проверки нет',
};

export default texts;
