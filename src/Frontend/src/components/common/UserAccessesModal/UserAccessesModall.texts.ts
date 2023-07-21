import { CourseAccessType } from "../../../consts/accessType";
import { getMoment, momentFromServerToLocal } from "../../../utils/momentUtils";
import moment from "moment-timezone";

export default {
	modalHeader: 'Управление правами',
	getCourseTitleInfo: (title: string) => `Курс «${ title }»`,
	userPrefix: `Пользователь `,
	accessesTitles: {
		[CourseAccessType.moderateUserGeneratedFlashcards]: 'Модерировать флешкарты',
	},

	buildAccessInfo: (grantedByVisibleName: string, grantTime: string) =>
		`Выдал ${ grantedByVisibleName } ${ getMoment(grantTime) }`,

	buildExpiresInfo: (expiresOn: string) => {
		const expiresOnMoment = momentFromServerToLocal(expiresOn);
		const expiresOnDateTime = expiresOnMoment.format('YY.MM.DD HH:mm');
		return expiresOnMoment.isAfter(moment())
			? `Действителены до ${ expiresOnDateTime }`
			: `Были действителены до ${ expiresOnDateTime }`;
	},

	accessControls: {
		grant: 'Выдать',
		revoke: 'Забрать',
		refresh: 'Продлить'
	}
};
