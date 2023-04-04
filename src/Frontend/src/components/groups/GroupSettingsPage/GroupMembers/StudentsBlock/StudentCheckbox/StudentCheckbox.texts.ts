import { GroupStudentInfo } from "../../../../../../models/groups";
import getGenderForm from "../../../../../../utils/getGenderForm";
import { getMoment } from "../../../../../../utils/momentUtils";

export default {
	buildAddingTimeInfo: (studentInfo: GroupStudentInfo) => {
		const genderForm = getGenderForm(studentInfo.user.gender, 'вступила', 'вступил');
		const moment = getMoment(studentInfo.addingTime);
		return ` ${ genderForm } ${ moment }`;
	}
};
