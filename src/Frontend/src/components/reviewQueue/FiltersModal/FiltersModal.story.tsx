import { ComponentMeta, ComponentStory } from "@storybook/react";
import FiltersModal from "./FiltersModal";
import { disableViewport } from "../../course/Navigation/stroies.data";
import { getMockFuncWithLogging } from "../../../utils/storyMock";
import {
	getMockedReviewQueueFilter,
	mockedCourseSlidesInfo,
	mockedFilteredGroups,
	mockedFilteredStudents,
	mockedSearchGroups,
	mockedSearchStudents
} from "../storyData";
import { StudentsFilter } from "../../../models/instructor";

export default {
	title: "Review Queue/FiltersModal",
	component: FiltersModal,
	...disableViewport
} as ComponentMeta<typeof FiltersModal>;

const Template: ComponentStory<typeof FiltersModal> = (args) => (
	<FiltersModal { ...args } />
);

export const FiltersModalStory = Template.bind({});
FiltersModalStory.storyName = "Default";
FiltersModalStory.args = {
	filter: getMockedReviewQueueFilter(),
	courseSlidesInfo: mockedCourseSlidesInfo,
	getStudents: mockedSearchStudents,
	getGroups: mockedSearchGroups,
	onApplyFilters: getMockFuncWithLogging('onApplyFilters'),
	onClose: getMockFuncWithLogging('onClose'),
};

export const FiltersModalUnitSelectedStory = Template.bind({});
FiltersModalUnitSelectedStory.storyName = "Unit selected";
FiltersModalUnitSelectedStory.args = {
	filter: getMockedReviewQueueFilter({
		unitId: mockedCourseSlidesInfo.units[0].id
	}),
	courseSlidesInfo: mockedCourseSlidesInfo,
	getStudents: mockedSearchStudents,
	getGroups: mockedSearchGroups,
	onApplyFilters: getMockFuncWithLogging('onApplyFilters'),
	onClose: getMockFuncWithLogging('onClose'),
};

export const FiltersModalFilterByStudentsStory = Template.bind({});
FiltersModalFilterByStudentsStory.storyName = "Filter by students";
FiltersModalFilterByStudentsStory.args = {
	filter: getMockedReviewQueueFilter({
		studentsFilter: StudentsFilter.StudentIds,
		students: mockedFilteredStudents.slice(0, 2)
	}),
	courseSlidesInfo: mockedCourseSlidesInfo,
	getStudents: mockedSearchStudents,
	getGroups: mockedSearchGroups,
	onApplyFilters: getMockFuncWithLogging('onApplyFilters'),
	onClose: getMockFuncWithLogging('onClose'),
};
export const FiltersModalFilterByGroupsStory = Template.bind({});
FiltersModalFilterByGroupsStory.storyName = "Filter by students";
FiltersModalFilterByGroupsStory.args = {
	filter: getMockedReviewQueueFilter({
		studentsFilter: StudentsFilter.GroupIds,
		groups: mockedFilteredGroups.slice(0, 2)
	}),
	courseSlidesInfo: mockedCourseSlidesInfo,
	getStudents: mockedSearchStudents,
	getGroups: mockedSearchGroups,
	onApplyFilters: getMockFuncWithLogging('onApplyFilters'),
	onClose: getMockFuncWithLogging('onClose'),
};
