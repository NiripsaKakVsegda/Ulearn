import MembersList from "./MembersList";
import { ComponentMeta, ComponentStory } from "@storybook/react";
import { GroupStudentInfo } from "../../../../models/groups";
import { getMockedShortUser } from "../../../../storiesUtils";
import { momentToServerFormat } from "../../../../utils/momentUtils";
import moment from "moment-timezone";
import { mockFunc } from "../../../../utils/storyMock";
import { CourseAccessType } from "../../../../consts/accessType";

export default {
	title: 'Group/common/MembersList',
	component: MembersList
} as ComponentMeta<typeof MembersList>;

const Template: ComponentStory<typeof MembersList> = (args) => (
	<MembersList { ...args } />
);

export const MembersListStory = Template.bind({});
MembersListStory.storyName = 'Default';
MembersListStory.args = {
	members: [
		getMockedStudentInfo(),
		getMockedStudentInfo({
			user: getMockedShortUser({
				id: '2',
				visibleName: 'Пётр Петров',
				lastName: 'Петров',
				firstName: 'Пётр',
			}),
			accesses: [
				{
					id: 1,
					accessType: CourseAccessType.moderateUserGeneratedFlashcards,
					courseId: 'courseId',
					grantTime: '',
					expiresOn: '',
					grantedBy: getMockedShortUser()
				}
			]
		}),
	],
	onCopyStudents: mockFunc,
	onResetLimits: mockFunc,
	onDeleteStudents: mockFunc,
	onChangeAccesses: mockFunc
};


function getMockedStudentInfo(student?: Partial<GroupStudentInfo>): GroupStudentInfo {
	return {
		user: getMockedShortUser({
			id: '1',
			visibleName: 'Иван Иванов',
			lastName: 'Иванов',
			firstName: 'Иван',
		}),
		accesses: [],
		addingTime: momentToServerFormat(moment().subtract(3, 'd')),
		...student
	};
}
