import { ComponentMeta, ComponentStory } from "@storybook/react";
import UserAccessesModal, { AccessesType } from "./UserAccessesModal";
import { getMockedShortUser } from "../../../storiesUtils";
import { ShortCourseAccess } from "../../../models/courseAccess";
import { mockFunc } from "../../../utils/storyMock";
import { CourseAccessType } from "../../../consts/accessType";
import { momentToServerFormat } from "../../../utils/momentUtils";
import moment from "moment-timezone";
import { disableViewport } from "../../course/Navigation/stroies.data";

export default {
	title: "common/UserAccessesModal",
	component: UserAccessesModal,
	...disableViewport
} as ComponentMeta<typeof UserAccessesModal>;

const Template: ComponentStory<typeof UserAccessesModal> = (args) => (
	<UserAccessesModal { ...args } />
);

const user = getMockedShortUser({
	visibleName: 'Иван Иванов'
});

export const StudentAccessesModalNoAccessesStory = Template.bind({});
StudentAccessesModalNoAccessesStory.storyName = "No accesses";
StudentAccessesModalNoAccessesStory.args = {
	courseTitle: 'Основы программирования',
	user: user,
	accesses: [],
	accessesType: AccessesType.StudentAccesses,
	canViewProfile: false,
	onGrantAccess: mockFunc,
	onRevokeAccess: mockFunc,
	onClose: mockFunc
};

export const StudentAccessesModalActualAccessStory = Template.bind({});
StudentAccessesModalActualAccessStory.storyName = "Actual access";
StudentAccessesModalActualAccessStory.args = {
	courseTitle: 'Основы программирования',
	user: user,
	accesses: [getMockedShortCourseAccess({
		grantTime: momentToServerFormat(moment().subtract(30, "days")),
		expiresOn: momentToServerFormat(moment().subtract(-335, "days")) // add не работает, ломается storybook
	})],
	accessesType: AccessesType.StudentAccesses,
	canViewProfile: false,
	onGrantAccess: mockFunc,
	onRevokeAccess: mockFunc,
	onClose: mockFunc
};

export const StudentAccessesModalExpiredAccessStory = Template.bind({});
StudentAccessesModalExpiredAccessStory.storyName = "Expired access";
StudentAccessesModalExpiredAccessStory.args = {
	courseTitle: 'Основы программирования',
	user: user,
	accesses: [getMockedShortCourseAccess({
		grantTime: momentToServerFormat(moment().subtract(400, "days")),
		expiresOn: momentToServerFormat(moment().subtract(35, "days"))
	})],
	accessesType: AccessesType.StudentAccesses,
	canViewProfile: false,
	onGrantAccess: mockFunc,
	onRevokeAccess: mockFunc,
	onClose: mockFunc
};

export const StudentAccessesModalInstructorAccessesStory = Template.bind({});
StudentAccessesModalInstructorAccessesStory.storyName = "Instructor accesses";
StudentAccessesModalInstructorAccessesStory.args = {
	courseTitle: 'Основы программирования',
	user: user,
	accesses: [
		getMockedShortCourseAccess({
			accessType: CourseAccessType.addAndRemoveInstructors,
			grantTime: momentToServerFormat(moment().subtract(10, "days")),
			expiresOn: undefined
		}),
		getMockedShortCourseAccess({
			accessType: CourseAccessType.editPinAndRemoveComments,
			grantTime: momentToServerFormat(moment().subtract(30, "days")),
			expiresOn: undefined
		}),
	],
	accessesType: AccessesType.InstructorAccesses,
	canViewProfile: false,
	onGrantAccess: mockFunc,
	onRevokeAccess: mockFunc,
	onClose: mockFunc
};

function getMockedShortCourseAccess(access?: Partial<ShortCourseAccess>): ShortCourseAccess {
	return {
		id: 0,
		accessType: CourseAccessType.moderateUserGeneratedFlashcards,
		courseId: 'basicprogramming',
		grantedBy: getMockedShortUser({
			id: 'instructor',
			visibleName: 'Петр Петров'
		}),
		grantTime: momentToServerFormat(moment()),
		expiresOn: momentToServerFormat(moment().subtract(-365, "days")), // add не работает, ломается storybook
		...(access ?? {})
	};
}
