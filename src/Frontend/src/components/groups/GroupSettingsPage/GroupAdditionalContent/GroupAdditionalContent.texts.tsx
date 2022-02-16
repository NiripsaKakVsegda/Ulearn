import React from "react";

export default {
	info: <>
		Автор курса может создать дополнительные модули и слайды, которые по умолчанию не показываются студентам.<br/>
		<a href={ 'https://docs.google.com/document/d/1eiqZDVyKCA9PtLAMAOG6vCgWBT3EU2zCGKbjUUHdvig/edit#heading=h.xbmyxqp5068h' }>Как
			это сделать</a><br/>
		Преподаватели с помощью интерфейса ниже могут открывать этот контент своим группам в нужный момент времени.<br/>
		Если студент находится в нескольких учебных группах, то доступ к контенту появится, когда наступит момент
		открытия хотя бы в одной его группе.
	</>,
	buildPublicationText: (visibleName?: string): string => {
		return visibleName
			? 'запланировал публикацию ' + visibleName
			: 'не публикуется';
	},
	hide: 'скрыть',
	successHide: 'Контент был снят с публикации',
	save: 'сохранить изменения',
	successSave: 'Изменения сохранены',
	publish: 'запланировать публикацию',
	cancel: 'отменить изменения',
	error: 'Возникла ошибка',
	noAdditionalContent: 'В этом курсе нет слайдов или модулей помеченных как доп. контент',
};
