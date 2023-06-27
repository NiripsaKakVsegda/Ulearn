import React, { PropsWithChildren, useEffect } from "react";

import api from "src/api";
import { connect } from "react-redux";

import { SlidePropsWithContext } from "./Slide.types";
import { SubmissionInfo } from "src/models/exercise";
import { RootState } from "src/redux/reducers";
import { getSubmissionsWithReviews, } from "../CourseUtils";
import { Dispatch } from "redux";

export type SubmissionsLoaderProps =
	SlidePropsWithContext
	& PropsFromRedux
	& DispatchFromRedux;

interface PropsFromRedux {
	submissions: SubmissionInfo[] | undefined;
	userId?: string | null;
	isSubmissionsLoading: boolean;
}

interface DispatchFromRedux {
	loadSubmissions: (userId: string, courseId: string, slideId: string) => unknown;
}

const SubmissionsLoader: React.FC<PropsWithChildren<SubmissionsLoaderProps>> = ({
	loadSubmissions,
	isSubmissionsLoading,
	submissions,
	userId,
	slideContext,
	children,
}) => {
	useEffect(() => {
		const { slideInfo, } = slideContext;
		const { slideId, courseId } = slideInfo;

		if(userId && slideId && !isSubmissionsLoading && !submissions) {
			loadSubmissions(userId, courseId, slideId);
		}
	}, [userId, slideContext, submissions, loadSubmissions, isSubmissionsLoading]);

	return (
		<>{ children }</>
	);
};

const mapStateToProps = (state: RootState, { slideContext: { slideInfo, } }: SlidePropsWithContext): PropsFromRedux => {
	const userId = slideInfo.isReview
		? slideInfo.query.userId
		: state.account.id;

	let submissions: SubmissionInfo[] | undefined =
		getSubmissionsWithReviews(
			slideInfo.courseId,
			slideInfo.slideId,
			userId,
			state.submissions.submissionsIdsByCourseIdBySlideIdByUserId,
			state.submissions.submissionsById,
			state.submissions.reviewsBySubmissionId
		);

	if(userId && submissions) {
		if(slideInfo.isReview && slideInfo.query.submissionId && !submissions.find(
			s => s.id === slideInfo.query.submissionId)) {
			submissions = undefined;
		}
	}

	return {
		userId,
		submissions,
		isSubmissionsLoading: !!(userId && state.submissions.submissionsLoadingForUser[userId]
			?.some(loading => loading.slideId === slideInfo.slideId && loading.courseId === slideInfo.courseId)),
	};
};

const mapDispatchToProps = (dispatch: Dispatch): DispatchFromRedux => {
	return {
		loadSubmissions: (userId: string, courseId: string, slideId: string) =>
			api.submissions.redux.getUserSubmissions(userId, courseId, slideId)(dispatch),
	};
};

export default connect(mapStateToProps, mapDispatchToProps)(SubmissionsLoader);
