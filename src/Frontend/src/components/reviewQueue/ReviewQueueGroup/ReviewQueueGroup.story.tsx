import { ComponentMeta, ComponentStory } from "@storybook/react";
import ReviewQueueGroup from "./ReviewQueueGroup";
import { mockedCourseSlidesInfo, mockedReviewQueueItems } from "../storyData";
import { getSlideTitlesByIds } from "../utils/getSlideTitlesByIds";
import { disableViewport } from "../../course/Navigation/stroies.data";

export default {
	title: "Review Queue/ReviewQueueGroup",
	component: ReviewQueueGroup,
	...disableViewport
} as ComponentMeta<typeof ReviewQueueGroup>;

const Template: ComponentStory<typeof ReviewQueueGroup> = (args) => (
	<ReviewQueueGroup { ...args } />
);

export const ReviewQueueGroupStory = Template.bind({});
ReviewQueueGroupStory.storyName = "Default";
ReviewQueueGroupStory.args = {
	reviewQueueItems: mockedReviewQueueItems,
	slideTitlesByIds: getSlideTitlesByIds(mockedCourseSlidesInfo),
	userId: 'userId',
	buildLinkToInstructorReview: () => '#'
};

export const ReviewQueueGroupNoTitleStory = Template.bind({});
ReviewQueueGroupNoTitleStory.storyName = "No grouping";
ReviewQueueGroupNoTitleStory.args = {
	alwaysOpened: true,
	reviewQueueItems: mockedReviewQueueItems,
	slideTitlesByIds: getSlideTitlesByIds(mockedCourseSlidesInfo),
	userId: 'userId',
	buildLinkToInstructorReview: () => '#'
};

export const ReviewQueueGroupStudentGroupingStory = Template.bind({});
ReviewQueueGroupStudentGroupingStory.storyName = "Student grouping";
ReviewQueueGroupStudentGroupingStory.args = {
	title: mockedReviewQueueItems[0].user.visibleName,
	reviewQueueItems: mockedReviewQueueItems,
	slideTitlesByIds: getSlideTitlesByIds(mockedCourseSlidesInfo),
	userId: 'userId',
	noStudent: true,
	buildLinkToInstructorReview: () => '#'
};

export const ReviewQueueGroupSlideGroupingStory = Template.bind({});
ReviewQueueGroupSlideGroupingStory.storyName = "Slide grouping";
ReviewQueueGroupSlideGroupingStory.args = {
	title: mockedCourseSlidesInfo.units[0].slides[0].title,
	reviewQueueItems: mockedReviewQueueItems,
	slideTitlesByIds: getSlideTitlesByIds(mockedCourseSlidesInfo),
	userId: 'userId',
	noStudent: true,
	buildLinkToInstructorReview: () => '#'
};
