export const sheetRegex = /^https:\/\/docs.google.com\/spreadsheets\/d\/(.)+\/edit#gid=(\d)+$/;
export const linkExample = 'https://docs.google.com/spreadsheets/d/{spreadsheet-id}/edit#gid={list-id}';
export const refreshPeriods = [
	{ label: '10 минут', value: '10' },
	{ label: '1 час', value: '60' },
	{ label: '1 день', value: '1440' },
];
export const texts = {
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
