import { GroupStudentInfo } from "../../../../models/groups";
import getGenderForm from "../../../../utils/getGenderForm";
import { getMoment } from "../../../../utils/momentUtils";
import getPluralForm from "../../../../utils/getPluralForm";

export default {
	selectAll: 'Выбрать всех',

	buildAddingTimeInfo: (studentInfo: GroupStudentInfo) => {
		const genderForm = getGenderForm(studentInfo.user.gender, 'вступила', 'вступил');
		const moment = getMoment(studentInfo.addingTime);
		return ` ${ genderForm } ${ moment }`;
	},
	buildAccessesCountInfo: (count: number) => {
		const accessPlural = getPluralForm(count, 'право', 'права', 'прав');

		return `выдано ${ count } доп. ${ accessPlural }`;
	},
	changeAccessesButton: 'Управлять правами',

	actions: {
		copyToGroup: 'Скопировать в группу...',
		resetLimits: 'Сбросить ограничения',
		remove: 'Исключить из группы',
	},

	noMembers: 'В группе нет ни одного студента'
}
