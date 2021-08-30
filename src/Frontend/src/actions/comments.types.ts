import { Comment, CommentPolicy, } from "src/models/comments";

const comments = 'COMMENTS';
export const COMMENTS_LOADED = comments + '_LOADED';
export const COMMENTS_POLICY_LOADED = comments + 'POLICY_LOADED';
const comment = 'COMMENT';
export const COMMENT_UPDATED = comment + '_UPDATED';
export const COMMENT_LIKE_UPDATED = comment + '_LIKE_UPDATED';
export const COMMENT_ADDED = comment + '_ADDED';
export const COMMENT_DELETED = comment + '_DELETED';

export interface CommentsLoadedAction {
	type: typeof COMMENTS_LOADED;
	comments: Comment[];
	slideId: string;
	courseId: string;
	forInstructor: boolean;
}

export interface CommentsPolicyAction {
	type: typeof COMMENTS_POLICY_LOADED;
	courseId: string;
	policy: CommentPolicy;
}

export interface CommentUpdatedAction {
	type: typeof COMMENT_UPDATED;
	comment: Comment;
}

export interface CommentLikeUpdatedAction {
	type: typeof COMMENT_LIKE_UPDATED;
	commentId: number;
	like: boolean;
}

export interface CommentAddedAction {
	type: typeof COMMENT_ADDED;
	comment: Comment;
	slideId: string;
	courseId: string;
	forInstructor?: boolean;
	parentCommentId?: number;
}

export interface CommentDeletedAction {
	type: typeof COMMENT_DELETED;
	slideId: string;
	courseId: string;
	commentId: number;
	forInstructor: boolean;
}

export type CommentsAction =
	CommentsLoadedAction
	| CommentsPolicyAction
	| CommentUpdatedAction
	| CommentAddedAction
	| CommentDeletedAction
	| CommentLikeUpdatedAction;
