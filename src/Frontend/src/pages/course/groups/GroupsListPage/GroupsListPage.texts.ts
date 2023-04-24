export default {
	buildPageTitle: (courseTitle: string) => `Группы в курсе ${ courseTitle.toLowerCase() }`,
	noActiveGroupsMessage: 'У вас нет активных групп. Создайте группу и пригласите в неё студентов, чтобы видеть их ' +
		'прогресс, проверять их тесты и делать код-ревью их решений.',
	noArchiveGroupsMessage: 'У вас нет архивных групп. Когда какая-нибудь группа станет вам больше не нужна, заархивируйте её. ' +
		'Архивные группы будут жить здесь вечно и не помешают вам в текущей работе. ' +
		'Однако если понадобится, вы всегда сможете вернуться к ним.',

	buildDeleteGroupToast: (groupName: string) => `Группа «${ groupName }» удалена`,
	buildArchiveToggleToast: (groupName: string, isSuperGroup: boolean, isArchived: boolean) => isArchived
		? `${ isSuperGroup ? "Супер-группа" : "Группа" } «${ groupName }» заархивирована`
		: `${ isSuperGroup ? "Супер-группа" : "Группа" } «${ groupName }» восстановлена`,
};
