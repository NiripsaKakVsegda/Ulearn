import { Gender, ShortUserInfo } from "../../../../../models/users";

export default {
	buildPublicationText: (user?: ShortUserInfo): string | null => {
		const genderText = user?.gender === Gender.Female ? 'запланировала' : 'запланировал';
		return user?.visibleName
			? `${ genderText } публикацию ${ user.visibleName }`
			: null;
	},

	publish: 'запланировать публикацию',
	publishNow: 'опубликовать сейчас',
	save: 'сохранить изменения',
	cancel: 'отменить изменения',
	hide: 'скрыть',

	incorrectDateFormatError: 'Некорректный формат даты',
	noDateError: 'Укажите дату',

	incorrectTimeFormatError: 'Некорректный формат времени',
	noTimeError: 'Укажите время',
};
