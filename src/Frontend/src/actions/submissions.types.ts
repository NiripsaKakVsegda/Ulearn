import {
	ADD,
	DELETE,
	EDIT,
	FAIL,
	FailAction,
	loadFail,
	loadStart,
	loadSuccess,
	START,
	SUCCESS,
} from "src/consts/actions";
import { ReviewCommentResponse, ReviewInfo, RunSolutionResponse } from "../models/exercise";
import { SubmissionsResponse } from "../models/instructor";

const submissions = "SUBMISSIONS";
const reviews = "REVIEWS";
const comment = "_COMMENT";
const enableManualChecking = "_ENABLE_MANUAL_CHECKING";
const score = "_SCORE";
const assignBot = "_ASSIGN_BOT";

const reviewsAdd = reviews + ADD;
const reviewsAddScore = reviews + ADD + score;
const reviewsEdit = reviews + EDIT;
const reviewsDelete = reviews + DELETE;
const reviewsAssignBot = reviews + assignBot;

const reviewAddComment = reviews + ADD + comment;
const reviewDeleteComment = reviews + DELETE + comment;

export const SUBMISSIONS_ADD_SUBMISSION = submissions + ADD + "_SUBMISSION";

export const SUBMISSIONS_LOAD_START = submissions + loadStart;
export const SUBMISSIONS_LOAD_SUCCESS = submissions + loadSuccess;
export const SUBMISSIONS_LOAD_FAIL = submissions + loadFail;

export const SUBMISSIONS_SET_NEXT_SUBMISSION_BUTTON_DISABLED = submissions + '_SET_NEXT_SUBMISSION_BUTTON_DISABLED';

export const SUBMISSIONS_ENABLE_MANUAL_CHECKING_START = submissions + enableManualChecking + START;
export const SUBMISSIONS_ENABLE_MANUAL_CHECKING_FAIL = submissions + enableManualChecking + FAIL;

export const REVIEWS_ADD_START = reviewsAdd + START;
export const REVIEWS_ADD_SUCCESS = reviewsAdd + SUCCESS;
export const REVIEWS_ADD_FAIL = reviewsAdd + FAIL;

export const REVIEWS_ADD_SCORE_START = reviewsAddScore + START;
export const REVIEWS_ADD_SCORE_FAIL = reviewsAddScore + FAIL;

export const REVIEWS_EDIT_START = reviewsEdit + START;
export const REVIEWS_EDIT_SUCCESS = reviewsEdit + SUCCESS;
export const REVIEWS_EDIT_FAIL = reviewsEdit + FAIL;

export const REVIEWS_DELETE_START = reviewsDelete + START;
export const REVIEWS_DELETE_SUCCESS = reviewsDelete + SUCCESS;
export const REVIEWS_DELETE_FAIL = reviewsDelete + FAIL;

export const REVIEWS_ASSIGN_BOT_START = reviewsAssignBot + START;
export const REVIEWS_ASSIGN_BOT_SUCCESS = reviewsAssignBot + SUCCESS;
export const REVIEWS_ASSIGN_BOT_FAIL = reviewsAssignBot + FAIL;

export const REVIEWS_ADD_COMMENT_START = reviewAddComment + START;
export const REVIEWS_ADD_COMMENT_SUCCESS = reviewAddComment + SUCCESS;
export const REVIEWS_ADD_COMMENT_FAIL = reviewAddComment + FAIL;

export const REVIEWS_DELETE_COMMENT_START = reviewDeleteComment + START;
export const REVIEWS_DELETE_COMMENT_SUCCESS = reviewDeleteComment + SUCCESS;
export const REVIEWS_DELETE_COMMENT_FAIL = reviewDeleteComment + FAIL;


export interface SubmissionsAddSubmissionAction {
	type: typeof SUBMISSIONS_ADD_SUBMISSION;
	courseId: string;
	slideId: string;
	userId: string;
	result: RunSolutionResponse;
}

export interface SubmissionsLoadStartAction {
	type: typeof SUBMISSIONS_LOAD_START;
	userId: string;
	courseId: string;
	slideId: string;
}

export interface SubmissionsLoadSuccessAction {
	type: typeof SUBMISSIONS_LOAD_SUCCESS;
	userId: string;
	courseId: string;
	slideId: string;
	response: SubmissionsResponse;
}

export interface SubmissionsLoadFailAction extends FailAction {
	type: typeof SUBMISSIONS_LOAD_FAIL;
	userId: string;
	courseId: string;
	slideId: string;
}

export interface SubmissionsEnableManualCheckingStartAction {
	type: typeof SUBMISSIONS_ENABLE_MANUAL_CHECKING_START;
	submissionId: number;
}

export interface SubmissionsEnableManualCheckingFailAction extends FailAction {
	type: typeof SUBMISSIONS_ENABLE_MANUAL_CHECKING_START;
	submissionId: number;
}

export interface ReviewsAddCommentStartAction {
	type: typeof REVIEWS_ADD_COMMENT_START;
	submissionId: number;
	reviewId: number;
	text: string;
}

