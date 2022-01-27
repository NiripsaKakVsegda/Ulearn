import React from "react";

export default {
	info: <>
		Автор курса может создать дополнительные модули и слайды, которые по умолчанию не показываются студентам.<br/>
		<a href={ 'https://docs.google.com/document/d/1Avke9Rilm92mlR8kXK_T687_G2rZb7gJ_ccKmASzy4Y/edit#heading=h.dne7e6529e69' }>Как
			это сделать</a><br/>
		Преподаватели с помощью интерфейса ниже могут открывать этот контент своим группам в нужный момент времени.<br/>
		Если студент находится в нескольких учебных группах, то доступ к контенту появится, когда наступит момент
		открытия хотя бы в одной его группе.
	</>,
	buildPublicationText: (visibleName?: string): string => {
		return visibleName
			? 'опубликовал ' + visibleName
			: 'не опубликован';
	},
	hide: 'скрыть',
	successHide: 'Контент был снят с публикации',
	save: 'сохранить изменения',
	successSave: 'Изменения сохранены',
	publish: 'опубликовать',
	cancel: 'отменить изменения',
	error: 'Возникла ошибка',
	buildTimeHint: (): string => `Время указано в GMT+3`,
	noAdditionalContent: 'В этом курсе нет слайдов или модулей помеченных как доп. контент',
};
