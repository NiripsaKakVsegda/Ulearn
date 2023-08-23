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
import { Grouping } from "./RevoewQueue.types";
import { fullscreenLayout } from "src/storiesUtils";

export default {
	title: "Review Queue/ReviewQueuePage",
	component: ReviewQueuePage,
	...fullscreenLayout
} as ComponentMeta<typeof ReviewQueuePage>;

const Template: ComponentStory<typeof ReviewQueuePage> = (args) => {
	const [filter, setFilter] = useState(args.filter);
	const [grouping, setGrouping] = useState(args.grouping ?? Grouping.NoGrouping);

	return <ReviewQueuePage
		{ ...args }
		filter={filter}
		grouping={grouping}
		onUpdateFilter={setFilter}
		onChangeGrouping={setGrouping}

	/>;
};

export const ReviewQueuePageStory = Template.bind({});
ReviewQueuePageStory.storyName = "Default";
ReviewQueuePageStory.args = {
	reviewQueueItems: mockedReviewQueueItems,
	loading: false,
	filter: getMockedReviewQueueFilter(),
	grouping: Grouping.NoGrouping,
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

export const ReviewQueuePageLoadingStory = Template.bind({});
ReviewQueuePageLoadingStory.storyName = "Loading";
ReviewQueuePageLoadingStory.args = {
	reviewQueueItems: [],
	loading: true,
	filter: getMockedReviewQueueFilter(),
	courseSlidesInfo: mockedCourseSlidesInfo,
	userId: "userId",
	getStudents: mockedSearchStudents,
	getGroups: mockedSearchGroups,
	buildLinkToInstructorReview: () => "#",
};
