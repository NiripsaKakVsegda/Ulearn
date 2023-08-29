import api from "src/api/index";
import { buildQuery } from "src/utils";

import { Language } from "src/consts/languages";
import {
	ReviewCommentResponse,
	ReviewInfo,
	RunSolutionResponse,
	SolutionRunStatus,
	SubmissionInfo
} from "src/models/exercise";
import { Dispatch } from "redux";
import { userProgressUpdateAction } from "../actions/userProgress";
import {
	addSubmissionAction,
	reviewsAddCommentFailAction,
	reviewsAddCommentStartAction,
	reviewsAddCommentSuccessAction,
	reviewsAddFailAction,
	reviewsAddScoreFail,
	reviewsAddScoreStart,
	reviewsAddStartAction,
	reviewsAddSuccessAction,
	reviewsAssignBotReviewFail,
	reviewsAssignBotReviewStart,
	reviewsAssignBotReviewSuccess,
	reviewsDeleteCommentFail,
	reviewsDeleteCommentStart,
	reviewsDeleteCommentSuccess,
	reviewsDeleteFailAction,
	reviewsDeleteStartAction,
	reviewsDeleteSuccessAction,
	reviewsEditFailAction,
	reviewsEditStartAction,
	reviewsEditSuccessAction,
	submissionsEnableManualCheckingFailAction,
	submissionsEnableManualCheckingStartAction,
	submissionsLoadFailAction,
	submissionsLoadStartAction,
	submissionsLoadSuccessAction,
} from "../actions/submissions";

import { reviews, submissions } from "../consts/routes";
import { SubmissionsResponse } from "../models/instructor";
import { studentProhibitFurtherManualCheckingLoadAction } from "../actions/instructor";
import renderSimpleMarkdown from "../utils/simpleMarkdownRender";
import { ShortUserInfo } from "../models/users";


export function submitCode(courseId: string, slideId: string, code: string,
	language: Language
): Promise<RunSolutionResponse> {
	const query = buildQuery({ language }) || "";
	return api.post<RunSolutionResponse>(
		`slides/${ courseId }/${ slideId }/exercise/submit` + query,
		api.createRequestParams({ solution: code })
	);
}

export function submitReviewScore(submissionId: number, percent: number
): Promise<void> {
	const url = `${ submissions }/${ submissionId }/score` + buildQuery({ percent });
	return api.post(url);
}

export function addReviewComment(reviewId: number, text: string): Promise<ReviewCommentResponse> {
	return api.post(
		`${ reviews }/${ reviewId }/comments`,
		api.createRequestParams({ text })
	);
}

export function deleteReviewComment(reviewId: number, commentId: number): Promise<Response> {
	return api.delete(`${ reviews }/${ reviewId }/comments/${ commentId }`
	);
}

export function getUserSubmissions(
	userId: string,
	courseId: string,
	slideId: string,
): Promise<SubmissionsResponse> {
	const url = submissions + buildQuery({ userId, courseId, slideId, });
	return api.get<SubmissionsResponse>(url);
}

export function addReview(
	submissionId: number,
	text: string,
	startLine: number, startPosition: number,
	finishLine: number, finishPosition: number
): Promise<ReviewInfo> {
	const url = reviews + buildQuery({ submissionId });
	return api.post<ReviewInfo>(
		url,
		api.createRequestParams({ text, startLine, startPosition, finishLine, finishPosition })
	);
}

export function editReviewOrComment(
	submissionId: number,
	reviewId: number,
	parentReviewId: number | undefined,
	text: string,
): Promise<ReviewInfo | ReviewCommentResponse> {
	const url = parentReviewId
		? `${ reviews }/${ parentReviewId }/comments/${ reviewId }`
		: reviews + buildQuery({ submissionId, reviewId, });
	return api.patch<ReviewInfo>(url, api.createRequestParams({ text }));
}

export function deleteReview(
	submissionId: number,
	reviewId: number,
): Promise<Response> {
	const url = reviews + buildQuery({ submissionId, reviewId, });
	return api.delete(url);
}

