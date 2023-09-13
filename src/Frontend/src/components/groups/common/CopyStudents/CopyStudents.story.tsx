import CopyStudents from "./CopyStudents";
import { ComponentMeta, ComponentStory } from "@storybook/react";
import { mockedSearchGroups } from "../../../reviewQueue/storyData";
import { mockFunc } from "../../../../utils/storyMock";

export default {
	title: 'Group/common/CopyStudents',
	component: CopyStudents
} as ComponentMeta<typeof CopyStudents>;

const Template: ComponentStory<typeof CopyStudents> = (args) => (
	<CopyStudents { ...args } />
);

export const CopyStudentsStory = Template.bind({});
CopyStudentsStory.storyName = 'Default';
CopyStudentsStory.args = {
	courses: [
		{
			id: 'basicprogramming',
			title: 'Основы программирования',
			isTempCourse: false,
			apiUrl: ''
		}
	],
	searchGroups: (courseId, query) => mockedSearchGroups(query),
	onCopyStudents: mockFunc
};

export const CopyStudentsModalStory = Template.bind({});
CopyStudentsModalStory.storyName = 'Modal';
CopyStudentsModalStory.args = {
	courses: [
		{
			id: 'basicprogramming',
			title: 'Основы программирования',
			isTempCourse: false,
			apiUrl: ''
		}
	],
	searchGroups: (courseId, query) => mockedSearchGroups(query),
	onCopyStudents: mockFunc,
	asModal: true
};
