import {
	SUBMISSIONS_LOAD_FAIL,
	SUBMISSIONS_LOAD_START,
	SUBMISSIONS_LOAD_SUCCESS,
	SubmissionsLoadFailAction,
	SubmissionsLoadStartAction,
	SubmissionsLoadSuccessAction,

	SUBMISSIONS_ADD_SUBMISSION,
	SubmissionsAddSubmissionAction,

	REVIEWS_ADD_START,
	REVIEWS_ADD_SUCCESS,
	REVIEWS_ADD_FAIL,
	ReviewsAddStartAction,
	ReviewsAddSuccessAction,
	ReviewsAddFailAction,

	REVIEWS_EDIT_START,
	REVIEWS_EDIT_SUCCESS,
	REVIEWS_EDIT_FAIL,
	ReviewsEditStartAction,
	ReviewsEditSuccessAction,
	ReviewsEditFailAction,

	REVIEWS_DELETE_START,
	REVIEWS_DELETE_SUCCESS,
	REVIEWS_DELETE_FAIL,
	ReviewsDeleteStartAction,
	ReviewsDeleteSuccessAction,
	ReviewsDeleteFailAction,

	REVIEWS_ADD_COMMENT_START,
	REVIEWS_ADD_COMMENT_SUCCESS,
	REVIEWS_ADD_COMMENT_FAIL,
	ReviewsAddCommentStartAction,
	ReviewsAddCommentSuccessAction,
	ReviewsAddCommentFailAction,

	REVIEWS_DELETE_COMMENT_START,
	REVIEWS_DELETE_COMMENT_SUCCESS,
	REVIEWS_DELETE_COMMENT_FAIL,
	ReviewsDeleteCommentStartAction,
	ReviewsDeleteCommentSuccessAction,
	ReviewsDeleteCommentFailAction,
	ReviewsAddScoreStartAction,
	REVIEWS_ADD_SCORE_START,
	ReviewsAddScoreFailAction,
	REVIEWS_ADD_SCORE_FAIL,
} from "./submissions.types";
import { SubmissionsResponse } from "../models/instructor";
import { ReviewCommentResponse, ReviewInfo, RunSolutionResponse } from "../models/exercise";

export const addSubmissionAction = (
	courseId: string,
	slideId: string,
	userId: string,
	result: RunSolutionResponse
): SubmissionsAddSubmissionAction => ({
	type: SUBMISSIONS_ADD_SUBMISSION,
	courseId,
	slideId,
	userId,
	result,
});

export const submissionsLoadStartAction = (
	userId: string,
	courseId: string,
	slideId: string,
): SubmissionsLoadStartAction => ({
	type: SUBMISSIONS_LOAD_START,
	userId,
	courseId,
	slideId,
});
export const submissionsLoadSuccessAction = (
	userId: string,
	courseId: string,
	slideId: string,
	response: SubmissionsResponse,
): SubmissionsLoadSuccessAction => ({
	type: SUBMISSIONS_LOAD_SUCCESS,
	userId,
	courseId,
	slideId,
	response,
});
export const submissionsLoadFailAction = (
	userId: string,
	courseId: string,
	slideId: string,
	error: string,
): SubmissionsLoadFailAction => ({
	type: SUBMISSIONS_LOAD_FAIL,
	userId,
	courseId,
	slideId,
	error,
});

export const reviewsAddStartAction = (
	submissionId: number,
	comment: string,
	startLine: number,
	startPosition: number,
	finishLine: number,
	finishPosition: number,
): ReviewsAddStartAction => ({
	type: REVIEWS_ADD_START,
	submissionId,
	comment,
	startLine,
	startPosition,
	finishLine,
	finishPosition,
});
export const reviewsAddSuccessAction = (
	submissionId: number,
	review: ReviewInfo,
): ReviewsAddSuccessAction => ({
	type: REVIEWS_ADD_SUCCESS,
	submissionId,
	review,
});
export const reviewsAddFailAction = (
	submissionId: number,
	error: string,
): ReviewsAddFailAction => ({
	type: REVIEWS_ADD_FAIL,
	submissionId,
	error,
});

