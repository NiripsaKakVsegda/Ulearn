import React from 'react';

export default {
	copyText: 'Скопировать ссылку',
	onCopyToastText: 'Ссылка скопирована',
	superGroupHintText: `Используя эту ссылку студенты попадут в группу, указанную в гугл-таблице`,

	buildIsLinkEnabledText: (isInviteLinkEnabled: boolean) => <>Ссылка для вступления в
		группу { isInviteLinkEnabled ? ' включена' : ' выключена' }</>,
	//buildLink: (inviteHash: string) => `${ window.location.origin }/Account/JoinGroup?hash=${ inviteHash }`,
	buildLink: (inviteHash: string) => `${ window.location.origin }/groups/${ inviteHash }`,
};
