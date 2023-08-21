import { ComponentMeta, ComponentStory } from "@storybook/react";
import ReviewQueuePage from "./ReviewQueuePage";
import {
	getMockedReviewQueueFilter,
	mockedCourseSlidesInfo,
	mockedReviewQueueItems,
	mockedSearchGroups,
	mockedSearchStudents
} from "./storyData";
import { useState } from "react";
import { ReviewQueueFilterState } from "./RevoewQueue.types";
import { fullscreenLayout } from "src/storiesUtils";

export default {
	title: "Review Queue/ReviewQueuePage",
	component: ReviewQueuePage,
	...fullscreenLayout
} as ComponentMeta<typeof ReviewQueuePage>;

const Template: ComponentStory<typeof ReviewQueuePage> = (args) => {
	const [filter, setFilter] = useState(args.filter);

	const updatedArgs = {
		...args,
		filter,
		onUpdateFilter: updateFilter,
	};

	return <ReviewQueuePage { ...updatedArgs } />;

	function updateFilter(filter: ReviewQueueFilterState) {
		setFilter(filter);
	}
};

export const ReviewQueuePageStory = Template.bind({});
ReviewQueuePageStory.storyName = "Default";
ReviewQueuePageStory.args = {
	reviewQueueItems: mockedReviewQueueItems,
	loading: false,
	filter: getMockedReviewQueueFilter(),
	courseSlidesInfo: mockedCourseSlidesInfo,
	userId: "userId",
	getStudents: mockedSearchStudents,
	getGroups: mockedSearchGroups,
	buildLinkToInstructorReview: () => "#",
};

export const ReviewQueuePageHistoryStory = Template.bind({});
ReviewQueuePageHistoryStory.storyName = "History";
ReviewQueuePageHistoryStory.args = {
	reviewQueueItems: mockedReviewQueueItems,
	loading: false,
	filter: getMockedReviewQueueFilter({
		reviewed: true
	}),
	courseSlidesInfo: mockedCourseSlidesInfo,
	userId: "userId",
	getStudents: mockedSearchStudents,
	getGroups: mockedSearchGroups,
	buildLinkToInstructorReview: () => "#",
};

export const ReviewQueuePageEmptyStory = Template.bind({});
ReviewQueuePageEmptyStory.storyName = "Empty";
ReviewQueuePageEmptyStory.args = {
	reviewQueueItems: [],
	loading: false,
	filter: getMockedReviewQueueFilter(),
	courseSlidesInfo: mockedCourseSlidesInfo,
	userId: "userId",
	getStudents: mockedSearchStudents,
	getGroups: mockedSearchGroups,
	buildLinkToInstructorReview: () => "#",
};

export const ReviewQueuePageNotAllLoadedStory = Template.bind({});
ReviewQueuePageNotAllLoadedStory.storyName = "Not all loaded";
ReviewQueuePageNotAllLoadedStory.args = {
	reviewQueueItems: mockedReviewQueueItems,
	notAllItemsLoaded: true,
	loading: false,
	filter: getMockedReviewQueueFilter(),
	courseSlidesInfo: mockedCourseSlidesInfo,
	userId: "userId",
	getStudents: mockedSearchStudents,
	getGroups: mockedSearchGroups,
	buildLinkToInstructorReview: () => "#",
};

export const ReviewQueuePageFetchingStory = Template.bind({});
ReviewQueuePageFetchingStory.storyName = "Fetching";
ReviewQueuePageFetchingStory.args = {
	reviewQueueItems: mockedReviewQueueItems,
	loading: true,
	filter: getMockedReviewQueueFilter(),
	courseSlidesInfo: mockedCourseSlidesInfo,
	userId: "userId",
	getStudents: mockedSearchStudents,
	getGroups: mockedSearchGroups,
	buildLinkToInstructorReview: () => "#",
};
