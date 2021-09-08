import api from "../../api";
import { Checkbox, DatePicker, Gapped, Input, Switcher } from "ui";
import React from "react";
import moment from "moment";
import styles from "./utils.less";

export const sheetRegex = /^https:\/\/docs.google.com\/spreadsheets\/d\/(.)+\/edit#gid=(\d)+$/;
export const linkExample = 'https://docs.google.com/spreadsheets/d/{spreadsheet-id}/edit#gid={list-id}';

export const texts = {
	task: {
		refreshTime: 'Интервал обновлений',
		buildAuthor: (authorName: string): React.ReactText => `Создатель выгрузки: ${ authorName }`,
		isVisibleForStudents: 'Студенты увидят ссылку на гугл таблицу на странице ведомости курса',
		refreshStartDate: 'Дата начала выгрузки',
		refreshEndDate: 'Дата окончания выгрузки',
	},
	button: {
		save: 'Сохранить',
		export: 'Выгрузить сейчас',
		delete: 'Удалить',
		create: 'Создать',
	},
	toast: {
		add: 'Выгрузка успешно создана',
		addFail: 'Ошибка при создании выгрузки',

		save: 'Изменения выгрузки успешно сохранены',
		saveFail: 'Ошибка при сохранении изменений выгрузки',

		delete: 'Выгрузка удалена',
		deleteFail: 'Ошибка при удалении выгрузки',

		export: 'Выгрузка в Google sheet успешно совершена',
		exportFail: 'Ошибка выгрузки в Google sheet',
	},
};

export const Api = {
	getAllCourseGroups: api.groups.getCourseGroups,
	getAllCourseTasks: api.googleSheet.getAllCourseTasks,
	createTask: api.googleSheet.addNewTask,
	updateTask: api.googleSheet.updateCourseTask,
	deleteTask: api.googleSheet.deleteTask,
	exportTaskNow: api.googleSheet.exportTaskNow,
	getTaskById: api.googleSheet.getTaskById,
};

export const refreshPeriods = [
	{ label: '10 минут', value: '10' },
	{ label: '1 час', value: '60' },
	{ label: '1 день', value: '1440' },
];

export const renderRefreshPeriodSwitcher = (
	refreshTimeInMinutes = 60,
	changeRefreshInterval: (value: string) => void
): React.ReactElement => {
	let items = [...refreshPeriods];

	for (let i = 0; i < items.length; i++) {
		const value = parseInt(items[i].value);
		if(refreshTimeInMinutes === value) {
			break;
		}
		if(i === items.length - 1 || refreshTimeInMinutes < parseInt(items[i + 1].value)) {
			items = items.slice(0, i + 1);
			items.push({
				label: `${ refreshTimeInMinutes } минут`,
				value: refreshTimeInMinutes.toString(),
			});
			items = items.concat(refreshPeriods.slice(i + 1, refreshPeriods.length - i));
			break;
		}
	}

	return (
		<Gapped gap={ 8 }>
			<Switcher
				items={ items }
				value={ refreshTimeInMinutes.toString() }
				onValueChange={ changeRefreshInterval }/>
			{ texts.task.refreshTime }
		</Gapped>
	);
};

export const isLinkMatchRegexp = (value: string): boolean => {
	return sheetRegex.test(value);
};

export const renderEditableFields = (
	isVisibleForStudents: boolean | undefined,
	changeVisibility: () => void,
	refreshStartDate: string | undefined,
	changeRefreshStartDate: (date: string) => void,
	refreshEndDate: string | undefined,
	changeRefreshEndDate: (date: string) => void,
	link: string,
	changeLink: (newLink: string) => void,
	showLinkHint?: boolean,
): React.ReactElement[] => {
	return [
		<Gapped gap={ 8 }>
			<Checkbox checked={ isVisibleForStudents } onClick={ changeVisibility }/>
			{ texts.task.isVisibleForStudents }
		</Gapped>,
		<Gapped gap={ 8 }>
			<DatePicker
				onValueChange={ changeRefreshStartDate }
				value={ moment(refreshStartDate).format('DD.MM.yyyy') }/>
			{ texts.task.refreshStartDate }
		</Gapped>,
		<Gapped gap={ 8 }>
			<DatePicker
				onValueChange={ changeRefreshEndDate }
				value={ moment(refreshEndDate).format('DD.MM.yyyy') }/>
			{ texts.task.refreshEndDate }
		</Gapped>,
		<Gapped gap={ 8 } vertical>
			<Input
				className={ styles.linkInput }
				selectAllOnFocus
				error={ link.length > 0 && !isLinkMatchRegexp(link) }
				value={ link }
				onValueChange={ changeLink }
				placeholder={ linkExample }
			/>
			{ showLinkHint &&
			<span className={ styles.aboutAccessAccount }>В настройках должен быть предоставлен доступ для ulearn@testproject-318905.iam.gserviceaccount.com в качестве редактора</span> }
		</Gapped>
	];
};
