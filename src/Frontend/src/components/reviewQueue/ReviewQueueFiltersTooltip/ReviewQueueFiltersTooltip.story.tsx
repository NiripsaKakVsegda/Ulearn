import { ComponentMeta, ComponentStory } from "@storybook/react";
import ReviewQueueFiltersTooltip from "./ReviewQueueFiltersTooltip";
import { getMockedReviewQueueFilter, mockedCourseSlidesInfo } from "../storyData";
import { Grouping } from "../RevoewQueue.types";

export default {
	title: 'Review Queue/ReviewQueueFiltersTooltip',
	component: ReviewQueueFiltersTooltip
} as ComponentMeta<typeof ReviewQueueFiltersTooltip>;

const Template: ComponentStory<typeof ReviewQueueFiltersTooltip> = (args) => (
	<ReviewQueueFiltersTooltip { ...args } />
);

export const ReviewQueueFiltersTooltipStory = Template.bind({});
ReviewQueueFiltersTooltipStory.storyName = 'Default';
ReviewQueueFiltersTooltipStory.args = {
	filter: getMockedReviewQueueFilter(),
	grouping: Grouping.NoGrouping,
	courseSlidesInfo: mockedCourseSlidesInfo,
	pos: 'bottom left'
};
