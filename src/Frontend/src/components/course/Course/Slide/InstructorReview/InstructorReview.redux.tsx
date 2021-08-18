import { connect } from "react-redux";

import InstructorReview from "./InstructorReview";

import { buildUserInfo } from "src/utils/courseRoles";

import { Dispatch } from "redux";
import { RootState } from "src/redux/reducers";
import api from "src/api";
import { getDataIfLoaded, ReduxData } from "src/redux";
import { ShortGroupInfo } from "src/models/comments";
import { ApiFromRedux, PropsFromRedux } from "./InstructorReview.types";
import { SubmissionInfo } from "src/models/exercise";
import { SlideContext } from "../Slide.types";
import { getSubmissionsWithReviews } from "../../CourseUtils";

interface Props {
	slideContext: SlideContext;
}

const mapStateToProps = (
	state: RootState,
	{ slideContext: { courseId, slideId, slideInfo, } }: Props
): PropsFromRedux => {
	const studentId = slideInfo.query.userId;

	if(!studentId) {
		throw new Error('User id was not provided');
	}

	const student = getDataIfLoaded(state.instructor.studentsById[studentId]);

	const studentSubmissions: SubmissionInfo[] | undefined =
		getSubmissionsWithReviews(
			courseId,
			slideId,
			studentId,
			state.submissions.submissionsIdsByCourseIdBySlideIdByUserId,
			state.submissions.submissionsById,
			state.submissions.reviewsBySubmissionId
		);
	const submissionIdFromQuery = slideInfo.query.submissionId;

	if(submissionIdFromQuery == null) {
		throw new Error("Submission id was not provided in query");
	}

	const scoresBySubmissionId = state.submissions.reviewScoresByUserIdBySubmissionId[studentId];
	let studentGroups: ShortGroupInfo[] | undefined;
	const reduxGroups = getDataIfLoaded(state.groups.groupsIdsByUserId[studentId])
		?.map(groupId => getDataIfLoaded(state.groups.groupById[groupId]));
	if(reduxGroups && reduxGroups.every(g => g !== undefined)) {
		studentGroups = reduxGroups.map(g => ({ ...g, courseId, })) as ShortGroupInfo[];
	}
	const favouriteReviews = getDataIfLoaded(
		state.favouriteReviews.favouritesReviewsByCourseIdBySlideId[courseId]?.[slideId]);

	const antiPlagiarismStatus = studentSubmissions &&
		state.instructor.antiPlagiarismStatusBySubmissionId[studentSubmissions[0].id];

	const prohibitFurtherManualChecking = state.instructor
		.prohibitFurtherManualCheckingByCourseIdBySlideIdByUserId[courseId]
		?.[slideId]
		?.[studentId] || false;

	return {
		user: buildUserInfo(state.account, courseId,),
		favouriteReviews,
		studentGroups,
		student,
		studentSubmissions,
		antiPlagiarismStatus: getDataIfLoaded(antiPlagiarismStatus),
		antiPlagiarismStatusLoading: !!(antiPlagiarismStatus as ReduxData)?.isLoading,
		prohibitFurtherManualChecking,
		scoresBySubmissionId,
		submissionIdFromQuery,
	};
};

const mapDispatchToProps = (dispatch: Dispatch): ApiFromRedux => {
	return {
		addReview: (
			submissionId: number,
			text: string,
			startLine: number, startPosition: number,
			finishLine: number, finishPosition: number
		) =>
			api.submissions.redux
				.addReview(submissionId, text, startLine, startPosition, finishLine, finishPosition)(dispatch),
		deleteReview: (submissionId, reviewId, isBotReview) =>
			api.submissions.redux.deleteReview(submissionId, reviewId, isBotReview)(dispatch),

		addReviewComment: (submissionId: number, reviewId: number, comment: string) =>
			api.submissions.redux.addReviewComment(submissionId, reviewId, comment)(dispatch),
		deleteReviewComment: (submissionId: number, reviewId: number, commentId: number) =>
			api.submissions.redux.deleteReviewComment(submissionId, reviewId, commentId)(dispatch),


		addFavouriteReview: (courseId: string, slideId: string, text: string) =>
			api.favouriteReviews.redux.addFavouriteReview(courseId, slideId, text)(dispatch),
		deleteFavouriteReview: (courseId: string, slideId: string, favouriteReviewId: number) =>
			api.favouriteReviews.redux.deleteFavouriteReview(courseId, slideId, favouriteReviewId)(dispatch),
		editReviewOrComment: (submissionId: number, reviewId: number, parentReviewId: number | undefined, text: string,
			oldText: string,
		) => api.submissions.redux.editReviewOrComment(submissionId, reviewId, parentReviewId, text, oldText,)(
			dispatch),

		prohibitFurtherReview: (courseId: string, slideId: string, userId: string, prohibit: boolean) =>
			api.instructor.redux.prohibitFurtherManualChecking(courseId, slideId, userId, prohibit)(dispatch),
		onScoreSubmit: (submissionId: number, userId: string, score: number, oldScore: number | undefined,) =>
			api.submissions.redux.submitReviewScore(submissionId, userId, score, oldScore)(dispatch),

		getStudentInfo: (studentId: string,) =>
			api.instructor.redux.getStudentInfo(studentId)(dispatch),
		getAntiPlagiarismStatus: (courseId: string, submissionId: number,) =>
			api.instructor.redux.getAntiPlagiarismStatus(courseId, submissionId)(dispatch),
		getFavouriteReviews: (courseId: string, slideId: string,) =>
			api.favouriteReviews.redux.getFavouriteReviews(courseId, slideId,)(dispatch),
		getStudentGroups: (courseId: string, userId: string,) =>
			api.groups.getCourseGroupsRedux(courseId, userId, true)(dispatch),
		enableManualChecking: (submissionId: number,) =>
			api.submissions.redux.enableManualChecking(submissionId)(dispatch),

		assignBotReview: (submissionId, review) =>
			api.submissions.redux.assignBotReview(submissionId, review)(dispatch),
	};
};

const Connected = connect(mapStateToProps, mapDispatchToProps)(InstructorReview);
export default Connected;
