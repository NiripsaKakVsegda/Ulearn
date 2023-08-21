import { ComponentMeta, ComponentStory } from "@storybook/react";
import ReviewQueueHistoryList from "./ReviewQueueHistoryList";
import { disableViewport } from "../../course/Navigation/stroies.data";
import { mockedCourseSlidesInfo, mockedReviewQueueItems } from "../storyData";

export default {
	title: "Review Queue/History/ReviewQueueHistoryList",
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
};

export const ReviewQueueHistoryListShowCommentsStory = Template.bind({});
ReviewQueueHistoryListShowCommentsStory.storyName = "Show comments";
ReviewQueueHistoryListShowCommentsStory.args = {
	reviewQueueItems: mockedReviewQueueItems,
	courseSlidesInfo: mockedCourseSlidesInfo,
	showComments: true
};

export const ReviewQueueHistoryListEmptyStory = Template.bind({});
ReviewQueueHistoryListEmptyStory.storyName = "Empty";
ReviewQueueHistoryListEmptyStory.args = {
	reviewQueueItems: [],
	courseSlidesInfo: mockedCourseSlidesInfo,
	showComments: true
};
