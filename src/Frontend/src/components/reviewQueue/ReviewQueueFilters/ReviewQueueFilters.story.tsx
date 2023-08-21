import { ComponentMeta, ComponentStory } from "@storybook/react";
import ReviewQueueFilters from "./ReviewQueueFilters";
import { Grouping, ReviewQueueFilterState } from "../RevoewQueue.types";
import {
	getMockedReviewQueueFilter,
	mockedCourseSlidesInfo,
	mockedSearchGroups,
	mockedSearchStudents
} from "../storyData";
import { useState } from "react";
import { disableViewport } from "../../course/Navigation/stroies.data";

export default {
	title: "Review Queue/ReviewQueueFilters",
	component: ReviewQueueFilters,
	...disableViewport
} as ComponentMeta<typeof ReviewQueueFilters>;

const Template: ComponentStory<typeof ReviewQueueFilters> = (args) => {
	const [filter, setFilter] = useState(args.filter);
	const [grouping, setGrouping] = useState(args.grouping);
	const [showComments, setShowComments] = useState(args.showComments);

	const updatedArgs = {
		...args,
		filter,
		grouping,
		showComments,
		onUpdateFilter: updateFilter,
		onChangeGrouping: changeGrouping,
		onChangeShowComments: changeShowComments
	};
	return <ReviewQueueFilters { ...updatedArgs } />;

	function updateFilter(filter: ReviewQueueFilterState) {
		setFilter(filter);
	}

	function changeGrouping(value: Grouping) {
		setGrouping(value);
	}

	function changeShowComments(value: boolean) {
		setShowComments(value);
	}
};

export const ReviewQueueFiltersStory = Template.bind({});
ReviewQueueFiltersStory.storyName = "Default";
ReviewQueueFiltersStory.args = {
	filter: getMockedReviewQueueFilter(),
	courseSlidesInfo: mockedCourseSlidesInfo,
	grouping: Grouping.NoGrouping,
	getStudents: mockedSearchStudents,
	getGroups: mockedSearchGroups,
};

export const ReviewQueueFiltersHistoryStory = Template.bind({});
ReviewQueueFiltersHistoryStory.storyName = "History";
ReviewQueueFiltersHistoryStory.args = {
	filter: getMockedReviewQueueFilter({ reviewed: true }),
	courseSlidesInfo: mockedCourseSlidesInfo,
	showComments: false,
	getStudents: mockedSearchStudents,
	getGroups: mockedSearchGroups,
};
