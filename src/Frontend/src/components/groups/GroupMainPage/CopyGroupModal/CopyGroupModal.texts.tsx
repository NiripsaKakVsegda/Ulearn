import React from 'react';

export default {
	copyGroupHeader: 'Скопировать группу из курса',
	buildCopyGroupHint: (courseTitle: string) => <>
		Новая группа будет создана для курса <b>«{ courseTitle }»</b>. Скопируются все настройки группы
		(в том числе владелец), в неё автоматически добавятся студенты из копируемой группы.
		Преподаватели тоже будут добавлены в группу, если у них есть права на курс <b>«{ courseTitle }»</b>.
	</>,
	copyGroupButtonText: 'Cкопировать',

	selectCourseHint: 'Вы можете выбрать курс, в котором являетесь преподавателем',
	selectCoursePlaceholder: 'Выберите курс',
	selectGroupHint: 'Вам доступны только те группы, в которых вы являетесь преподавателем',

	buildChangeOwnerHint: (ownerName: string, courseTitle: string) => <>
		Владелец этой группы <b>{ ownerName }</b> не является преподавателем курса <b>«{ courseTitle }»</b>.
		Вы можете сделать себя владельцем скопированной группы.
	</>,
	makeMeOwnerCheckboxText: 'Сделать меня владельцем группы',
};
