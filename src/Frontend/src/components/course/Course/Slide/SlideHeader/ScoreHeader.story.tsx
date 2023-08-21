import React from "react";
import { Provider } from 'react-redux';
import type { Meta, Story } from "@storybook/react";
import configureStore from "redux-mock-store";

import { ScoreHeader, ScoreHeaderPropsFromRedux } from "./ScoreHeader";
import { RootState } from "src/models/reduxState";
import { SlideType } from "src/models/slide";
import { CourseRoleType } from "src/consts/accessType";
import { SlideInfo } from "../../CourseUtils";
import { getInstructorReviewFilterSearchParamsFromQuery } from "../../../../reviewQueue/utils/getFilterSearchParamsFromQuery";

const courseId = "courseId";
const slideId = "slideId";

const slideInfo: SlideInfo = {
	slideId,
	courseId,
	slideType: SlideType.Exercise,
	isLti: false,
	isReview: true,
	isNavigationVisible: false,
	query: {
		...getInstructorReviewFilterSearchParamsFromQuery(),
		queueSlideId: undefined,
		slideId: null,
		submissionId: 1,
		isLti: false,
		userId: null
	},
	deadLineInfo: null,
};

const ListTemplate: Story<{ items: { props: ScoreHeaderPropsFromRedux, header: string }[] }>
	= ({ items }) => {
	return <>
		{ items.map((item) =>
			<>
				<p>{ item.header }</p>
				<Provider store={ GetStore(item.props) }>
					<ScoreHeader { ...item.props } slideInfo={ slideInfo }/>
				</Provider>
			</>
		) }
	</>;
};

const reduxProps: ScoreHeaderPropsFromRedux = {
	slideId,
	courseId,
	showStudentSubmissions: false,
	scoreHeader: 10,
	isSkipped: false,
	waitingForManualChecking: false,
	prohibitFurtherManualChecking: false,
	maxScore: 50,
	hasReviewedSubmissions: false,
	anyAttemptsUsed: false,
};

export const AllHeaders = ListTemplate.bind({});
AllHeaders.args = {
	items: [
		{ props: { ...reduxProps, scoreHeader: 50 }, header: "Полный балл" },
		{ props: { ...reduxProps, scoreHeader: 0 }, header: "0 баллов" },
		{ props: { ...reduxProps, isSkipped: true }, header: "Пропущено" },
		{ props: { ...reduxProps, waitingForManualChecking: true }, header: "Ожидает ревью" },
		{
			props: { ...reduxProps, waitingForManualChecking: true, scoreHeader: 50 },
			header: "Ожидает ревью и полный балл"
		},
		{ props: { ...reduxProps, prohibitFurtherManualChecking: true }, header: "Ревью запрещено" },
		{
			props: { ...reduxProps, prohibitFurtherManualChecking: true, scoreHeader: 50 },
			header: "Ревью запрещено и полный балл"
		},
		{ props: { ...reduxProps, hasReviewedSubmissions: true }, header: "Прошел ревью, неполный балл" },
		{
			props: { ...reduxProps, hasReviewedSubmissions: true, scoreHeader: 50 },
			header: "Прошел ревью, полный балл"
		},
		//Instructor cases
		{
			props: { ...reduxProps, isSkipped: true, showStudentSubmissions: true },
			header: "Пропущено (преподаватель)"
		},
		{
			props: { ...reduxProps, scoreHeader: 50, showStudentSubmissions: true },
			header: "Полный балл (преподаватель)"
		},
		{ props: { ...reduxProps, scoreHeader: 0, showStudentSubmissions: true }, header: "0 баллов (преподаватель)" },
	]
};

function GetStore(reduxProps: ScoreHeaderPropsFromRedux) {
	const {
		scoreHeader,
		isSkipped,
		waitingForManualChecking,
		prohibitFurtherManualChecking,
		maxScore,
		hasReviewedSubmissions,
		showStudentSubmissions
	} = reduxProps;
	const state: DeepPartial<RootState> = {
		userProgress: {
			progress: {
				[courseId]: {
					[slideId]: {
						score: scoreHeader,
						isSkipped: isSkipped,
						waitingForManualChecking: waitingForManualChecking,
						prohibitFurtherManualChecking: prohibitFurtherManualChecking,
					}
				}
			}
		},
		courses: {
			fullCoursesInfo: {
				[courseId]: {
					units: [{
						slides: [{
							id: slideId,
							type: SlideType.Exercise,
							maxScore: maxScore
						}]
					}]
				}
			}
		},
		submissions: { submissionsById: { 1: { manualChecking: { percent: hasReviewedSubmissions ? 100 : null, } } } },
		account: {
			isSystemAdministrator: false,
			roleByCourse: { [courseId]: showStudentSubmissions ? CourseRoleType.instructor : CourseRoleType.student }
		}
	};
	const mockStore = configureStore();
	return mockStore(state);
}

export default {
	title: 'Slide/ScoreHeader',
	argTypes: {
		items: { table: { disable: true } },
	}
} as Meta;