export interface ReviewsAddCommentSuccessAction {
	type: typeof REVIEWS_ADD_COMMENT_SUCCESS;
	submissionId: number;
	reviewId: number;
	comment: ReviewCommentResponse;
}

export interface ReviewsAddCommentFailAction extends FailAction {
	type: typeof REVIEWS_ADD_COMMENT_FAIL;
	submissionId: number;
	reviewId: number;
	text: string;
}

export interface ReviewsEditStartAction {
	type: typeof REVIEWS_EDIT_START;
	submissionId: number;
	reviewId: number;
	parentReviewId?: number;
	text: string;
}

export interface ReviewsEditSuccessAction {
	type: typeof REVIEWS_EDIT_SUCCESS;
	submissionId: number;
	reviewId: number;
	parentReviewId?: number;
	reviewOrComment: ReviewInfo | ReviewCommentResponse;
}

export interface ReviewsEditFailAction extends FailAction {
	type: typeof REVIEWS_EDIT_FAIL;
	submissionId: number;
	reviewId: number;
	parentReviewId?: number;
	oldText: string;
}

export interface ReviewsDeleteCommentStartAction {
	type: typeof REVIEWS_DELETE_COMMENT_START;
	submissionId: number;
	reviewId: number;
	commentId: number;
}

export interface ReviewsDeleteCommentSuccessAction {
	type: typeof REVIEWS_DELETE_COMMENT_SUCCESS;
	submissionId: number;
	reviewId: number;
	commentId: number;
}

export interface ReviewsDeleteCommentFailAction extends FailAction {
	type: typeof REVIEWS_DELETE_COMMENT_FAIL;
	submissionId: number;
	reviewId: number;
	commentId: number;
}

export interface ReviewsAddStartAction {
	type: typeof REVIEWS_ADD_START;
	submissionId: number;
	comment: string;
	startLine: number;
	startPosition: number;
	finishLine: number;
	finishPosition: number;
}

export interface ReviewsAddSuccessAction {
	type: typeof REVIEWS_ADD_SUCCESS;
	submissionId: number;
	review: ReviewInfo;
}

export interface ReviewsAddFailAction extends FailAction {
	type: typeof REVIEWS_ADD_FAIL;
	submissionId: number;
}

export interface ReviewsDeleteStartAction {
	type: typeof REVIEWS_DELETE_START;
	submissionId: number;
	reviewId: number;
	isBotReview?: boolean;
}

export interface ReviewsDeleteSuccessAction {
	type: typeof REVIEWS_DELETE_SUCCESS;
	submissionId: number;
	reviewId: number;
	isBotReview?: boolean;
}

export interface ReviewsDeleteFailAction extends FailAction {
	type: typeof REVIEWS_DELETE_FAIL;
	submissionId: number;
	reviewId: number;
	isBotReview?: boolean;
}

export interface ReviewsAddScoreStartAction {
	type: typeof REVIEWS_ADD_SCORE_START;
	submissionId: number;
	score: number;
}

export interface ReviewsAddScoreFailAction extends FailAction {
	type: typeof REVIEWS_ADD_SCORE_FAIL;
	submissionId: number;
	oldScore: number | null;
}

export interface ReviewsAssignBotStartAction {
	type: typeof REVIEWS_ASSIGN_BOT_START;
	submissionId: number;
	botReviewId: number;
}

export interface ReviewsAssignBotSuccessAction {
	type: typeof REVIEWS_ASSIGN_BOT_SUCCESS;
	submissionId: number;
	botReviewId: number;
	review: ReviewInfo;
}

export interface ReviewsAssignBotFailAction extends FailAction {
	type: typeof REVIEWS_ASSIGN_BOT_FAIL;
	submissionId: number;
	botReviewId: number;
}

export interface SubmissionSetNextSubmissionButtonDisabled {
	type: typeof SUBMISSIONS_SET_NEXT_SUBMISSION_BUTTON_DISABLED;
	disabled: boolean;
}


export type SubmissionsAction =
	SubmissionsAddSubmissionAction

	| SubmissionsLoadStartAction
	| SubmissionsLoadSuccessAction
	| SubmissionsLoadFailAction

	| SubmissionsEnableManualCheckingStartAction
	| SubmissionsEnableManualCheckingFailAction

	| ReviewsAddScoreStartAction
	| ReviewsAddScoreFailAction

	| ReviewsAddStartAction
	| ReviewsAddSuccessAction
	| ReviewsAddFailAction

	| ReviewsEditStartAction
	| ReviewsEditSuccessAction
	| ReviewsEditFailAction

	| ReviewsDeleteStartAction
	| ReviewsDeleteSuccessAction
	| ReviewsDeleteFailAction

	| ReviewsAddCommentStartAction
	| ReviewsAddCommentSuccessAction
	| ReviewsAddCommentFailAction

	| ReviewsDeleteCommentStartAction
	| ReviewsDeleteCommentSuccessAction
	| ReviewsDeleteCommentFailAction

	| ReviewsAssignBotStartAction
	| ReviewsAssignBotSuccessAction
	| ReviewsAssignBotFailAction

	| SubmissionSetNextSubmissionButtonDisabled
	;

