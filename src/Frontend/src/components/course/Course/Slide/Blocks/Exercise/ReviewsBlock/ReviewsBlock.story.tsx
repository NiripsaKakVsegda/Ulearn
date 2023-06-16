import React from "react";
import ReviewsBlock, {Props} from "./ReviewsBlock";

import type { Story } from "@storybook/react";
import { UserInfo } from "src/utils/courseRoles";
import { InstructorReviewInfoWithAnchor } from "../../../InstructorReview/InstructorReview.types";
import {getMockedUser} from "../../../../../../../storiesUtils";

const addingTime = "2020-12-03T20:03:29.9725057+05:00";

const author: UserInfo = getMockedUser({
	avatarUrl: "",
	firstName: "Name",
	id: "0",
	lastName: "LastName",
	visibleName: "Name LastName",
	login: undefined,
	email: undefined,
});

const authorWithLongName = {
	...author,
	firstName: "Всеволод",
	lastName: "Воронцов-Иванов",
	visibleName: "Всеволод Воронцов-Иванов",
};

const comment = {
	id: 0,
	text: "text **bold** __italic__ ```code```",
	renderedText: "text <b>bold</b> <i>italic</i> <pre>code</pre>",
	publishTime: addingTime,
	author: author
};

const review: InstructorReviewInfoWithAnchor = {
	id: 0,
	author: null,
	startLine: 10,
	startPosition: 5,
	finishLine: 10,
	finishPosition: 5,
	comment: "text **bold** __italic__ ```code```",
	renderedComment: "text <b>bold</b> <i>italic</i> <pre>code</pre>",
	addingTime: null,
	comments: [comment, { ...comment, id: 1 }],
	anchor: 0,
};

const teacherReview = {
	...review,
	author,
	addingTime,
};

const reviews = [
	review,
	{ ...teacherReview, id: 1 },
	{ ...teacherReview, id: 2 },
];

const props: Props = {
	reviews,
	selectedReviewId: -1,
	user: { ...author, id: '-1' },
	onSelectReview: () => void (0),
	onSendComment: () => void (0),
	onDeleteReviewOrComment: () => void (0),
	onEditReviewOrComment: () => void (0),
	onToggleReviewFavourite: () => void (0),
	onAssignBotComment: () => void (0),
};

const Template: Story<Props> = (args: Props) =>
	<div style={ { width: '260px', position: 'relative', display: 'flex', } }>
		<ReviewsBlock { ...args }/>
	</div>;

export const NothingSelected = Template.bind({});
NothingSelected.args = {
	...props,
};

export const FirstSelected = Template.bind({});
FirstSelected.args = {
	...props,
	selectedReviewId: 0,
};

export const SecondSelected = Template.bind({});
SecondSelected.args = {
	...props,
	selectedReviewId: 1,
};

export const ThirdSelected = Template.bind({});
ThirdSelected.args = {
	...props,
	selectedReviewId: 2,
};

export const UserCanDeleteComment = Template.bind({});
UserCanDeleteComment.args = {
	...props,
	user: author,
	selectedReviewId: 1,
};

export const SpaceAmongReviews = Template.bind({});
SpaceAmongReviews.args = {
	...props,
	reviews: [
		{ ...review, startLine: 0, finishLine: 0, },
		{ ...review, startLine: 20, finishLine: 20, },
	],
};

export const LinesCompare = Template.bind({});
LinesCompare.args = {
	...props,
	reviews: [
		{ ...review, startLine: 0, finishLine: 0, },
		{ ...review, startLine: 1, finishLine: 2, },
		{ ...review, startLine: 2, finishLine: 10, },
	],
};

export const WithLongNames = Template.bind({});
WithLongNames.args = {
	...props,
	reviews: [
		{ ...review, author: authorWithLongName, },
		{ ...review, author: authorWithLongName, },
		{ ...review, author: authorWithLongName, },
	],
};

export default {
	title: 'Exercise/Review',
	component: ReviewsBlock,
};
