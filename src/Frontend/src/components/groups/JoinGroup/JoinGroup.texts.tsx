import React from "react";
import { Link } from "react-router-dom";
import { GroupType, JoinGroupInfo } from "../../../models/groups";
import styles from './JoinGroup.less';

export default {
	title: 'Присоединиться к группе',

	join: {
		getTitle: (groupType: GroupType) => groupType === GroupType.SingleGroup
			? 'Присоединиться к группе'
			: 'Присоединиться к курсу',
		buildMainInfo: (group: JoinGroupInfo) => {
			const isSingleGroup = group.groupType === GroupType.SingleGroup;
			const joinTo = isSingleGroup
				? `группе «${ group.name }» в курсе «${ group.courseTitle }»`
				: `курсу «${ group.courseTitle }»`;
			return <>
				{ group.owner.visibleName } приглашает вас присоединиться к { joinTo }.
				<br/>
				Преподаватели { isSingleGroup && 'группы' } смогут проверять ваши задания и тесты,
				проводить код-ревью и выставлять вам дополнительные баллы.
			</>;
		},
		additionalInfo: 'Чтобы преподаватели могли связаться с вами в случае необходимости, ' +
			'мы покажем им вашу электронную почту и привязанные аккаунты социальных сетей.',
		userCanSeeProgress: 'После вступления вам станет доступна ведомость всей группы.',

		button: 'Присоединиться'
	},

	joined: {
		title: 'Вы в группе',
		buildInfo: (groupName: string, courseTitle: string) => <>
			<p>
				Вы присоединились к группе «{ groupName }» в курсе «{ courseTitle }».
				Теперь этот курс доступен в меню «Мои курсы».
			</p>
			<p>
				Преподаватели группы смогут проверять ваши задания и тесты, проводить код-ревью и выставлять вам
				дополнительные баллы.
			</p>
		</>,

		navigateCourse: 'Перейти к курсу →',
	},

	error: {
		title: 'Неудача!',
		inviteLinkDisabled: 'В эту группу больше нельзя вступить',
		defaultGroupTitle: 'Почти получилось!',
		buildDefaultGroupError: (group: JoinGroupInfo, accountLink: string) => <>
			<p>
				Поздравляем! Вы вступили в курс «{ group.courseTitle }», осталось попасть в нужную группу.
			</p>
			<p>
				Для этого корректно напишите имя и фамилию в&nbsp;
				<Link to={ accountLink } target="_blank" rel="noopener noreferrer">
					аккаунте
				</Link>: на русском, без сокращений и никнеймов.
				<br/>
				После этого еще раз пройдите по ссылке для присоединения.
			</p>
			<p className={ styles.additional }>
				Если это не поможет, то немного подождите,
				преподаватель скоро завершит распределение по группам.
			</p>
		</>
	}
};
