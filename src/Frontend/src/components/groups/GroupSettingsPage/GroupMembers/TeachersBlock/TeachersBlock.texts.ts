import { GroupAccessesInfo } from "../../../../../models/groups";
import getGenderForm from "../../../../../utils/getGenderForm";
import { getMoment } from "../../../../../utils/momentUtils";

export default {
	teachersHeader: 'Преподаватели',
	teachersInfo: 'Преподаватели могут видеть список участников группы, проводить код-ревью' +
		'и проверку тестов, выставлять баллы и смотреть ведомость.',
	owner: 'Владелец',

	buildGrantedInfo: (accessesInfo: GroupAccessesInfo): string => {
		const genderFrom = getGenderForm(accessesInfo.grantedBy.gender, 'предоставила', 'предоставил');
		const grantedBy = accessesInfo.grantedBy.visibleName;
		const moment = getMoment(accessesInfo.grantTime);
		return `Полный доступ ${ genderFrom } ${ grantedBy } ${ moment }`;
	}
};
