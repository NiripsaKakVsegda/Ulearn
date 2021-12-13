import React from "react";

import { convertDefaultTimezoneToLocal } from "src/utils/momentUtils";
import { Star, Star2, Edit, Trash, } from "icons";

export default {
	sendButton: 'Ответить',
	editButton: <><Edit/> Редактировать</>,
	copyButton: <>Скопировать текст</>,
	deleteButton: <><Trash/> Удалить</>,
	getToggleFavouriteMarkup: (isFavourite: boolean): React.ReactElement => isFavourite
		? <span><Star color={ '#F69C00' }/> Убрать из Избранных</span>
		: <span><Star2/> Добавить в Избранные</span>,

	editing: {
		save: 'Сохранить',
		cancel: 'Отменить',
	},

	botReview: {
		hintText: 'Опубликовать от своего имени',
		assign: 'Присвоить',
		delete: 'Удалить',
	},

	getLineCapture: (startLine: number, finishLine: number): string => {
		return startLine === finishLine ? `строка ${ startLine + 1 }` : `строки ${ startLine + 1 }-${ finishLine + 1 }`;
	},

	getAddingTime: (addingTime: string): string => {
		return convertDefaultTimezoneToLocal(addingTime).format("DD MMMM YYYY");
	},
};
