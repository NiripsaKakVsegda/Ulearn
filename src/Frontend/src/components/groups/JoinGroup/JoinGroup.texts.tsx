import React from "react";
import { GroupInfo } from "src/models/groups";
import { Link } from "ui";

export default {
	title: 'Присоединиться к группе',
	failTitle: 'Неудача!',

	joinButtonText: 'Присоединиться',
	additional: 'Чтобы преподаватели могли связаться с вами в случае необходимости, мы покажем им вашу электронную почту и привязанные аккаунты социальных сетей.',
	userCanSeeProgress: 'Вступив, вы сможете смотреть ведомость всей группы прямо на сайте.',

	failNoGoogleSheetText: 'Группа ещё не настроена',

	buildInstructionText: (group: GroupInfo) => {
		return (
			<>
				{ group.owner.visibleName } приглашает вас присоединиться к группе «{ group.name }» в курсе
				«{ group.courseTitle }».
				<br/>
				Преподаватели группы смогут проверять ваши задания и тесты, проводить код-ревью и выставлять вам
				дополнительные баллы.
			</>
		);
	},
	inviteDisabledText: 'В эту группу больше нельзя вступить',
	buildSuperGroupUserNotFound: (onLinkClick: (e: React.MouseEvent<HTMLAnchorElement>) => void) => <>Мы не смогли вас
		найти, убедитесь в правильности
		написания имени в вашем <Link onClick={ onLinkClick } href={ '/account/manage' }>аккаунте</Link>.<br/>
		Или свяжитесь с вашим преподавателем
	</>,
	superGroupUserNotFoundTableLinkText: 'таблицей',
	joined: {
		buildInstructionText: (group: GroupInfo) => {
			return (
				<>
					<h2>Вы в группе</h2>
					<p>
						Вы присоединились к группе «{ group.name }» в курсе «{ group.courseTitle }».
						Теперь этот курс доступен в меню «Мои курсы».<br/>
						Преподаватели группы смогут проверять ваши задания и тесты, проводить код-ревью и выставлять вам
						дополнительные баллы.
					</p>
				</>
			);
		},
		joinButtonText: 'Перейти к курсу →',
	}
};