export function enableManualChecking(
	submissionId: number,
): Promise<Response> {
	const url = `${ submissions }/${ submissionId }/manual-checking`;
	return api.post(url);
}

export function assignBotReview(submissionId: number, review: ReviewInfo): Promise<[ReviewInfo, Response]> {
	return Promise
		.all(
			[
				addReview(
					submissionId,
					review.comment,
					review.startLine,
					review.startPosition,
					review.finishLine,
					review.finishPosition
				),
				deleteReview(submissionId, review.id)
			]);
}

//REDUX

const submitCodeRedux = (
	courseId: string,
	slideId: string,
	code: string,
	language: Language,
	userId: string,
): (dispatch: Dispatch) => void => {
	return (dispatch: Dispatch) => {
		submitCode(courseId, slideId, code, language)
			.then(r => {
				dispatch(addSubmissionAction(courseId, slideId, userId, r));
				updateUserProgress(r, dispatch);
			})
			.catch(err => {
				const result: RunSolutionResponse = {
					solutionRunStatus: SolutionRunStatus.InternalServerError,
					message: err.message,
					submission: null,
				};
				dispatch(addSubmissionAction(courseId, slideId, userId, result));
			});
	};

	function updateUserProgress(r: RunSolutionResponse, dispatch: Dispatch) {
		let fieldsToUpdate = {};
		if(r.score != null) {
			fieldsToUpdate = { ...fieldsToUpdate, score: r.score };
		}
		if(r.waitingForManualChecking != null) {
			fieldsToUpdate = { ...fieldsToUpdate, waitingForManualChecking: r.waitingForManualChecking };
		}
		if(r.prohibitFurtherManualChecking != null) {
			fieldsToUpdate = {
				...fieldsToUpdate,
				prohibitFurtherManualChecking: r.prohibitFurtherManualChecking
			};
		}
		if(Object.keys(fieldsToUpdate).length > 0) {
			dispatch(userProgressUpdateAction(courseId, slideId, fieldsToUpdate));
		}
	}
};

const addReviewRedux = (
	submissionId: number,
	author: ShortUserInfo,
	comment: string,
	startLine: number, startPosition: number,
	finishLine: number, finishPosition: number
) => {
	return (dispatch: Dispatch): Promise<ReviewInfo | void> => {
		const review: ReviewInfo = {
			id: -1,
			comment,
			startLine,
			startPosition,
			finishLine,
			finishPosition,
			author,
			comments: [],
			addingTime: new Date().toDateString(),
			renderedComment: renderSimpleMarkdown(comment),
		};
		dispatch(reviewsAddStartAction(submissionId, review));
		return addReview(submissionId, comment, startLine, startPosition, finishLine, finishPosition)
			.then(review => {
				dispatch(reviewsAddSuccessAction(submissionId, review,));
				return review;
			})
			.catch(error => {
				dispatch(reviewsAddFailAction(submissionId, error.toString()));
				throw error;
			});
	};
};

const editReviewOrCommentRedux = (
	submissionId: number,
	reviewId: number,
	parentReviewId: number | undefined,
	text: string,
	oldText: string,
) => {
	return (dispatch: Dispatch): Promise<ReviewInfo | ReviewCommentResponse | string> => {
		dispatch(reviewsEditStartAction(submissionId, reviewId, parentReviewId, text));
		return editReviewOrComment(submissionId, reviewId, parentReviewId, text)
			.then(r => {
				dispatch(reviewsEditSuccessAction(submissionId, reviewId, parentReviewId, r));
				return r;
			})
			.catch(err => {
				dispatch(reviewsEditFailAction(submissionId, reviewId, parentReviewId, oldText, err));
				return err;
			});
	};
};

const deleteReviewRedux = (
	submissionId: number,
	reviewId: number,
	isBotReview?: boolean,
) => {
	return (dispatch: Dispatch): Promise<Response> => {
		dispatch(reviewsDeleteStartAction(submissionId, reviewId, isBotReview));
		return deleteReview(submissionId, reviewId)
			.then(review => {
				dispatch(reviewsDeleteSuccessAction(submissionId, reviewId, isBotReview));
				return review;
			})
			.catch(error => {
				dispatch(reviewsDeleteFailAction(submissionId, reviewId, error.toString(), isBotReview));
				return error;
			});
	};
};

