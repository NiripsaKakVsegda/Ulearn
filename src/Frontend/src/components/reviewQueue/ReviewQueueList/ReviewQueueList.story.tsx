import { ComponentMeta, ComponentStory } from "@storybook/react";
import ReviewQueueList from "./ReviewQueueList";
import { mockedCourseSlidesInfo, mockedReviewQueueItems } from "../storyData";
import { Grouping } from "../RevoewQueue.types";
import { disableViewport } from "../../course/Navigation/stroies.data";

export default {
	title: "Review Queue/ReviewQueueList",
	component: ReviewQueueList,
	...disableViewport
} as ComponentMeta<typeof ReviewQueueList>;

const Template: ComponentStory<typeof ReviewQueueList> = (args) => (
	<ReviewQueueList { ...args } />
);

export const ReviewQueueListStory = Template.bind({});
ReviewQueueListStory.storyName = "Default";
ReviewQueueListStory.args = {
	reviewQueueItems: mockedReviewQueueItems,
	courseSlidesInfo: mockedCourseSlidesInfo,
	userId: 'userId',
	grouping: Grouping.NoGrouping,
	buildLinkToInstructorReview: () => '#',
};

export const ReviewQueueListGroupStudentsStory = Template.bind({});
ReviewQueueListGroupStudentsStory.storyName = "Group students";
ReviewQueueListGroupStudentsStory.args = {
	reviewQueueItems: mockedReviewQueueItems,
	courseSlidesInfo: mockedCourseSlidesInfo,
	userId: 'userId',
	grouping: Grouping.GroupStudents,
	buildLinkToInstructorReview: () => '#',
};

export const ReviewQueueListGroupSlidesStory = Template.bind({});
ReviewQueueListGroupSlidesStory.storyName = "Group slides";
ReviewQueueListGroupSlidesStory.args = {
	reviewQueueItems: mockedReviewQueueItems,
	courseSlidesInfo: mockedCourseSlidesInfo,
	userId: 'userId',
	grouping: Grouping.GroupExercises,
	buildLinkToInstructorReview: () => '#',
};

export const ReviewQueueListEmptyStory = Template.bind({});
ReviewQueueListEmptyStory.storyName = "Empty";
ReviewQueueListEmptyStory.args = {
	reviewQueueItems: [],
	courseSlidesInfo: mockedCourseSlidesInfo,
	userId: 'userId',
	grouping: Grouping.NoGrouping,
	buildLinkToInstructorReview: () => '#',
};
