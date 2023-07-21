import getPluralForm from "src/utils/getPluralForm";
import React from "react";
import { isTimeArrived, momentFromServerToLocal } from "src/utils/momentUtils";
import { DeadLineInfo } from "src/models/deadLines";
import { Link } from "react-router-dom";
import { CourseRoleType } from "../../../consts/accessType";
import { isCourseAdmin, isInstructor, UserRoles } from "../../../utils/courseRoles";

export default {
	ulearnDescription: 'Интерактивные учебные онлайн-курсы по программированию',
	ulearnTitle: 'Ulearn',

	flashcardsTitle: 'Вопросы для самопроверки',
	codeReviewLink: '← Код-ревью и проверка тестов',
	renderFooter: (userRoles: UserRoles): React.ReactNode => {
		const _isCourseAdmin = isCourseAdmin(userRoles);
		const _isInstructor = _isCourseAdmin || isInstructor(userRoles);

		const availableChannels = [
			_isCourseAdmin && { name: 'авторов курсов', link: 'ulearnAuthors' },
			_isInstructor && { name: 'преподавателей', link: 'ulearnteachers' }
		].filter(c => c) as unknown as { name: string, link: string }[];
		const availableChannelsMarkup = availableChannels
			.map<React.ReactNode>(({ link, name }, i) => (
				<a
					key={ i }
					href={ `https://t.me/${ link }` }
					rel={ "noopener noreferrer" }
					target={ "_blank" }
				>
					{ i === 0 ? 'Канал' : 'канал' } { name }
				</a>
			));
		return (
			<>
				<p><Link to="/Home/Terms">Условия использования платформы</Link></p>
				<p>
					Вопросы и пожеланиями пишите на <a href="mailto:support@ulearn.me">support@ulearn.me</a>
				</p>
				{
					availableChannels.length > 0 &&
					<p>
						{
							availableChannelsMarkup.reduce((prev, curr) => [prev, ', ', curr])
						}
						&nbsp;в телеграме
					</p>
				}
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
	afterDeadLine: (deadLineInfo: DeadLineInfo, slideMaxScore: number,): string => {
		if(deadLineInfo.scorePercent === 100) {
			return '';
		}

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
	renderDeadLineInfo: (deadLineInfo: DeadLineInfo, isAnyDeadLineArrivedBefore = false): string => {
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
