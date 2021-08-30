import {
	COMMENT_ADDED,
	COMMENT_DELETED,
	COMMENT_LIKE_UPDATED,
	COMMENT_UPDATED, CommentAddedAction, CommentDeletedAction, CommentLikeUpdatedAction,
	COMMENTS_LOADED,
	COMMENTS_POLICY_LOADED,
	CommentsAction,
	CommentsLoadedAction,
	CommentsPolicyAction, CommentUpdatedAction,
} from 'src/actions/comments.types';
import { Comment, CommentPolicy, } from "src/models/comments";

export interface CommentRedux extends Omit<Comment, 'replies'> {
	isDeleted?: boolean;
	repliesIds?: number[],
}

export interface CommentsState {
	byIds: {
		[commentId: number]: CommentRedux,
	},
	byCourses: {
		[courseId: string]: {
			policy: CommentPolicy,
			bySlides: {
				[slideId: string]: {
					commentsIds: number[];
					instructorCommentsIds: number[];
				};
			}
		}
	}
}

const initialCommentsState: CommentsState = {
	byIds: {},
	byCourses: {},
};

export default function comments(state = initialCommentsState, action: CommentsAction): CommentsState {
	switch (action.type) {
		case COMMENTS_LOADED: {
			const { slideId, courseId, comments, forInstructor, } = action as CommentsLoadedAction;
			const newComments = { ...state.byIds };
			const newCommentsIds = [];
			for (const { id, replies, ...rest } of comments) {
				const repliesIds = [];
				for (const reply of replies) {
					repliesIds.push(reply.id);
					newComments[reply.id] = reply;
				}

				newComments[id] = { id, repliesIds, ...rest, };
				newCommentsIds.push(id);
			}

			if(forInstructor) {
				return {
					...state,
					byIds: newComments,
					byCourses: {
						[courseId]: {
							...state.byCourses[courseId],
							bySlides: {
								...state.byCourses[courseId]?.bySlides,
								[slideId]: {
									...state.byCourses[courseId]?.bySlides?.[slideId],
									instructorCommentsIds: newCommentsIds,
								}
							}
						},
					}
				};
			}

			return {
				...state,
				byIds: newComments,
				byCourses: {
					[courseId]: {
						...state.byCourses[courseId],
						bySlides: {
							...state.byCourses[courseId]?.bySlides,
							[slideId]: {
								...state.byCourses[courseId]?.bySlides?.[slideId],
								commentsIds: newCommentsIds,
							}
						}
					},
				}
			};
		}
		case COMMENTS_POLICY_LOADED: {
			const { courseId, policy, } = action as CommentsPolicyAction;

			return {
				...state,
				byCourses: {
					[courseId]: {
						...state.byCourses[courseId],
						policy,
					},
				}
			};
		}
		case COMMENT_UPDATED: {
			const { comment } = action as CommentUpdatedAction;

			return {
				...state,
				byIds: {
					...state.byIds,
					[comment.id]: { ...comment, repliesIds: state.byIds[comment.id].repliesIds },
				}
			};
		}
		case COMMENT_LIKE_UPDATED: {
			const { commentId, like, } = action as CommentLikeUpdatedAction;
			const comment = state.byIds[commentId];

			return {
				...state,
				byIds: {
					...state.byIds,
					[commentId]: {
						...comment,
						isLiked: like,
						likesCount: like ? comment.likesCount + 1 : comment.likesCount - 1,
					},
				}
			};
		}
		case COMMENT_DELETED: {
			const { commentId, } = action as CommentDeletedAction;

			const newByIds = { ...state.byIds };
			newByIds[commentId].isDeleted = true;

			return {
				...state,
				byIds: newByIds,
			};
		}
		case COMMENT_ADDED: {
			const { comment, courseId, slideId, forInstructor, parentCommentId, } = action as CommentAddedAction;

			if(parentCommentId) {
				const parentReplies = state.byIds[parentCommentId].repliesIds || [];
				return {
					...state,
					byIds: {
						...state.byIds,
						[parentCommentId]: {
							...state.byIds[parentCommentId],
							repliesIds: [...parentReplies, comment.id],
						},
						[comment.id]: comment,
					}
				};
			}

			const slideCommentsIds = { ...state.byCourses[courseId].bySlides[slideId] };
			if(forInstructor) {
				slideCommentsIds.instructorCommentsIds = [...slideCommentsIds.instructorCommentsIds, comment.id];
			} else {
				slideCommentsIds.commentsIds = [...slideCommentsIds.commentsIds, comment.id];
			}

			return {
				...state,
				byIds: {
					...state.byIds,
					[comment.id]: comment,
				},
				byCourses: {
					...state.byCourses,
					[courseId]: {
						...state.byCourses[courseId],
						bySlides: {
							...state.byCourses[courseId].bySlides,
							[slideId]: slideCommentsIds,
						}
					}
				}
			};
		}
		default:
			return state;
	}
}
