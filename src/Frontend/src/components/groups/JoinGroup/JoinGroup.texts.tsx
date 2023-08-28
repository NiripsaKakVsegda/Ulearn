import React from "react";
import { Link } from "react-router-dom";
import { JoinGroupInfo } from "../../../models/groups";

export default {
	title: 'Присоединиться к группе',

	join: {
		title: 'Присоединиться к группе',
		buildMainInfo: (group: JoinGroupInfo, courseTitle?: string) => <>
			{ group.owner.visibleName } приглашает вас присоединиться
			к группе «{ group.name }» {courseTitle && <>в курсе «{ courseTitle }»</>}.<br/>
			Преподаватели группы смогут проверять ваши задания и тесты, проводить код-ревью и выставлять вам
			дополнительные баллы.
		</>,
		additionalInfo: 'Чтобы преподаватели могли связаться с вами в случае необходимости, ' +
			'мы покажем им вашу электронную почту и привязанные аккаунты социальных сетей.',
		userCanSeeProgress: 'После вступления вам станет доступна ведомость всей группы.',

		button: 'Присоединиться'
	},

	joined: {
		title: 'Вы в группе',
		buildInfo: (groupName: string, courseTitle?: string) => <>
			<p>
				Вы присоединились к группе «{ groupName }» {courseTitle && <>в курсе «{ courseTitle }»</>}.
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
		noDistributionLinkError: 'Группа ещё не настроена',
		buildGroupNotFoundError: (accountLink: string) => <>
			<p>
				Мы не смогли вас найти,
				убедитесь в правильности написания имени в вашем <Link to={ accountLink }>аккаунте</Link>.
			</p>
			<p>
				Или свяжитесь с вашим преподавателем
			</p>
		</>
	}
};