export const reviewsEditStartAction = (
	submissionId: number,
	reviewId: number,
	commentId: number | undefined,
	text: string,
): ReviewsEditStartAction => ({
	type: REVIEWS_EDIT_START,
	submissionId,
	reviewId,
	parentReviewId: commentId,
	text,
});
export const reviewsEditSuccessAction = (
	submissionId: number,
	reviewId: number,
	parentReviewId: number | undefined,
	reviewOrComment: ReviewInfo | ReviewCommentResponse,
): ReviewsEditSuccessAction => ({
	type: REVIEWS_EDIT_SUCCESS,
	submissionId,
	reviewId,
	parentReviewId,
	reviewOrComment,
});
export const reviewsEditFailAction = (
	submissionId: number,
	reviewId: number,
	parentReviewId: number | undefined,
	oldText: string,
	error: string,
): ReviewsEditFailAction => ({
	type: REVIEWS_EDIT_FAIL,
	submissionId,
	reviewId,
	parentReviewId,
	oldText,
	error,
});

export const reviewsDeleteStartAction = (
	submissionId: number,
	reviewId: number,
): ReviewsDeleteStartAction => ({
	type: REVIEWS_DELETE_START,
	submissionId,
	reviewId,
});
export const reviewsDeleteSuccessAction = (
	submissionId: number,
	reviewId: number,
): ReviewsDeleteSuccessAction => ({
	type: REVIEWS_DELETE_SUCCESS,
	submissionId,
	reviewId,
});
export const reviewsDeleteFailAction = (
	submissionId: number,
	reviewId: number,
	error: string,
): ReviewsDeleteFailAction => ({
	type: REVIEWS_DELETE_FAIL,
	submissionId,
	reviewId,
	error,
});

export const reviewsAddCommentStartAction = (
	submissionId: number,
	reviewId: number,
	text: string,
): ReviewsAddCommentStartAction => ({
	type: REVIEWS_ADD_COMMENT_START,
	submissionId,
	reviewId,
	text,
});
export const reviewsAddCommentSuccessAction = (
	submissionId: number,
	reviewId: number,
	comment: ReviewCommentResponse
): ReviewsAddCommentSuccessAction => ({
	type: REVIEWS_ADD_COMMENT_SUCCESS,
	submissionId,
	reviewId,
	comment,
});
export const reviewsAddCommentFailAction = (
	submissionId: number,
	reviewId: number,
	text: string,
	error: string
): ReviewsAddCommentFailAction => ({
	type: REVIEWS_ADD_COMMENT_FAIL,
	submissionId,
	reviewId,
	text,
	error,
});

export const reviewsDeleteCommentStart = (
	submissionId: number,
	reviewId: number,
	commentId: number,
): ReviewsDeleteCommentStartAction => ({
	type: REVIEWS_DELETE_COMMENT_START,
	submissionId,
	reviewId,
	commentId,
});
export const reviewsDeleteCommentSuccess = (
	submissionId: number,
	reviewId: number,
	commentId: number
): ReviewsDeleteCommentSuccessAction => ({
	type: REVIEWS_DELETE_COMMENT_SUCCESS,
	submissionId,
	reviewId,
	commentId,
});
export const reviewsDeleteCommentFail = (
	submissionId: number,
	reviewId: number,
	commentId: number,
	error: string
): ReviewsDeleteCommentFailAction => ({
	type: REVIEWS_DELETE_COMMENT_FAIL,
	submissionId,
	reviewId,
	commentId,
	error,
});

export const reviewsAddScoreStart = (
	submissionId: number,
	userId: string,
	score: number,
): ReviewsAddScoreStartAction => ({
	type: REVIEWS_ADD_SCORE_START,
	submissionId,
	userId,
	score
});
export const reviewsAddScoreFail = (
	submissionId: number,
	userId: string,
	oldScore: number | undefined,
	error: string,
): ReviewsAddScoreFailAction => ({
	type: REVIEWS_ADD_SCORE_FAIL,
	submissionId,
	userId,
	oldScore,
	error,
});