const assignBotReviewRedux = (
	submissionId: number,
	botReview: ReviewInfo,
) => {
	return (dispatch: Dispatch): Promise<ReviewInfo> => {
		dispatch(reviewsAssignBotReviewStart(submissionId, botReview.id));
		return assignBotReview(submissionId, botReview)
			.then(([review]) => {
				dispatch(reviewsAssignBotReviewSuccess(submissionId, botReview.id, review));
				return review;
			})
			.catch(err => {
				dispatch(reviewsAssignBotReviewFail(submissionId, botReview.id, err));
				return err;
			});
	};
};

const addReviewCommentRedux = (
	submissionId: number,
	reviewId: number,
	text: string,
) => {
	return (dispatch: Dispatch): Promise<ReviewCommentResponse | void> => {
		dispatch(reviewsAddCommentStartAction(submissionId, reviewId, text));

		return addReviewComment(reviewId, text,)
			.then(r => {
				dispatch(reviewsAddCommentSuccessAction(submissionId, reviewId, r));
				return r;
			})
			.catch(err => {
				dispatch(reviewsAddCommentFailAction(submissionId, reviewId, text, err));
				throw err;
			});
	};
};

const deleteReviewCommentRedux = (
	submissionId: number,
	reviewId: number,
	commentId: number,
) => {
	return (dispatch: Dispatch): Promise<Response> => {
		dispatch(reviewsDeleteCommentStart(submissionId, reviewId, commentId));

		return deleteReviewComment(reviewId, commentId,)
			.then(r => {
				dispatch(reviewsDeleteCommentSuccess(submissionId, reviewId, commentId));
				return r;
			})
			.catch(err => {
				dispatch(reviewsDeleteCommentFail(submissionId, reviewId, commentId, err));
				return err;
			});
	};
};

const getUserSubmissionsRedux = (userId: string, courseId: string, slideId: string,) => {
	return (dispatch: Dispatch): Promise<SubmissionInfo[] | string> => {
		dispatch(submissionsLoadStartAction(userId, courseId, slideId,));
		return getUserSubmissions(userId, courseId, slideId)
			.then(json => {
				dispatch(submissionsLoadSuccessAction(userId, courseId, slideId, json));
				dispatch(studentProhibitFurtherManualCheckingLoadAction(
					courseId,
					slideId,
					userId,
					json.prohibitFurtherManualChecking
				));
				return json.submissions;
			})
			.catch(error => {
				dispatch(submissionsLoadFailAction(userId, courseId, slideId, error.toString()));
				return error;
			});
	};
};

export function submitReviewScoreRedux(submissionId: number, percent: number,
	oldScore: number | null,
) {
	return (dispatch: Dispatch): Promise<Response | void> => {
		dispatch(reviewsAddScoreStart(submissionId, percent));
		return api.submissions.submitReviewScore(submissionId, percent)
			.catch(err => {
				dispatch(reviewsAddScoreFail(submissionId, oldScore, err));
				throw err;
			});
	};
}

const enableManualCheckingRedux = (
	submissionId: number,
) => {
	return (dispatch: Dispatch): Promise<Response | string> => {
		dispatch(submissionsEnableManualCheckingStartAction(submissionId));
		return enableManualChecking(submissionId)
			.catch(error => {
				dispatch(submissionsEnableManualCheckingFailAction(submissionId, error.toString()));
				return error;
			});
	};
};

export const redux = {
	submitCode: submitCodeRedux,
	submitReviewScore: submitReviewScoreRedux,

	addReview: addReviewRedux,
	editReviewOrComment: editReviewOrCommentRedux,
	deleteReview: deleteReviewRedux,

	assignBotReview: assignBotReviewRedux,

	addReviewComment: addReviewCommentRedux,
	deleteReviewComment: deleteReviewCommentRedux,

	getUserSubmissions: getUserSubmissionsRedux,

	enableManualChecking: enableManualCheckingRedux,
};
