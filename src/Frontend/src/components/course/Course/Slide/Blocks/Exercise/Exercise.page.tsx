import { connect } from "react-redux";
import { Dispatch } from "redux";

import Exercise, { FromReduxDispatch, FromReduxProps, Props } from './Exercise';

import { AutomaticExerciseCheckingResult as CheckingResult } from "src/models/exercise";
import { RootState } from "src/models/reduxState";

import api from "src/api";
import { skipExercise } from "src/actions/userProgress";

import { Language } from "src/consts/languages";
import { buildUserInfo } from "src/utils/courseRoles";
import { getSlideInfoById } from "../../../CourseUtils";
import { getSubmissionsWithReviews } from "../../InstructorReview/InstructorReview.redux";

const mapStateToProps = (state: RootState,
	{ slideContext: { slideId, courseId, }, }: Pick<Props, 'slideContext'>
): FromReduxProps => {
	const { account, userProgress, device, instructor, courses, } = state;
	const {
		submissionError,
		lastCheckingResponse,
	} = state.submissions;
	const slideInfo = getSlideInfoById(slideId, courses.fullCoursesInfo[courseId])!.current;
	const slideProgress = userProgress?.progress[courseId]?.[slideId] || {};

	const submissions = getSubmissionsWithReviews(
		courseId,
		slideId,
		account.id,
		state.submissions.submissionsIdsByCourseIdBySlideIdByUserId,
		state.submissions.submissionsById, state.submissions.reviewsBySubmissionId
	)?.filter((s, i, arr) =>
			(i === arr.length - 1)
			|| (!s.automaticChecking || s.automaticChecking.result === CheckingResult.RightAnswer));

	//newer is first
	submissions?.sort((s1, s2) => (new Date(s2.timestamp).getTime() - new Date(s1.timestamp).getTime()));

	return {
		isAuthenticated: account.isAuthenticated,
		submissions: submissions || [],
		submissionError,
		lastCheckingResponse: !(lastCheckingResponse && lastCheckingResponse.courseId === courseId && lastCheckingResponse.slideId === slideId) ? null : lastCheckingResponse,
		user: buildUserInfo(account, courseId),
		slideProgress,
		deviceType: device.deviceType,
		maxScore: slideInfo!.maxScore,
		forceInitialCode: !instructor.isStudentMode,
	};
};

const mapDispatchToProps = (dispatch: Dispatch): FromReduxDispatch => ({
	sendCode: (courseId: string, slideId: string, userId: string, code: string, language: Language,
	) => api.submissions.redux.submitCode(courseId, slideId, code, language, userId,)(dispatch),

	addReviewComment: (submissionId: number, reviewId: number,
		comment: string
	) => api.submissions.redux.addReviewComment(submissionId, reviewId, comment)(dispatch),

	deleteReviewComment: (submissionId: number, reviewId: number, commentId: number
	) => api.submissions.redux.deleteReviewComment(submissionId, reviewId, commentId)(dispatch),

	skipExercise: (courseId: string, slideId: string, onSuccess: () => void,
	) => skipExercise(courseId, slideId, onSuccess)(dispatch),
});

export default connect(mapStateToProps, mapDispatchToProps)(Exercise);
