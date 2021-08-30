import {
	COMMENT_ADDED,
	COMMENT_DELETED,
	COMMENT_UPDATED,
	COMMENTS_LOADED,
	COMMENTS_POLICY_LOADED,
	CommentsLoadedAction,
	CommentsPolicyAction,
	CommentUpdatedAction,
	CommentAddedAction,
	CommentDeletedAction, CommentLikeUpdatedAction, COMMENT_LIKE_UPDATED,
} from "./comments.types";
import { Comment, CommentPolicy, } from "src/models/comments";

export const commentsAction = (
	slideId: string,
	courseId: string,
	comments: Comment[],
	forInstructor: boolean,
): CommentsLoadedAction => ({
	type: COMMENTS_LOADED,
	slideId,
	courseId,
	comments,
	forInstructor,
});

export const commentsPolicyAction = (
	courseId: string,
	policy: CommentPolicy,
): CommentsPolicyAction => ({
	type: COMMENTS_POLICY_LOADED,
	courseId,
	policy,
});


export const commentUpdatedAction = (
	comment: Comment,
): CommentUpdatedAction => ({
	type: COMMENT_UPDATED,
	comment,
});

export const commentLikeUpdatedAction = (
	commentId: number,
	like: boolean,
): CommentLikeUpdatedAction => ({
	type: COMMENT_LIKE_UPDATED,
	commentId,
	like,
});

export const commentAddedAction = (
	courseId: string,
	slideId: string,
	comment: Comment,
	forInstructor?: boolean,
	parentCommentId?: number,
): CommentAddedAction => ({
	type: COMMENT_ADDED,
	comment,
	courseId,
	slideId,
	forInstructor,
	parentCommentId,
});

export const commentDeletedAction = (
	courseId: string,
	slideId: string,
	commentId: number,
	forInstructor: boolean,
): CommentDeletedAction => ({
	type: COMMENT_DELETED,
	commentId,
	courseId,
	slideId,
	forInstructor,
});
