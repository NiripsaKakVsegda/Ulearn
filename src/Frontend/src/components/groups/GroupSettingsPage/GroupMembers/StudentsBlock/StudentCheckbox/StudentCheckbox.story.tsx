import { ComponentMeta, ComponentStory } from "@storybook/react";
import StudentCheckbox from "./StudentCheckbox";
import { mockFunc } from "../../../../../../utils/storyMock";
import { GroupStudentInfo } from "../../../../../../models/groups";
import { getMockedShortUser } from "../../../../../../storiesUtils";
import { momentToServerFormat } from "../../../../../../utils/momentUtils";
import moment from "moment-timezone";
import { AccountState } from "../../../../../../redux/account";
import { CourseAccessType } from "../../../../../../consts/accessType";

export default {
	title: "Group/StudentCheckbox",
	component: StudentCheckbox
} as ComponentMeta<typeof StudentCheckbox>;

const Template: ComponentStory<typeof StudentCheckbox> = (args) => (
	<StudentCheckbox { ...args } />
);

export const StudentCheckboxNoAccessesStory = Template.bind({});
StudentCheckboxNoAccessesStory.storyName = "No accesses";
StudentCheckboxNoAccessesStory.args = {
	studentInfo: getMockedStudentInfo(),
	account: {
		isSystemAdministrator: false,
		systemAccesses: []
	} as unknown as AccountState,
	isChecked: false,
	onCheck: mockFunc,
	onChangeStudentAccesses: mockFunc
};

export const StudentCheckbox1AccessStory = Template.bind({});
StudentCheckbox1AccessStory.storyName = "1 access";
StudentCheckbox1AccessStory.args = {
	studentInfo: getMockedStudentInfo({
		accesses: [{
			id: 0,
			accessType: CourseAccessType.moderateUserGeneratedFlashcards,
			courseId: 'basicprogramming',
			grantedBy: getMockedShortUser(),
			grantTime: '',
			expiresOn: ''
		}]
	}),
	account: {
		isSystemAdministrator: false,
		systemAccesses: []
	} as unknown as AccountState,
	isChecked: false,
	onCheck: mockFunc,
	onChangeStudentAccesses: mockFunc
};

function getMockedStudentInfo(student?: Partial<GroupStudentInfo>): GroupStudentInfo {
	return {
		user: getMockedShortUser({
			visibleName: 'Иван иванов'
		}),
		accesses: [],
		addingTime: momentToServerFormat(moment().subtract(3, 'd')),
		...student
	};
}
