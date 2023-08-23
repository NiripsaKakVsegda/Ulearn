import { ComponentMeta, ComponentStory } from "@storybook/react";
import ReviewQueueHistoryList from "./ReviewQueueHistoryList";
import { disableViewport } from "../../course/Navigation/stroies.data";
import { mockedCourseSlidesInfo, mockedReviewQueueItems } from "../storyData";
import { mockFunc } from "../../../utils/storyMock";

export default {
	title: "Review Queue/ReviewQueueHistoryList",
	component: ReviewQueueHistoryList,
	...disableViewport
} as ComponentMeta<typeof ReviewQueueHistoryList>;


const Template: ComponentStory<typeof ReviewQueueHistoryList> = (args) => (
	<ReviewQueueHistoryList { ...args } />
);

export const ReviewQueueHistoryListStory = Template.bind({});
ReviewQueueHistoryListStory.storyName = "Default";
ReviewQueueHistoryListStory.args = {
	reviewQueueItems: mockedReviewQueueItems,
	courseSlidesInfo: mockedCourseSlidesInfo,
	buildLinkToInstructorReview: mockFunc,
};

export const ReviewQueueHistoryListShowCommentsStory = Template.bind({});
ReviewQueueHistoryListShowCommentsStory.storyName = "Show comments";
ReviewQueueHistoryListShowCommentsStory.args = {
	reviewQueueItems: mockedReviewQueueItems,
	courseSlidesInfo: mockedCourseSlidesInfo,
	showComments: true,
	buildLinkToInstructorReview: mockFunc,
};

export const ReviewQueueHistoryListEmptyStory = Template.bind({});
ReviewQueueHistoryListEmptyStory.storyName = "Empty";
ReviewQueueHistoryListEmptyStory.args = {
	reviewQueueItems: [],
	courseSlidesInfo: mockedCourseSlidesInfo,
	showComments: true,
	buildLinkToInstructorReview: mockFunc,
};

export const ReviewQueueHistoryListMockedStory = Template.bind({});
ReviewQueueHistoryListMockedStory.storyName = "Mocked";
ReviewQueueHistoryListMockedStory.args = {
	reviewQueueItems: [],
	loading: true,
	courseSlidesInfo: mockedCourseSlidesInfo,
	showComments: true,
	buildLinkToInstructorReview: mockFunc,
};
