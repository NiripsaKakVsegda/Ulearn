import React from 'react';

import { AcceptedSolutionsModal, AcceptedSolutionsProps } from './AcceptedSolutions';
import type { Story } from "@storybook/react";

import { ViewportWrapper } from "../../../../../Navigation/stroies.data";
import {
	AcceptedSolution,
	AcceptedSolutionsResponse,
	LikedAcceptedSolutionsResponse
} from "../../../../../../../models/acceptedSolutions";
import { mockFunc, returnPromiseAfterDelay } from "../../../../../../../utils/storyMock";
import { AcceptedSolutionsApi } from "../../../../../../../api/acceptedSolutions";
import { Language } from "../../../../../../../consts/languages";
import { getMockedShortUser } from "../../../../../../comments/storiesData";

const Template: Story<AcceptedSolutionsProps> = (args: AcceptedSolutionsProps) => {
	return <ViewportWrapper>
		<AcceptedSolutionsModal { ...args } />
	</ViewportWrapper>;
};

const getAcceptedSolutionsApi = (promotedSolutions: AcceptedSolution[], randomLikedSolutions: AcceptedSolution[],
	newestSolutions: AcceptedSolution[], likedSolutions: AcceptedSolution[] | null, loadTime = 0,
): AcceptedSolutionsApi => {
	return {
		getAcceptedSolutions: (courseId: string, slideId: string) => {
			const acceptedSolutionsResponse: AcceptedSolutionsResponse = {
				promotedSolutions: promotedSolutions,
				randomLikedSolutions: randomLikedSolutions,
				newestSolutions: newestSolutions,
			};
			return returnPromiseAfterDelay(loadTime, acceptedSolutionsResponse);
		},
		getLikedAcceptedSolutions: (courseId: string, slideId: string, offset: number, count: number) => {
			if(likedSolutions == null) {
				throw new Error();
			}
			const likedSolutionsResponse: LikedAcceptedSolutionsResponse = { likedSolutions: likedSolutions };
			return returnPromiseAfterDelay(loadTime, likedSolutionsResponse);
		},
		likeAcceptedSolution: (solutionId: number) => returnPromiseAfterDelay(loadTime, {} as Response),
		dislikeAcceptedSolution: (solutionId: number) => returnPromiseAfterDelay(loadTime, {} as Response),
		promoteAcceptedSolution: (solutionId: number) => returnPromiseAfterDelay(loadTime, {} as Response),
		unpromoteAcceptedSolution: (solutionId: number) => returnPromiseAfterDelay(loadTime, {} as Response),
	};
};

const as: AcceptedSolution = {
	submissionId: 1,
	code: "var a = 1\nvar a = 1\nvar a = 1\nvar a = 1",
	language: Language.cSharp,
	likesCount: 1,
	likedByMe: true,
	promotedBy: getMockedShortUser({})
};

const as2: AcceptedSolution = {
	submissionId: 2,
	code: "var a = 2\nvar a = 2\nvar a = 2\nvar a = 2",
	language: Language.cSharp,
	likesCount: 10000,
	likedByMe: false,
};

const longLinesSolution: AcceptedSolution = {
	submissionId: 4,
	code: "var a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;a=2;",
	language: Language.javaScript,
	likesCount: 3,
	likedByMe: false,
};

export const instructor = Template.bind({});
instructor.args = {
	courseId: "",
	slideId: "",
	isInstructor: true,
	user: getMockedShortUser({}),
	onClose: mockFunc,
	acceptedSolutionsApi: getAcceptedSolutionsApi([as], [longLinesSolution], [as2], [as2]),
};

export const studentWithPromoted = Template.bind({});
studentWithPromoted.args = {
	courseId: "",
	slideId: "",
	isInstructor: false,
	user: getMockedShortUser({}),
	onClose: mockFunc,
	acceptedSolutionsApi: getAcceptedSolutionsApi([as2], [longLinesSolution], [as], null),
};

export const student = Template.bind({});
student.args = {
	courseId: "",
	slideId: "",
	isInstructor: false,
	user: getMockedShortUser({}),
	onClose: mockFunc,
	acceptedSolutionsApi: getAcceptedSolutionsApi([], [longLinesSolution], [as], null),
};

export default {
	title: "Exercise/AcceptedSolutions",
	component: AcceptedSolutionsModal,
};
