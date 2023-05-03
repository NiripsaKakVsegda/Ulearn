import React from 'react';

export default {
	copyLink: 'Скопировать ссылку',
	linkCopiedToast: 'Ссылка скопирована',
	superGroupHintText: `Используя эту ссылку студенты попадут в группу, указанную в гугл-таблице`,

	buildLinkHint: (isEnabled: boolean,
		isSuperGroup: boolean
	) => `Ссылка для вступления в ${ isSuperGroup ? 'супер-' : '' }группу ` + (isEnabled ? 'включена' : 'выключена'),
	buildLink: (inviteHash: string) => `${ window.location.origin }/groups/${ inviteHash }`,
};
