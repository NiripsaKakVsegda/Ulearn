import React from 'react';

import ExerciseSelfChecking, { ExerciseSelfCheckingProps } from './ExerciseSelfChecking';
import {
	buildListTemplate,
	GetMock,
	mock,
} from "src/storiesUtils";

const ListTemplate = buildListTemplate<ExerciseSelfCheckingProps>(
	(props) => <ExerciseSelfChecking { ...props } />
);
export const Default = ListTemplate.bind({});


const submissionWithAutomaticChecking = GetMock.OfSubmission
	.withMockedAutomaticChecking()
	.submission;
const submissionWithManualChecking = GetMock.OfSubmission
	.withMockedManualChecking()
	.submission;
const submissionWithAllChecking = GetMock.OfSubmission
	.withMockedAutomaticChecking()
	.withMockedManualChecking()
	.submission;

const args = [{
	title: 'With automatic reviews',
	props: {
		showFirstComment: mock,
		showFirstBotComment: mock,
		lastSubmission: submissionWithAutomaticChecking,
		lastSubmissionWithReview: submissionWithAutomaticChecking,
		onCheckupClick: mock,
		checkups: GetMock.OfCheckups.checkups,
	}
}, {
	title: 'With automatic reviews and all checked',
	props: {
		showFirstComment: mock,
		showFirstBotComment: mock,
		lastSubmission: submissionWithAutomaticChecking,
		lastSubmissionWithReview: submissionWithAutomaticChecking,
		onCheckupClick: mock,
		checkups: GetMock.OfCheckups.withAllSelected().checkups,
	}
}, {
	title: 'With manual reviews',
	props: {
		showFirstComment: mock,
		showFirstBotComment: mock,
		lastSubmission: submissionWithManualChecking,
		lastSubmissionWithReview: submissionWithManualChecking,
		onCheckupClick: mock,
		checkups: GetMock.OfCheckups.withManualReviewCheckup(submissionWithManualChecking).checkups,
	}
}, {
	title: 'With manual reviews and all checked',
	props: {
		showFirstComment: mock,
		showFirstBotComment: mock,
		lastSubmission: submissionWithManualChecking,
		lastSubmissionWithReview: submissionWithManualChecking,
		onCheckupClick: mock,
		checkups: GetMock.OfCheckups.withManualReviewCheckup(submissionWithManualChecking).withAllSelected().checkups,
	}
}, {
	title: 'With manual and automatic reviews',
	props: {
		showFirstComment: mock,
		showFirstBotComment: mock,
		lastSubmission: submissionWithAllChecking,
		lastSubmissionWithReview: submissionWithAllChecking,
		onCheckupClick: mock,
		checkups: GetMock.OfCheckups.withManualReviewCheckup(submissionWithAllChecking).checkups,
	}
}, {
	title: 'With manual and automatic reviews and all checked',
	props: {
		showFirstComment: mock,
		showFirstBotComment: mock,
		lastSubmission: submissionWithAllChecking,
		lastSubmissionWithReview: submissionWithAllChecking,
		onCheckupClick: mock,
		checkups: GetMock.OfCheckups.withManualReviewCheckup(submissionWithAllChecking).withAllSelected().checkups,
	}
}
];

Default.args = args;

export default {
	title: "Slide/Blocks/SelfChecking/Exercise",
	component: ExerciseSelfChecking,
};

