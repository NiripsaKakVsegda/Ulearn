import React from "react";

const texts = {
	runningText: 'Проверка на списывание: ищу похожие решения у других студентов…',
	getSuspicionText: (
		count: number,
		strongSuspicion?: boolean,
	): React.ReactText => {
		const text = count > 0
			? `у ${ count } других студентов найдены ${ strongSuspicion ? 'очень' : '' } похожие решения.`
			: `похожих решений не найдено`;
		return `Проверка на списывание: ${ text }`;
	},
	antiPlagiarismDetailsLinkText: `Посмотреть подробности`,
	notCheckingText: `Эта задача не проверяется на списывание`,
	errorText: `Нам не удалось проверить эту задачу`,
	scoreZeroText: `Поставить 0 %`,
};

export default texts;
