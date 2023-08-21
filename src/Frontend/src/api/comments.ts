import api from "./index";

import { buildQuery } from "src/utils";

import { commentPoliciesPath, commentsPath, constructPathToComment } from "src/consts/routes";
import { Comment, CommentPolicyResponse, SlideCommentsResponse, } from "src/models/comments";
import { Dispatch } from "redux";
import {
	commentAddedAction,
	commentDeletedAction,
	commentLikeUpdatedAction,
	commentsAction,
	commentsPolicyAction,
	commentUpdatedAction,
} from "src/actions/comments";
import {
	CommentAddedAction,
	CommentDeletedAction,
	CommentLikeUpdatedAction,
	CommentsLoadedAction,
	CommentsPolicyAction,
	CommentUpdatedAction
} from "src/actions/comments.types";

export function getComments(courseId: string, slideId: string, forInstructors: boolean, offset?: number,
	count?: number
): (dispatch: Dispatch) => Promise<CommentsLoadedAction> {
	const query = buildQuery({ courseId, slideId, forInstructors, count, offset });
	const url = commentsPath + query;

	return (dispatch: Dispatch): Promise<CommentsLoadedAction> => {
		return api.get<SlideCommentsResponse>(url)
			.then(comments => dispatch(commentsAction(slideId, courseId, comments.topLevelComments, forInstructors)));
	};
}

export function addComment(courseId: string, slideId: string, text: string, parentCommentId?: number,
	forInstructors?: boolean
): (dispatch: Dispatch) => Promise<CommentAddedAction> {
	const query = buildQuery({ courseId });
	const url = commentsPath + query;
	const params = api.createRequestParams({
		slideId,
		text,
		parentCommentId,
		forInstructors,
	});


	return (dispatch: Dispatch): Promise<CommentAddedAction> => {
		return api
			.post<Comment>(url, params)
			.then(c => dispatch(commentAddedAction(courseId, slideId, c, forInstructors, parentCommentId,)));
	};
}

export function deleteComment(courseId: string, slideId: string, commentId: number, forInstructor: boolean,)
	: (dispatch: Dispatch) => Promise<CommentDeletedAction> {
	const url = constructPathToComment(commentId);

	return (dispatch: Dispatch): Promise<CommentDeletedAction> => {
		return api
			.delete(url)
			.then(() => dispatch(commentDeletedAction(courseId, slideId, commentId, forInstructor)));
	};
}

export function updateComment(
	commentId: number,
	updatedFields?: Pick<Partial<Comment>, "text" | "isApproved" | "isCorrectAnswer" | "isPinnedToTop">
)
	: (dispatch: Dispatch) => Promise<CommentUpdatedAction> {
	const url = constructPathToComment(commentId);
	const params = api.createRequestParams(updatedFields || {});

	return (dispatch: Dispatch): Promise<CommentUpdatedAction> => {
		return api
			.patch<Comment>(url, params)
			.then(c => dispatch(commentUpdatedAction(c)));
	};
}

export function likeComment(commentId: number)
	: (dispatch: Dispatch) => Promise<CommentLikeUpdatedAction> {
	const url = constructPathToComment(commentId, true);

	return (dispatch: Dispatch): Promise<CommentLikeUpdatedAction> => {
		return api.post(url)
			.then(() => dispatch(
				commentLikeUpdatedAction(commentId, true)));
	};
}

export function dislikeComment(commentId: number)
	: (dispatch: Dispatch) => Promise<CommentLikeUpdatedAction> {
	const url = constructPathToComment(commentId, true);

	return (dispatch: Dispatch): Promise<CommentLikeUpdatedAction> => {
		return api
			.delete(url)
			.then(() => dispatch(
				commentLikeUpdatedAction(commentId, false)));
	};
}

export function getCommentPolicy(courseId: string) {
	return (dispatch: Dispatch): Promise<CommentsPolicyAction> => {
		const query = buildQuery({ courseId });
		const url = commentPoliciesPath + query;

		return api.get<CommentPolicyResponse>(url)
			.then(p => dispatch(commentsPolicyAction(courseId, p)));
	};
}

