import { getMoment } from "../../../utils/momentUtils";
import { FlashcardModerationStatus } from "../../../models/flashcards";
import styles from './openedFlashcardWrapper.less';
import React from "react";

export default {
	buildUnitTitle: (title: string) => `Модуль «${ title }»`,
	flashcardPublishedInfo: 'Поздравляем, модератор опубликовал вашу карточку для всех пользователей! ' +
		'Её больше нельзя редактировать и удалять. ' +
		'Спасибо за развитие курса!',
	meta: {
		author: 'Автор',
		buildLastChangeInfo: (timeStamp: string) => `Последнее изменение ${ getMoment(timeStamp) }`,
		buildStatusInfo: (status: FlashcardModerationStatus) => {
			let statusNode = <span></span>;
			switch (status) {
				case FlashcardModerationStatus.New:
					statusNode = <span className={ styles.newColor }>на модерации</span>;
					break;
				case FlashcardModerationStatus.Approved:
					statusNode = <span className={ styles.approvedColor }>опубликована</span>;
					break;
				case FlashcardModerationStatus.Declined:
					statusNode = <span className={ styles.declinedColor }>отклонена</span>;
					break;
			}
			return <>Статус: { statusNode }</>;
		},
		moderator: 'Модератор',
		buildModerationTimeStampInfo: (timestamp: string, status: FlashcardModerationStatus) => {
			let verb = 'Последняя модерация';
			switch (status) {
				case FlashcardModerationStatus.Approved:
					verb = 'Опубликована';
					break;
				case FlashcardModerationStatus.Declined:
					verb = 'Отклонена';
					break;
			}
			return `${ verb } ${ getMoment(timestamp) }`;
		}
	},
	controls: {
		edit: 'Редактировать',
		remove: 'Удалить',
		publish: 'Начать публикацию',
		decline: 'Отклонить',
		publishHint: 'Вы сможете отредактировать карточку перед публикацией'
	}
};
