import getPluralForm from "src/utils/getPluralForm";
import React from "react";
import { isTimeArrived, momentFromServerToLocal } from "src/utils/momentUtils";
import { DeadLineInfo } from "src/models/deadLines";
import { Link } from "react-router-dom";

export default {
	ulearnDescription: 'Интерактивные учебные онлайн-курсы по программированию',
	ulearnTitle: 'Ulearn',

	flashcardsTitle: 'Вопросы для самопроверки',
	codeReviewLink: '← Код-ревью и проверка тестов',
	renderFooter: (): React.ReactNode => {
		return (
			<>
				<p><Link to="/Home/Terms">Условия использования платформы</Link></p>
				<p>
					Вопросы и пожеланиями пишите на <a href="mailto:support@ulearn.me">support@ulearn.me</a>
				</p>
				<p>
					Сделано в <a href="https://kontur.ru/career">Контуре</a>
				</p>
			</>
		);
	},

	//additional content
	unitIsNotPublished: 'Модуль ещё не опубликован',
	slideIsNotPublished: 'Слайд ещё не опубликован',

	//deadlines
	afterDeadLine: (deadLineInfo: DeadLineInfo, slideMaxScore: number,): React.ReactText => {
		const score = Math.ceil(deadLineInfo.scorePercent * slideMaxScore / 100);

		if(isTimeArrived(deadLineInfo.date)) {
			return `Вы получите не более ${ score } ${ getPluralForm(score, 'балла',
				'баллов', 'баллов') } за авто-проверку`;
		}

		return (
			`Если сдать после срока, вы получите не более ${ score } ${ getPluralForm(score, 'балла',
				'баллов', 'баллов') } за авто-проверку`
		);
	},
	renderDeadLineInfo: (deadLineInfo: DeadLineInfo, isAnyDeadLineArrivedBefore = false): React.ReactText => {
		if(isTimeArrived(deadLineInfo.date)) {
			return `Дедлайн для сдачи прошёл`;
		}
		if(isAnyDeadLineArrivedBefore) {
			return `Следующий дедлайн для сдачи ${ momentFromServerToLocal(deadLineInfo.date).format(
				'DD.MM.YYYY HH:mm') }`;
		}
		return (
			`Дедлайн для сдачи ${ momentFromServerToLocal(deadLineInfo.date).format('DD.MM.YYYY HH:mm') }`
		);
	},
};
