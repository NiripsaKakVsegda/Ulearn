import { Gender, ShortUserInfo } from "../../../../../models/users";

export default {
	buildPublicationText: (user?: ShortUserInfo): string => {
		const genderText = user?.gender === Gender.Female ? 'запланировала' : 'запланировал';
		return user?.visibleName
			? `${ genderText } публикацию ${ user.visibleName }`
			: 'не публикуется';
	},

	publish: 'запланировать публикацию',
	save: 'сохранить изменения',
	cancel: 'отменить изменения',
	hide: 'скрыть',

	incorrectDateFormatError: 'Некорректный формат даты',
	incorrectDateError: 'Неверная дата',
	noDateError: 'Укажите дату',

	incorrectTimeFormatError: 'Некорректный формат времени',
	noTimeError: 'Укажите время',
};
