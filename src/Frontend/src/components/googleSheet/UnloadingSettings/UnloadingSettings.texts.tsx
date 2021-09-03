import React from "react";
import styles from './unloadingSettings.less';

const texts = {
	backToList: '← Все выгрузки',
	settings: "Настройки",
	task: {
		buildAuthor: (authorName: string): React.ReactText => `Создатель выгрузки: ${ authorName }`,
		buildListId: (listId?: number, notSpecified?: boolean): React.ReactNode => <>
			List ID: {
			listId !== undefined
				? listId
				: <span className={ styles.error }>{ notSpecified ? 'не указан' : 'ошибка' }</span>
		}
		</>,
		buildSpreadsheetId: (spreadsheetId?: string, notSpecified?: boolean): React.ReactNode => <>
			Spreadsheet ID: {
			spreadsheetId !== undefined
				? spreadsheetId
				: <span className={ styles.error }>{ notSpecified ? 'не указан' : 'ошибка' }</span> }
		</>,
		isVisibleForStudents: 'Студенты увидят ссылку на гугл таблицу на странице ведомости курса',
		refreshTime: 'Интервал обновлений',
		refreshStartDate: 'Дата начала выгрузки',
		refreshEndDate: 'Дата окончания выгрузки',
		spreadsheetId: 'Spreadsheet Id ',
		listId: 'List Id',
	},
	button: {
		save: 'Сохранить',
		export: 'Выгрузить сейчас',
		delete: 'Удалить',
	},
};

export default texts;
